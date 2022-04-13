using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class FavoritesController : BaseController
    {
        private readonly IDatasetService _datasetService;
        private readonly IUserFavoriteService _userFavoriteService;

        public FavoritesController(IDatasetService datasetService, IUserFavoriteService userFavoriteService)
        {
            _datasetService = datasetService;
            _userFavoriteService = userFavoriteService;
        }

        public ActionResult EditFavorites()
        {
            return View(BuildFavoritesModel());
        }

        public ActionResult Delete(int favId)
        {
            // fetch the Favorite from the database
            //Associate Id only needed until legacy favorites are refactored
            _userFavoriteService.RemoveUserFavorite(favId, SharedContext.CurrentUser.AssociateId);
            return PartialView("_FavoritesList", BuildFavoritesModel());
        }

        public ActionResult Sort(FavoritesModel model)
        {
            // create a list of integers that contain the Ids of the Favorites in the specified order
            List<int> orderedIds = model.OrderedFavoriteIds.Split(',').Select(int.Parse).ToList();
            IList<FavoriteItem> favoriteItems = _userFavoriteService.SetUserFavoritesOrder(orderedIds, SharedContext.CurrentUser.AssociateId);
            return PartialView("_FavoritesList", BuildFavoritesModel(favoriteItems));
        }

        public JsonResult SetFavorite(int datasetId)
        {
            try
            {
                string result = _datasetService.SetDatasetFavorite(datasetId, SharedContext.CurrentUser.AssociateId);
                Response.StatusCode = (int)HttpStatusCode.OK;
                return Json(new { message = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { message = "Failed to modify favorite." }, JsonRequestBehavior.AllowGet);
            }
        }

        private FavoritesModel BuildFavoritesModel()
        {
            // fetch the user's Favorites; sorted by Sequence, then Title
            IList<FavoriteItem> favList = _userFavoriteService.GetUserFavoriteItems(SharedContext.CurrentUser.AssociateId);
            return BuildFavoritesModel(favList);
        }

        private FavoritesModel BuildFavoritesModel(IList<FavoriteItem> favoriteItems)
        {
            FavoritesModel model = new FavoritesModel
            {
                Favorites = favoriteItems.Select(x => x.ToModel()).OrderBy(x => x.Sequence).ThenBy(y => y.Title).ToList()
            };

            // add each favorite Id (in order) to the model collection
            model.OrderedFavoriteIds = string.Join(",", model.Favorites.Select(x => x.Id).ToList());

            return model;
        }
    }
}