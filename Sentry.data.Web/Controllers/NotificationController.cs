using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Shared;

namespace Sentry.data.Web.Controllers
{
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW)]
    public class NotificationController : BaseController
    {

        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public ActionResult ManageNotification()
        {
            ManageNotificationViewModel model = new ManageNotificationViewModel()
            {
                CanModifyNotifications = _notificationService.CanUserModifyNotifications()
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult ModifyNotification()
        {
            if (_notificationService.CanUserModifyNotifications())
            {
                NotificationModel model = _notificationService.GetModifyNotificationModel().ToWeb();
                return View(model);
            }
            return View("NotFound");
        }

        [HttpPost]
        public ActionResult SubmitNotification(NotificationModel model)
        {
            if (!_notificationService.CanUserModifyNotifications())
            {
                return View("NotFound");
            }

            AddCoreValidationExceptionsToModel(model.Validate());
            if (ModelState.IsValid)
            {
                _notificationService.SubmitNotification(model.ToCore());
                return View("ManageNotification");
            }

            return View("ModifyNotification",model);
        }

        [HttpGet]
        public JsonResult GetAssetNotificationInfoForGrid(int Id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequest)
        {
            List<NotificationModel> files = _notificationService.GetNotificationsForDataAsset(Id).ToWeb();

            DataTablesQueryableAdapter<NotificationModel> dtqa = new DataTablesQueryableAdapter<NotificationModel>(files.AsQueryable(), dtRequest);
            return Json(dtqa.GetDataTablesResponse(), JsonRequestBehavior.AllowGet);
        }


    }
}