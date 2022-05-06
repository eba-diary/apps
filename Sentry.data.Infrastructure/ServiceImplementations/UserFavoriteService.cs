using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure
{
    public class UserFavoriteService : IUserFavoriteService
    {
        private readonly IDatasetContext _datasetContext;

        public UserFavoriteService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }
        
        public IList<FavoriteItem> GetUserFavoriteItems(string associateId)
        {            
            List<UserFavorite> userFavorites = _datasetContext.UserFavorites.Where(f => f.AssociateId == associateId).ToList();
            List<FavoriteItem> favoriteItems = CreateFavoriteItems(userFavorites);

            //Get favorites from legacy favorites until refactored
            List<Favorite> legacyFavorites = _datasetContext.Favorites.Where(w => w.UserId == associateId).ToList();
            favoriteItems.AddRange(CreateLegacyFavoriteItems(legacyFavorites));

            return favoriteItems;
        }

        public void RemoveUserFavorite(int userFavoriteId, bool isLegacyFavorite)
        {
            //remove from legacy favorites until refactored
            if (isLegacyFavorite)
            {
                Favorite legacyFavorite = _datasetContext.Favorites.FirstOrDefault(f => f.FavoriteId == userFavoriteId);

                if (legacyFavorite != null)
                {
                    Logger.Info($"Found Legacy Favorite {userFavoriteId} to remove");
                    _datasetContext.Remove(legacyFavorite);
                    _datasetContext.SaveChanges();
                }
            }
            else
            {
                UserFavorite userFavorite = _datasetContext.UserFavorites.FirstOrDefault(f => f.UserFavoriteId == userFavoriteId);

                if (userFavorite != null)
                {
                    Logger.Info($"Found User Favorite {userFavoriteId} to remove");
                    _datasetContext.Remove(userFavorite);
                    _datasetContext.SaveChanges();
                }
            }            
        }
        
        public IList<FavoriteItem> SetUserFavoritesOrder(List<KeyValuePair<int, bool>> orderedIds)
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

        public void AddUserFavorite(IFavorable favorite, string associateId)
        {
            int favoriteEntityId = favorite.GetFavoriteEntityId();
            string favoriteType = favorite.GetFavoriteType();

            //check if favorite already exists
            UserFavorite existing = _datasetContext.UserFavorites.FirstOrDefault(x => x.AssociateId == associateId && x.FavoriteEntityId == favoriteEntityId && x.FavoriteType == favoriteType);

            if (existing == null)
            {
                //create new UserFavorite
                UserFavorite userFavorite = new UserFavorite()
                {
                    AssociateId = associateId,
                    FavoriteType = favoriteType,
                    FavoriteEntityId = favoriteEntityId,
                    CreateDate = DateTime.Now
                };

                //save UserFavorite
                _datasetContext.Add(userFavorite);
                _datasetContext.SaveChanges();
            }
        }

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
    }
}
