using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sentry.data.Core;
using System.Threading.Tasks;
using Sentry.Core;
using Sentry.data.Web.Filters.AuthorizationFilters;

namespace Sentry.data.Web.Controllers
{
    public class AssetController : ApiController
    {
        private IDataAssetContext _dataAssetContext;
        private UserService _userService;       

        public AssetController(IDataAssetContext dataAssetContext, UserService userService)
        {
            _dataAssetContext = dataAssetContext;
            _userService = userService;
        }

        [HttpGet]
        [Route("IsAlive")]
        [WebApiAuthorizeByPermission(PermissionNames.UseAssetAPI)]
        public IHttpActionResult IsAlive()
        {            
            return Ok("Alive");
        }

        [HttpPost]
        [WebApiAuthorizeByPermission(PermissionNames.UseAssetAPI)]
        public IHttpActionResult CreateAlert(string message, int severity, string assetName, string userName = null)
        {

            if (string.IsNullOrWhiteSpace(assetName))
            {
                return BadRequest("Asset name is required");
            }

            try
            {
                AssetNotifications an = new AssetNotifications()
                {
                    Message = message,
                    StartTime = DateTime.Now,
                    ExpirationTime = DateTime.MaxValue,
                    MessageSeverity = severity,
                    ParentDataAsset = _dataAssetContext.GetDataAsset(assetName)
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

                _dataAssetContext.Merge<AssetNotifications>(an);
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

        [HttpPost]
        [Route("ExpireAlertsByUser")]
        [WebApiAuthorizeByPermission(PermissionNames.UseAssetAPI)]
        public IHttpActionResult ExpireAlertsByUser(string userName)
        {
            List<AssetNotifications> userAlerts = _dataAssetContext.GetAllAssetNotifications().Where(w => w.CreateUser == userName && w.ExpirationTime > DateTime.Now).ToList();

            foreach (AssetNotifications alert in userAlerts)
            {
                alert.ExpirationTime = DateTime.Now;
                _dataAssetContext.Merge<AssetNotifications>(alert);
            }

            _dataAssetContext.SaveChanges();

            return Ok("Success");
        }

        [HttpPost]
        [WebApiAuthorizeByPermission(PermissionNames.UseAssetAPI)]
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
                List<AssetNotifications> assetAlerts = _dataAssetContext.GetAllAssetNotifications().Where(w => w.ParentDataAsset == asset && w.ExpirationTime > DateTime.Now).ToList();

                foreach (AssetNotifications alert in assetAlerts)
                {
                    alert.ExpirationTime = DateTime.Now;
                    _dataAssetContext.Merge<AssetNotifications>(alert);
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
                    case AssetNotifications.ValidationErrors.emptyCreateUser:
                        ModelState.AddModelError("CreateUser", vr.Description);
                        break;
                    case AssetNotifications.ValidationErrors.expireDateBeforeStartDate:
                        ModelState.AddModelError("ExpirationDate", vr.Description);
                        break;
                    case AssetNotifications.ValidationErrors.messageIsBlank:
                        ModelState.AddModelError("Message", vr.Description);
                        break;
                    case AssetNotifications.ValidationErrors.invalidSeverity:
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