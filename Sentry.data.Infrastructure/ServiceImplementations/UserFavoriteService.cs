using Sentry.Associates;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.FeatureFlags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class UserFavoriteService : IUserFavoriteService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IDataFeatures _dataFeatures;

        public UserFavoriteService(IDatasetContext datasetContext, IGlobalDatasetProvider globalDatasetProvider, IDataFeatures dataFeatures)
        {
            _datasetContext = datasetContext;
            _globalDatasetProvider = globalDatasetProvider;
            _dataFeatures = dataFeatures;
        }
        
        public IList<FavoriteItem> GetUserFavoriteItems(string associateId)
        {
            try
            {
                List<UserFavorite> userFavorites = _datasetContext.UserFavorites.Where(f => f.AssociateId == associateId).ToList();
                List<FavoriteItem> favoriteItems = CreateFavoriteItems(userFavorites);

                //Get favorites from legacy favorites until refactored
                List<Favorite> legacyFavorites = _datasetContext.Favorites.Where(w => w.UserId == associateId).ToList();
                favoriteItems.AddRange(CreateLegacyFavoriteItems(legacyFavorites));

                return favoriteItems;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving User Favorite Items for {associateId}", ex);
                throw;
            }
        }

        public void AddUserFavorite(string favoriteType, int entityId, string associateId)
        {
            try
            {
                //check if favorite already exists
                UserFavorite existing = GetUserFavorite(favoriteType, entityId, associateId);

                if (existing == null)
                {
                    //create new UserFavorite
                    UserFavorite userFavorite = new UserFavorite()
                    {
                        AssociateId = associateId,
                        FavoriteType = favoriteType,
                        FavoriteEntityId = entityId,
                        CreateDate = DateTime.Now
                    };

                    //save UserFavorite
                    _datasetContext.Add(userFavorite);
                    _datasetContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding User Favorite of type {favoriteType} with entity id {entityId} for associate {associateId}", ex);
                throw;
            }
        }

        public void RemoveUserFavorite(int userFavoriteId, bool isLegacyFavorite)
        {
            try
            {
                //remove from legacy favorites until refactored
                if (isLegacyFavorite)
                {
                    Favorite legacyFavorite = _datasetContext.Favorites.FirstOrDefault(f => f.FavoriteId == userFavoriteId);

                    if (legacyFavorite != null)
                    {
                        Logger.Info($"Found Legacy Favorite {userFavoriteId} to remove");
                        _datasetContext.Remove(legacyFavorite);

                        if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                        {
                            _globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(legacyFavorite.DatasetId, legacyFavorite.UserId).Wait();
                        }

                        _datasetContext.SaveChanges();
                    }
                }
                else
                {
                    UserFavorite userFavorite = _datasetContext.UserFavorites.FirstOrDefault(f => f.UserFavoriteId == userFavoriteId);
                    RemoveUserFavorite(userFavorite);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error removing User Favorite {userFavoriteId}", ex);
                throw;
            }
        }

        public void RemoveUserFavorite(string favoriteType, int entityId, string associateId)
        {
            try
            {
                UserFavorite userFavorite = GetUserFavorite(favoriteType, entityId, associateId);
                RemoveUserFavorite(userFavorite);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error removing User Favorite of type {favoriteType} with entity id {entityId} for associate {associateId}", ex);
                throw;
            }
        }

        public IList<FavoriteItem> SetUserFavoritesOrder(List<KeyValuePair<int, bool>> orderedIds)
        {
            try
            {
                List<int> favoriteIds = orderedIds.Where(x => !x.Value).Select(x => x.Key).ToList();
                List<UserFavorite> userFavorites = _datasetContext.UserFavorites.Where(f => favoriteIds.Contains(f.UserFavoriteId)).ToList();

                List<int> legacyFavoriteIds = orderedIds.Where(x => x.Value).Select(x => x.Key).ToList();
                List<Favorite> legacyFavorites = _datasetContext.Favorites.Where(f => legacyFavoriteIds.Contains(f.FavoriteId)).ToList();

                int i = 0;
                foreach (KeyValuePair<int, bool> kvp in orderedIds)
                {
                    if (kvp.Value)
                    {
                        //is legacy favorite
                        Favorite legacyFavorite = legacyFavorites.FirstOrDefault(f => f.FavoriteId == kvp.Key);
                        if (legacyFavorite != null)
                        {
                            legacyFavorite.Sequence = i;
                        }
                    }
                    else
                    {
                        //is user favorite
                        UserFavorite userFavorite = userFavorites.FirstOrDefault(f => f.UserFavoriteId == kvp.Key);
                        if (userFavorite != null)
                        {
                            userFavorite.Sequence = i;
                        }
                    }

                    i++;
                }

                _datasetContext.SaveChanges();

                List<FavoriteItem> favoriteItems = CreateFavoriteItems(userFavorites);
                favoriteItems.AddRange(CreateLegacyFavoriteItems(legacyFavorites));

                return favoriteItems;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error setting favorite order for ids {string.Join(", ", orderedIds.Select(x => x.Key))}", ex);
                throw;
            }
        }

        public UserFavorite GetUserFavorite(string favoriteType, int entityId, string associateId)
        {
            return _datasetContext.UserFavorites.FirstOrDefault(x => x.AssociateId == associateId && x.FavoriteEntityId == entityId && x.FavoriteType == favoriteType);
        }

        #region Methods
        private List<FavoriteItem> CreateFavoriteItems(List<UserFavorite> userFavorites)
        {
            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();

            foreach (UserFavorite userFavorite in userFavorites)
            {
                IFavorable favoriteEntity = GetFavoriteEntity(userFavorite);
                if (favoriteEntity != null)
                {
                    FavoriteItem favoriteItem = new FavoriteItem() 
                    { 
                        Id = userFavorite.UserFavoriteId,
                        Sequence = userFavorite.Sequence 
                    };
                    favoriteEntity.SetFavoriteItem(favoriteItem);
                    favoriteItems.Add(favoriteItem);
                }
            }

            return favoriteItems;
        }

        private IFavorable GetFavoriteEntity(UserFavorite userFavorite)
        {
            switch (userFavorite.FavoriteType)
            {
                case GlobalConstants.UserFavoriteTypes.SAVEDSEARCH:
                    return _datasetContext.SavedSearches.FirstOrDefault(x => userFavorite.FavoriteEntityId == x.SavedSearchId);
                default:
                    return null;
            }
        }

        private List<FavoriteItem> CreateLegacyFavoriteItems(List<Favorite> favorites)
        {
            List<FavoriteItem> items = new List<FavoriteItem>();
            
            foreach (Favorite favorite in favorites)
            {
                Dataset ds = _datasetContext.Datasets.Where(w => w.DatasetId == favorite.DatasetId && w.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active).FirstOrDefault();

                if (ds != null)
                {
                    items.Add(new FavoriteItem(favorite.FavoriteId, favorite.DatasetId.ToString(), ds.DatasetName, CreateDataFeed(ds), favorite.Sequence));
                }
            }

            return items;
        }

        private DataFeed CreateDataFeed(Dataset ds)
        {
            if (ds.DatasetType == GlobalConstants.DataEntityCodes.REPORT)
            {
                return new DataFeed()
                {
                    Id = ds.DatasetId,
                    Name = GlobalConstants.DataFeedName.BUSINESS_INTELLIGENCE,
                    Url = !string.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location) ? ds.Metadata.ReportMetadata.Location : null,
                    UrlType = !string.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType) ? ds.Metadata.ReportMetadata.LocationType : null,
                    Type = GlobalConstants.DataFeedType.Exhibits
                };
            }

            return new DataFeed()
            {
                Id = ds.DatasetId,
                Name = GlobalConstants.DataFeedName.DATASET,
                Url = "/Datasets/Detail/" + ds.DatasetId,
                UrlType = ds.DatasetType,
                Type = GlobalConstants.DataFeedType.Datasets
            };
        }

        private void RemoveUserFavorite(UserFavorite favorite)
        {
            if (favorite != null)
            {
                Logger.Info($"Found User Favorite {favorite.UserFavoriteId} to remove");
                _datasetContext.Remove(favorite);
                _datasetContext.SaveChanges();
            }
        }
        #endregion
    }
}
