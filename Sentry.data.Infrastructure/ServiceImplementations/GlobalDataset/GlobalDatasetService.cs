using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetService : IGlobalDatasetService
    {
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly ISchemaFieldProvider _schemaFieldProvider;
        private readonly IUserService _userService;
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetService(IGlobalDatasetProvider globalDatasetProvider, ISchemaFieldProvider schemaFieldProvider, IUserService userService, IDataFeatures dataFeatures)
        {
            _globalDatasetProvider = globalDatasetProvider;
            _schemaFieldProvider = schemaFieldProvider;
            _userService = userService;
            _dataFeatures = dataFeatures;
        }

        public async Task<SearchGlobalDatasetsResultsDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            Task<List<GlobalDataset>> globalDatasetsTask = _globalDatasetProvider.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            List<GlobalDataset> globalDatasets;
                        
            if (searchGlobalDatasetsDto.ShouldSearchColumns && !string.IsNullOrWhiteSpace(searchGlobalDatasetsDto.SearchText))
            {
                globalDatasets = await GetGlobalDatasetsWithColumnSearchAsync(searchGlobalDatasetsDto, globalDatasetsTask);
            }
            else
            {
                globalDatasets = await globalDatasetsTask;
            }

            string currentUserId = _userService.GetCurrentUser().AssociateId;
            List<SearchGlobalDatasetDto> searchDtos = globalDatasets.ToSearchResults(currentUserId);

            SearchGlobalDatasetsResultsDto resultDto = new SearchGlobalDatasetsResultsDto
            {
                GlobalDatasets = searchDtos
            };

            return resultDto;
        }

        public async Task<GetGlobalDatasetFiltersResultDto> GetGlobalDatasetFiltersAsync(GetGlobalDatasetFiltersDto getGlobalDatasetFiltersDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            GetGlobalDatasetFiltersResultDto resultsDto = new GetGlobalDatasetFiltersResultDto();

            if (getGlobalDatasetFiltersDto.ShouldSearchColumns && !string.IsNullOrWhiteSpace(getGlobalDatasetFiltersDto.SearchText))
            {
                resultsDto.FilterCategories = await GetFilterCategoriesWithColumnSearchAsync(getGlobalDatasetFiltersDto);
            }
            else
            {
                resultsDto.FilterCategories = await _globalDatasetProvider.GetGlobalDatasetFiltersAsync(getGlobalDatasetFiltersDto);
            }

            return resultsDto;
        }

        #region Private
        private async Task<List<GlobalDataset>> GetGlobalDatasetsWithColumnSearchAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto, Task<List<GlobalDataset>> globalDatasetsTask)
        {
            List<GlobalDataset> filteredGlobalDatasets = await GetFilteredGlobalDatasetsAsync(searchGlobalDatasetsDto.FilterCategories);

            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                SearchText = searchGlobalDatasetsDto.SearchText,
                DatasetIds = filteredGlobalDatasets.SelectMany(x => x.EnvironmentDatasets.Select(d => d.DatasetId)).ToList()
            };

            //get columns that match search
            List<ElasticSchemaField> schemaFields = await _schemaFieldProvider.SearchSchemaFieldsWithHighlightingAsync(searchSchemaFieldsDto);

            List<GlobalDataset> globalDatasets = await globalDatasetsTask;

            if (schemaFields.Any())
            {
                await AddAdditionalGlobalDatasetsForMatchedColumns(globalDatasets, schemaFields, filteredGlobalDatasets);

                //add columns to search highlights
                foreach (ElasticSchemaField schemaField in schemaFields)
                {
                    GlobalDataset retrievedGlobalDataset = globalDatasets.FirstOrDefault(x => x.EnvironmentDatasets.Select(d => d.DatasetId).Contains(schemaField.DatasetId));
                    retrievedGlobalDataset?.MergeSearchHighlights(schemaField.SearchHighlights);
                }
            }

            return globalDatasets;
        }

        private async Task<List<GlobalDataset>> GetFilteredGlobalDatasetsAsync(List<FilterCategoryDto> filterCategories)
        {
            List<GlobalDataset> filteredGlobalDatasets = new List<GlobalDataset>();

            //only get a subset of dataset ids if search includes filters
            if (filterCategories.Any())
            {
                SearchGlobalDatasetsDto additionalSearchDto = new SearchGlobalDatasetsDto
                {
                    FilterCategories = filterCategories
                };

                filteredGlobalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsAsync(additionalSearchDto);
            }

            return filteredGlobalDatasets;
        }

        private async Task AddAdditionalGlobalDatasetsForMatchedColumns(List<GlobalDataset> globalDatasets, List<ElasticSchemaField> schemaFields, List<GlobalDataset> filteredGlobalDatasets)
        {
            //only retrieve additional global datasets that we don't already have
            List<int> environmentDatasetIdsToGet = GetAdditionalEnvironmentDatasetIds(schemaFields, globalDatasets);

            if (environmentDatasetIdsToGet.Any())
            {
                List<GlobalDataset> additionalGlobalDatasetsForColumns = await _globalDatasetProvider.GetGlobalDatasetsByEnvironmentDatasetIdsAsync(environmentDatasetIdsToGet);

                if (filteredGlobalDatasets.Any())
                {
                    foreach (GlobalDataset additionalGlobalDataset in additionalGlobalDatasetsForColumns)
                    {
                        GlobalDataset globalDatasetWithHighlight = filteredGlobalDatasets.FirstOrDefault(x => x.GlobalDatasetId == additionalGlobalDataset.GlobalDatasetId);
                        if (globalDatasetWithHighlight != null)
                        {
                            additionalGlobalDataset.MergeSearchHighlights(globalDatasetWithHighlight.SearchHighlights);
                        }
                    }
                }

                globalDatasets.AddRange(additionalGlobalDatasetsForColumns);
            }
        }

        private async Task<List<FilterCategoryDto>> GetFilterCategoriesWithColumnSearchAsync(GetGlobalDatasetFiltersDto getGlobalDatasetFiltersDto)
        {
            //get columns that match search
            Task<List<ElasticSchemaField>> schemaFieldsTask = _schemaFieldProvider.SearchSchemaFieldsAsync(getGlobalDatasetFiltersDto);

            //get filters without the matched columns
            Task<DocumentsFiltersDto<GlobalDataset>> documentsFiltersTask = _globalDatasetProvider.GetGlobalDatasetsAndFiltersAsync(getGlobalDatasetFiltersDto);

            List<ElasticSchemaField> schemaFields = await schemaFieldsTask;
            DocumentsFiltersDto<GlobalDataset> documentsFilters = await documentsFiltersTask;

            if (schemaFields.Any())
            {
                //only need to add additional filter categories if there are additional datasets not in original result set
                List<int> environmentDatasetIdsToGet = GetAdditionalEnvironmentDatasetIds(schemaFields, documentsFilters.Documents);

                if (environmentDatasetIdsToGet.Any())
                {
                    List<FilterCategoryDto> additionalFilterCategories = await _globalDatasetProvider.GetFiltersByEnvironmentDatasetIdsAsync(environmentDatasetIdsToGet);
                    documentsFilters.FilterCategories.MergeFilterCategories(additionalFilterCategories);
                }
            }

            return documentsFilters.FilterCategories;
        }

        private List<int> GetAdditionalEnvironmentDatasetIds(List<ElasticSchemaField> schemaFields, List<GlobalDataset> globalDatasets)
        {
            return schemaFields.Where(x => !globalDatasets.Any(a => a.EnvironmentDatasets.Select(d => d.DatasetId).Contains(x.DatasetId))).Select(x => x.DatasetId).Distinct().ToList();
        }
        #endregion
    }
}
