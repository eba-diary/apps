using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

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

            GetGlobalDatasetFiltersResultDto resultsDto = new GetGlobalDatasetFiltersResultDto
            {
                FilterCategories = await _globalDatasetProvider.GetGlobalDatasetFiltersAsync(getGlobalDatasetFiltersDto)
            };

            return resultsDto;
        }

        #region Private
        private async Task<List<GlobalDataset>> GetGlobalDatasetsWithColumnSearchAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto, Task<List<GlobalDataset>> globalDatasetsTask)
        {
            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                SearchText = searchGlobalDatasetsDto.SearchText,
                DatasetIds = new List<int>()
            };

            //only get a subset of dataset ids if search includes filters
            if (searchGlobalDatasetsDto.FilterCategories.Any())
            {
                HighlightableFilterSearchDto filterSearchDto = new HighlightableFilterSearchDto
                {
                    FilterCategories = searchGlobalDatasetsDto.FilterCategories,
                    UseHighlighting = false
                };

                List<GlobalDataset> globalDatasetsForColumnSearch = await _globalDatasetProvider.SearchGlobalDatasetsAsync(filterSearchDto);
                searchSchemaFieldsDto.DatasetIds = globalDatasetsForColumnSearch.SelectMany(x => x.EnvironmentDatasets.Select(d => d.DatasetId)).ToList();
            }

            //get columns that match search
            List<ElasticSchemaField> schemaFields = await _schemaFieldProvider.SearchSchemaFieldsAsync(searchSchemaFieldsDto);

            List<GlobalDataset> globalDatasets = await globalDatasetsTask;

            if (schemaFields.Any())
            {
                //only retrieve additional global datasets that we don't already have
                List<int> environmentDatasetIdsToGet = schemaFields.Where(x => !globalDatasets.Any(a => a.EnvironmentDatasets.Select(d => d.DatasetId).Contains(x.DatasetId))).Select(x => x.DatasetId).ToList();

                if (environmentDatasetIdsToGet.Any())
                {
                    List<GlobalDataset> columnGlobalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsByEnvironmentDatasetIdsAsync(environmentDatasetIdsToGet);
                    globalDatasets.AddRange(columnGlobalDatasets);
                }

                //add columns to search highlights
                foreach (ElasticSchemaField schemaField in schemaFields)
                {
                    GlobalDataset retrievedGlobalDataset = globalDatasets.FirstOrDefault(x => x.EnvironmentDatasets.Select(d => d.DatasetId).Contains(schemaField.DatasetId));
                    if (retrievedGlobalDataset != null)
                    {
                        retrievedGlobalDataset.AddSearchHighlight(SearchDisplayNames.SchemaField.COLUMNNAME, $"<em>{schemaField.Name}</em>");
                    }
                }
            }

            return globalDatasets;
        }
        #endregion
    }
}
