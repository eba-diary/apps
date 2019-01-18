using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{
    public class FavoritesController : BaseController
    {
        private readonly IDataFeedContext _feedContext;
        private readonly IDatasetContext _datasetContext;

        public FavoritesController(IDataFeedContext feedContext, IDatasetContext datasetContext)
        {
            _feedContext = feedContext;
            _datasetContext = datasetContext;
        }

        public ActionResult EditFavorites()
        {
            return View(BuildFavoritesModel());
        }

        public ActionResult Delete(int favId)
        {
            // fetch the Favorite from the database
            Favorite fav = _datasetContext.GetFavorite(favId);

            _datasetContext.Remove(fav);
            _datasetContext.SaveChanges();

            return PartialView("_FavoritesList", BuildFavoritesModel());
        }

        public ActionResult SortFavorites(FavoritesModel model)
        {
            // create a list of integers that contain the Ids of the Favorites in the specified order
            List<int> sortedIds = model.OrderedFavoriteIds.Split(',').Select(Int32.Parse).ToList();

            // get the Favorites from the database
            List<Favorite> favItems = _datasetContext.GetFavorites(sortedIds);

            // variable used to set the order while iterating through the list of sorted Favorite Ids
            int i = 1;

            // loop through the list of sorted Favorite Ids, finding the matching Favorite
            foreach (int favId in sortedIds)
            {
                Favorite fav = favItems.Single(x => x.FavoriteId == favId);

                // assign the new sequence
                fav.Sequence = i;

                // increment i
                i += 1;
            }

            // save the changes
            _datasetContext.SaveChanges();

            return PartialView("_FavoritesList", BuildFavoritesModel());
        }

        // 
        private FavoritesModel BuildFavoritesModel()
        {
            // fetch the user's Favorites; sorted by Sequence, then Title
            List<FavoriteItem> favItems = _feedContext.GetUserFavorites(SharedContext.CurrentUser.AssociateId).OrderBy(x => x.Sequence).ThenBy(y => y.Title).ToList();

            FavoritesModel model = new FavoritesModel
            {
                Favorites = ToWeb(favItems)
            };

            // add each favorite Id (in order) to the model collection
            model.OrderedFavoriteIds = String.Join(",",model.Favorites.Select(x => x.Id).ToList());

            return model;
        }


        // TODO: these methods should really be an extension class so they can be reusable. will look into converting to one if there is time
        private List<FavoriteItemModel> ToWeb(List<FavoriteItem> favItems)
        {
            List<FavoriteItemModel> favorites = new List<FavoriteItemModel>();

            // convert the Core object to a Web object
            foreach (FavoriteItem fi in favItems)
            {
                favorites.Add(ToWeb(fi));
            }

            return favorites;
        }

        private FavoriteItemModel ToWeb(FavoriteItem favItem)
        {
            return new FavoriteItemModel
            {
                Id = favItem.Id,
                FeedId = favItem.FeedId,
                FeedName = favItem.FeedName,
                FeedUrl = favItem.FeedUrl,
                FeedUrlType = favItem.FeedUrlType,
                Img = favItem.Img,
                Sequence = favItem.Sequence,
                Title = favItem.Title,
                Url = favItem.Url
            };
        }
    }
}