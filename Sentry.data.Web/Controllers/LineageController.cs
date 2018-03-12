using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class LineageController : BaseController
    {
        private IDataAssetProvider _dataAssetProvider;
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;
        private List<DataAsset> das;

        public LineageController(IDataAssetProvider dap, MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _dataAssetProvider = dap;
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        [AuthorizeByPermission(PermissionNames.UserSwitch)]
        public ActionResult Index()
        { 
            return View();
        }



    }
}
