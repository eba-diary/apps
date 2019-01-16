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

        public FavoritesController(IDataFeedContext feedContext)
        {
            _feedContext = feedContext;
        }

        public ActionResult ManageFavorites()
        {
            List<FavoriteItem> favItems = _feedContext.GetUserFavorites(SharedContext.CurrentUser.AssociateId).OrderBy(x => x.Sequence).ThenBy(y => y.Title).ToList();

            FavoritesModel model = new FavoritesModel {
                Favorites = ToWeb(favItems)
            };

            return View(model);
        }

        // TODO: this should really be an extension method so it can be reusable. will look into converting to one if there is time
        private List<FavoriteItemModel> ToWeb(List<FavoriteItem> favItems)
        {
            List<FavoriteItemModel> favorites = new List<FavoriteItemModel>();

            // convert the Core object to a Web object
            foreach (FavoriteItem fi in favItems)
            {
                favorites.Add(new FavoriteItemModel
                {
                    FeedId = fi.FeedId,
                    FeedName = fi.FeedName,
                    FeedUrl = fi.FeedUrl,
                    FeedUrlType = fi.FeedUrlType,
                    Img = fi.Img,
                    Sequence = fi.Sequence,
                    Title = fi.Title,
                    Url = fi.Url
                });
            }

            return favorites;
        }
    }
}