using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using System.Text;

namespace Sentry.data.Web.Controllers
{
    public class DataAssetController : Controller
    {
        private IDataAssetProvider _dataAssetProvider;
        private MetadataRepositoryService _metadataRepositoryService;
        private List<DataAsset> das;

        public DataAssetController(IDataAssetProvider dap, MetadataRepositoryService metadataRepositoryService)
        {
            _dataAssetProvider = dap;
            _metadataRepositoryService = metadataRepositoryService;
            das = new List<DataAsset>(_dataAssetProvider.GetDataAssets());
        }
        
        public ActionResult Index(int id)
        {
            id = (id == 0) ? das[0].Id : id;

            DataAsset da = _dataAssetProvider.GetDataAsset(id);
            da.LastUpdated = DateTime.Now;
            da.Status = 1;
            ViewBag.DataAssets = das;
            //ViewData["fluid"] = true;

            if (da != null) { return View(da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }

        public ActionResult DataAsset(string assetName)
        {
            assetName = (assetName == null) ? das[0].Name : assetName;

            DataAsset da = _dataAssetProvider.GetDataAsset(assetName);
            da.LastUpdated = DateTime.Now;
            da.Status = 1;
            ViewBag.DataAssets = das;
            //ViewData["fluid"] = true;

            if (da != null) { return View("Index", da); }
            else { return RedirectToAction("NotFound", "Error"); }
        }
    }
}