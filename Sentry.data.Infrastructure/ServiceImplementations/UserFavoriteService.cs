using Sentry.data.Core;
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
            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();
            
            List<UserFavorite> userFavorites = _datasetContext.UserFavorites.Where(f => f.AssociateId == associateId).ToList();


            //Get favorites from legacy favorites until refactored
            favoriteItems.AddRange(GetLegacyFavoriteItems(associateId));

            return favoriteItems;
        }

        public void RemoveUserFavorite(int userFavoriteId, string associateId)
        {
            //associate id only used until legacy favorites are refactored to help prevent incorrect favorite from being deleted
            UserFavorite userFavorite = _datasetContext.UserFavorites.FirstOrDefault(f => f.UserFavoriteId == userFavoriteId && f.AssociateId == associateId);

            if (userFavorite != null)
            {
                _datasetContext.Remove(userFavorite);
                _datasetContext.SaveChanges();
            }

            //remove from legacy favorites until refactored
            Favorite legacyFavorite = _datasetContext.Favorites.FirstOrDefault(f => f.FavoriteId == userFavoriteId && f.UserId == associateId);

            if (legacyFavorite != null)
            {
                _datasetContext.Remove(legacyFavorite);
                _datasetContext.SaveChanges();
            }
        }
        
        public IList<FavoriteItem> SetUserFavoritesOrder(List<int> orderedIds, string associateId)
        {
            //associate id only used until legacy favorites are refactored to help prevent ordering incorrect favorites
            List<UserFavorite> userFavorites = _datasetContext.UserFavorites.Where(f => orderedIds.Contains(f.UserFavoriteId) && f.AssociateId == associateId).ToList();
            List<Favorite> legacyFavorites = _datasetContext.Favorites.Where(f => orderedIds.Contains(f.FavoriteId) && f.UserId == associateId).ToList();

            int i = 0;
            foreach (int id in orderedIds)
            {
                UserFavorite userFavorite = userFavorites.FirstOrDefault(f => f.UserFavoriteId == id);
                if (userFavorite != null)
                {
                    userFavorite.Sequence = i;
                }
                else
                {
                    Favorite legacyFavorite = legacyFavorites.FirstOrDefault(f => f.FavoriteId == id);
                    if (legacyFavorite != null)
                    {
                        legacyFavorite.Sequence = i;
                    }
                }
            }

            _datasetContext.SaveChanges();

            List<FavoriteItem> favoriteItems = CreateFavoriteItems(userFavorites);
            favoriteItems.AddRange(CreateLegacyFavoriteItems(legacyFavorites));

            return favoriteItems;
        }

        private List<FavoriteItem> CreateFavoriteItems(List<UserFavorite> userFavorites)
        {
            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();

            foreach (UserFavorite userFavorite in userFavorites)
            {
                IFavorable favoriteEntity = GetFavoriteEntity(userFavorite);
                if (favoriteEntity != null)
                {
                    favoriteItems.Add(favoriteEntity.CreateFavoriteItem(userFavorite));
                }
            }

            return favoriteItems;
        }

        private IFavorable GetFavoriteEntity(UserFavorite userFavorite)
        {
            switch (userFavorite.FavoriteType)
            {
                case GlobalConstants.UserFavoriteTypes.SAVEDSEARCH:
                    return _datasetContext.SavedSearches.FirstOrDefault(x => userFavorite.FavoriteEntityId == x.SavedSearchId) as IFavorable;
                default:
                    return null;
            }
        }

        private List<FavoriteItem> GetLegacyFavoriteItems(string associateId)
        {
            List<Favorite> favsList = _datasetContext.Favorites.Where(w => w.UserId == associateId).ToList();
            return CreateLegacyFavoriteItems(favsList);
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
