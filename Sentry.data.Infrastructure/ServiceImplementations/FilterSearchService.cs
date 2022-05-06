using Newtonsoft.Json;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class FilterSearchService : IFilterSearchService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserFavoriteService _userFavoriteService;

        public FilterSearchService(IDatasetContext datasetContext, IUserFavoriteService userFavoriteService)
        {
            _datasetContext = datasetContext;
            _userFavoriteService = userFavoriteService;
        }

        public SavedSearchDto GetSavedSearch(string searchType, string savedSearchName, string associateId)
        {
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchType == searchType && s.SearchName == savedSearchName && s.AssociateId == associateId);
            return savedSearch?.ToDto();
        }

        public List<string> GetSavedSearchNames(string searchType, string associateId)
        {
            return _datasetContext.SavedSearches.Where(s => s.SearchType == searchType && s.AssociateId == associateId).Select(s => s.SearchName).ToList();
        }

        public string SaveSearch(SavedSearchDto savedSearchDto)
        {
            string result;
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchType == savedSearchDto.SearchType && s.SearchName == savedSearchDto.SearchName && s.AssociateId == savedSearchDto.AssociateId);
            
            if (savedSearch != null)
            {
                //update existing
                savedSearch.SearchText = savedSearchDto.SearchText;
                savedSearch.FilterCategoriesJson = savedSearchDto.FilterCategories != null ? JsonConvert.SerializeObject(savedSearchDto.FilterCategories) : null;
                result = GlobalConstants.SaveSearchResults.UPDATE;
            }
            else
            {
                //create new
                savedSearch = savedSearchDto.ToEntity();
                _datasetContext.Add(savedSearch);
                result = GlobalConstants.SaveSearchResults.NEW;
            }

            _datasetContext.SaveChanges();

            if (savedSearchDto.AddToFavorites)
            {
                _userFavoriteService.AddUserFavorite(savedSearch, savedSearchDto.AssociateId);
            }

            return result;
        }
    }
}
