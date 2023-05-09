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
            //get only schemafields that match full search, highlighting, filter by dataset id if there are filter categories

            //get columns that match search
            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                SearchText = searchGlobalDatasetsDto.SearchText,
                DatasetIds = new List<int>()
            };

            //only get a subset of dataset ids if search includes filters
            if (searchGlobalDatasetsDto.FilterCategories.Any())
            {
                HighlightableFilterSearchDto additionalSearchDto = new HighlightableFilterSearchDto
                {
                    FilterCategories = searchGlobalDatasetsDto.FilterCategories,
                    UseHighlighting = false
                };

                List<GlobalDataset> globalDatasetsForColumnSearch = await _globalDatasetProvider.SearchGlobalDatasetsAsync(additionalSearchDto);
                searchSchemaFieldsDto.DatasetIds = globalDatasetsForColumnSearch.SelectMany(x => x.EnvironmentDatasets.Select(d => d.DatasetId)).ToList();
            }

            //get columns that match search
            List<ElasticSchemaField> schemaFields = await _schemaFieldProvider.SearchSchemaFieldsWithHighlightingAsync(searchSchemaFieldsDto);

            List<GlobalDataset> globalDatasets = await globalDatasetsTask;

            if (schemaFields.Any())
            {
                //only retrieve additional global datasets that we don't already have
                List<int> environmentDatasetIdsToGet = GetAdditionalEnvironmentDatasetIds(schemaFields, globalDatasets);

                if (environmentDatasetIdsToGet.Any())
                {
                    List<GlobalDataset> columnGlobalDatasets = await _globalDatasetProvider.GetGlobalDatasetsByEnvironmentDatasetIdsAsync(environmentDatasetIdsToGet);
                    globalDatasets.AddRange(columnGlobalDatasets);
                }

                //add columns to search highlights
                foreach (ElasticSchemaField schemaField in schemaFields)
                {
                    GlobalDataset retrievedGlobalDataset = globalDatasets.FirstOrDefault(x => x.EnvironmentDatasets.Select(d => d.DatasetId).Contains(schemaField.DatasetId));

                    if (retrievedGlobalDataset != null)
                    {
                        retrievedGlobalDataset.MergeSearchHighlights(schemaField.SearchHighlights);
                    }
                    else if (searchGlobalDatasetsDto.FilterCategories.Any())
                    {
                        //Have to match up the environment dataset id to the filter??
                        //is it even possible to get the correct filter highlight associated with the column 
                        //don't want when 2 filters are selected, both to show up in the highlights
                    }
                }
            }

            return globalDatasets;
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
