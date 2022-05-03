using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<SavedSearchDto> GetSavedSearchAsync(string searchType, string savedSearchName, string associateId)
        {
            return await Task.Run(() => GetSavedSearch(searchType, savedSearchName, associateId));
        }

        public SavedSearchDto GetSavedSearch(string searchType, string savedSearchName, string associateId)
        {
            try
            {
                SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchType == searchType && s.SearchName == savedSearchName && s.AssociateId == associateId);
                return savedSearch?.ToDto();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving Saved Search of type {searchType} named {savedSearchName} for {associateId}", ex);
                throw;
            }
        }

        public List<SavedSearchOptionDto> GetSavedSearchOptions(string searchType, string associateId)
        {
            try
            {
                List<SavedSearchOptionDto> savedSearchOptions = new List<SavedSearchOptionDto>();

                List<SavedSearch> savedSearches = _datasetContext.SavedSearches.Where(s => s.SearchType == searchType && s.AssociateId == associateId).ToList();

                foreach (SavedSearch savedSearch in savedSearches)
                {
                    savedSearchOptions.Add(new SavedSearchOptionDto()
                    {
                        SavedSearchId = savedSearch.SavedSearchId,
                        SavedSearchName = savedSearch.SearchName,
                        IsFavorite = _userFavoriteService.GetUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, savedSearch.SavedSearchId, associateId) != null
                    });
                }

                return savedSearchOptions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving Saved Search Options of type {searchType} for {associateId}", ex);
                throw;
            }
        }

        public void RemoveSavedSearch(int savedSearchId, string associateId)
        {
            try
            {
                SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SavedSearchId == savedSearchId && s.AssociateId == associateId);

                if (savedSearch != null)
                {
                    Logger.Info($"Found Saved Search {savedSearchId} to remove for user {associateId}");
                    _datasetContext.Remove(savedSearch);
                    _datasetContext.SaveChanges();

                    _userFavoriteService.RemoveUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, savedSearchId, associateId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error removing Saved Search {savedSearchId} for {associateId}", ex);
                throw;
            }
        }

        public string SaveSearch(SavedSearchDto savedSearchDto)
        {
            try
            {
                return savedSearchDto.SavedSearchId == 0 ? SaveNewSearch(savedSearchDto) : UpdateSavedSearch(savedSearchDto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error Saving Search", ex);
                throw;
            }
        }

        #region Methods
        private string SaveNewSearch(SavedSearchDto savedSearchDto)
        {            
            //check if saved search already exists
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SearchType == savedSearchDto.SearchType && s.SearchName == savedSearchDto.SearchName && s.AssociateId == savedSearchDto.AssociateId);

            if (savedSearch == null)
            {
                savedSearch = savedSearchDto.ToEntity();
                _datasetContext.Add(savedSearch);
                _datasetContext.SaveChanges();

                if (savedSearchDto.AddToFavorites)
                {
                    _userFavoriteService.AddUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, savedSearch.SavedSearchId, savedSearchDto.AssociateId);
                }

                return GlobalConstants.SaveSearchResults.NEW;
            }
            
            return GlobalConstants.SaveSearchResults.EXISTS;
        }

        private string UpdateSavedSearch(SavedSearchDto savedSearchDto)
        {
            SavedSearch savedSearch = _datasetContext.SavedSearches.FirstOrDefault(s => s.SavedSearchId == savedSearchDto.SavedSearchId);
            
            savedSearch.SearchName = savedSearchDto.SearchName;
            savedSearch.SearchText = savedSearchDto.SearchText;
            savedSearch.FilterCategoriesJson = savedSearchDto.FilterCategories != null ? JsonConvert.SerializeObject(savedSearchDto.FilterCategories) : null;

            _datasetContext.SaveChanges();

            if (savedSearchDto.AddToFavorites)
            {
                _userFavoriteService.AddUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, savedSearch.SavedSearchId, savedSearchDto.AssociateId);
            }
            else
            {
                _userFavoriteService.RemoveUserFavorite(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, savedSearch.SavedSearchId, savedSearchDto.AssociateId);
            }
            
            return GlobalConstants.SaveSearchResults.UPDATE;
        }
        #endregion
    }
}
