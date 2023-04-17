using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetService : IGlobalDatasetService
    {
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IUserService _userService;

        public GlobalDatasetService(IGlobalDatasetProvider globalDatasetProvider, IUserService userService)
        {
            _globalDatasetProvider = globalDatasetProvider;
            _userService = userService;
        }

        public async Task<SearchGlobalDatasetsResultDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto)
        {
            List<GlobalDataset> globalDatasets = await _globalDatasetProvider.SearchGlobalDatasetsAsync(searchGlobalDatasetsDto);

            string currentUserId = _userService.GetCurrentUser().AssociateId;
            SearchGlobalDatasetsResultDto resultDto = new SearchGlobalDatasetsResultDto
            {
                GlobalDatasets = globalDatasets.Select(x => x.ToSearchResult(currentUserId)).ToList()
            };

            return resultDto;
        }

        public FilterSearchDto GetFiltersForGlobalDatasets(FilterSearchDto filterSearchDto)
        {
            throw new NotImplementedException();
        }
    }
}
