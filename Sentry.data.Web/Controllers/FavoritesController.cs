﻿using Sentry.data.Core;
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
            return View(BuildFavorites());
        }

        public ActionResult Delete(int favId, bool isLegacyFavorite)
        {
            // fetch the Favorite from the database
            //isLegacyFavorite only needed until legacy favorites are converted
            _userFavoriteService.RemoveUserFavorite(favId, isLegacyFavorite);
            return PartialView("_FavoritesList", BuildFavorites());
        }

        public ActionResult Sort(List<KeyValuePair<int, bool>> orderedFavoriteIds)
        {
            //key value pairs only needed until legacy favorites are converted
            IList<FavoriteItem> favoriteItems = _userFavoriteService.SetUserFavoritesOrder(orderedFavoriteIds);
            return PartialView("_FavoritesList", BuildFavorites(favoriteItems));
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

        private List<FavoriteItemModel> BuildFavorites()
        {
            // fetch the user's Favorites; sorted by Sequence, then Title
            IList<FavoriteItem> favList = _userFavoriteService.GetUserFavoriteItems(SharedContext.CurrentUser.AssociateId);
            return BuildFavorites(favList);
        }

        private List<FavoriteItemModel> BuildFavorites(IList<FavoriteItem> favoriteItems)
        {
            return favoriteItems.Select(x => x.ToModel()).OrderBy(x => x.Sequence).ThenBy(y => y.Title).ToList();
        }
    }
}