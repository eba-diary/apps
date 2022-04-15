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

        public SavedSearchDto GetSavedSearch(string savedSearchName, string associateId)
        {
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchName == savedSearchName && s.AssociateId == associateId);
            return savedSearch?.ToDto();
        }

        public List<string> GetSavedSearchNames(string associateId)
        {
            return _datasetContext.SavedSearches.Where(s => s.AssociateId == associateId).Select(s => s.SearchName).ToList();
        }

        public void SaveSearch(SavedSearchDto savedSearchDto)
{
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchName == savedSearchDto.SearchName && s.AssociateId == savedSearchDto.AssociateId);
            
            if (savedSearch != null)
            {
                //update existing
                savedSearch.SearchText = savedSearchDto.SearchText;
                savedSearch.FilterCategoriesJson = JsonConvert.SerializeObject(savedSearchDto.FilterCategories);
            }
            else
            {
                //create new
                savedSearch = savedSearchDto.ToEntity();
                _datasetContext.Add(savedSearch);
            }

            _datasetContext.SaveChanges();

            if (savedSearchDto.AddToFavorites)
            {
                _userFavoriteService.AddUserFavorite(savedSearch, savedSearchDto.AssociateId);
            }
        }
    }
}
