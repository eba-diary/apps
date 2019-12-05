using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Sentry.data.Core;
using Sentry.Core;
using Sentry.data.Web.Filters.AuthorizationFilters;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_ASSET)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.DATA_ASSET_VIEW)]
    public class AssetController : BaseWebApiController
    {
        private IDataAssetContext _dataAssetContext;
        private UserService _userService;       

        public AssetController(IDataAssetContext dataAssetContext, UserService userService)
        {
            _dataAssetContext = dataAssetContext;
            _userService = userService;
        }


        /// <summary>
        /// Check if the asset is alive
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("isAlive")]
        public IHttpActionResult IsAlive()
        {            
            return Ok("Alive");
        }

        /// <summary>
        ///  create a new alert
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <param name="assetName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{assetName}/users/{userName}/{message}/{severity}")]
        public IHttpActionResult CreateAlert(string message, int severity, string assetName, string userName = null)
        {

            if (string.IsNullOrWhiteSpace(assetName))
            {
                return BadRequest("Asset name is required");
            }

            try
            {
                Notification an = new Notification()
                {
                    Message = message,
                    StartTime = DateTime.Now,
                    ExpirationTime = DateTime.MaxValue,
                    MessageSeverity = (NotificationSeverity)Enum.ToObject(typeof(NotificationSeverity), severity),
                    ParentObject = _dataAssetContext.GetDataAsset(assetName).Id,
                    NotificationType = Core.GlobalConstants.Notifications.DATAASSET_TYPE
                };

                if (String.IsNullOrWhiteSpace(userName))
                {
                    try
                    {
                        an.CreateUser = _userService.GetCurrentUser().AssociateId;
                    }
                    catch (Exception)
                    {
                        an.CreateUser = "Unknown";
                    }
                }
                else
                {
                    an.CreateUser = userName;
                }

                _dataAssetContext.Merge<Notification>(an);
                _dataAssetContext.SaveChanges();
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok("Success");
        }

        /// <summary>
        /// expire all alerts for a user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("expireAlerts/users/{userName}")]
        public IHttpActionResult ExpireAlertsByUser(string userName)
        {
            List<Notification> userAlerts = _dataAssetContext.GetAllAssetNotifications().Where(w => w.CreateUser == userName && w.ExpirationTime > DateTime.Now).ToList();

            foreach (Notification alert in userAlerts)
            {
                alert.ExpirationTime = DateTime.Now;
                _dataAssetContext.Merge<Notification>(alert);
            }

            _dataAssetContext.SaveChanges();

            return Ok("Success");
        }

        /// <summary>
        /// expire all alerts for an asset
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{assetName}/expireAlerts")]
        public IHttpActionResult ExpireAlertsByAsset(string assetName)
        {
            DataAsset asset;

            try
            {
                asset = _dataAssetContext.GetDataAsset(assetName);
            }
            catch
            {
                return BadRequest("Unable to retrieve specified asset");                
            }

            try
            {
                List<Notification> assetAlerts = _dataAssetContext.GetAllAssetNotifications().Where(w => w.ParentObject == asset.Id && w.ExpirationTime > DateTime.Now).ToList();

                foreach (Notification alert in assetAlerts)
                {
                    alert.ExpirationTime = DateTime.Now;
                    _dataAssetContext.Merge<Notification>(alert);
                }

                _dataAssetContext.SaveChanges();
            }
            catch (Sentry.Core.ValidationException ex)
            {
                AddCoreValidationExceptionsToModel(ex);
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok("Success");
        }

        protected void AddCoreValidationExceptionsToModel(Sentry.Core.ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                switch (vr.Id)
                {
                    case Notification.ValidationErrors.emptyCreateUser:
                        ModelState.AddModelError("CreateUser", vr.Description);
                        break;
                    case Notification.ValidationErrors.expireDateBeforeStartDate:
                        ModelState.AddModelError("ExpirationDate", vr.Description);
                        break;
                    case Notification.ValidationErrors.messageIsBlank:
                        ModelState.AddModelError("Message", vr.Description);
                        break;
                    case Notification.ValidationErrors.invalidSeverity:
                        ModelState.AddModelError("Severity", vr.Description);
                        break;
                    default:
                        ModelState.AddModelError(vr.Id, vr.Description);
                        break;
                }
            }
        }
    }
}