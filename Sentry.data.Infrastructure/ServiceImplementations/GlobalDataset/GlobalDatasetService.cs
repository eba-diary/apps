using Sentry.data.Core;
using Sentry.data.Infrastructure.FeatureFlags;
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

        public async Task<SearchGlobalDatasetsResultDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "SearchGlobalDatasets");
            }

            List<GlobalDataset> globalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            string currentUserId = _userService.GetCurrentUser().AssociateId;
            SearchGlobalDatasetsResultDto resultDto = new SearchGlobalDatasetsResultDto
            {
                GlobalDatasets = globalDatasets.Select(x => x.ToSearchResult(currentUserId)).ToList()
            };

            return resultDto;
        }

        public async Task<List<FilterCategoryDto>> GetGlobalDatasetFiltersAsync(FilterSearchDto filterSearchDto)
        {
            return await _globalDatasetProvider.GetGlobalDatasetFiltersAsync(filterSearchDto);
        }
    }
}
