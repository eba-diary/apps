using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetService : IGlobalDatasetService
    {
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IUserService _userService;
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetService(IGlobalDatasetProvider globalDatasetProvider, IUserService userService, IDataFeatures dataFeatures)
        {
            _globalDatasetProvider = globalDatasetProvider;
            _userService = userService;
            _dataFeatures = dataFeatures;
        }

        public async Task<SearchGlobalDatasetsResultsDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            List<GlobalDataset> globalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

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
    }
}
