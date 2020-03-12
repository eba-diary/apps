using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BusinessAreaController : BaseController
    {
        private readonly IBusinessAreaService _businessAreaService;
        private readonly IEventService _eventService;
        private readonly INotificationService _notificationService;
        private readonly IDataFeatures _featureFlags;

        public BusinessAreaController(IBusinessAreaService busAreaService, IEventService eventService, 
            INotificationService notificationService, IDataFeatures featureFlags)
        {
            _businessAreaService = busAreaService;
            _eventService = eventService;
            _notificationService = notificationService;
            _featureFlags = featureFlags;
        }

        public ActionResult PersonalLines()
        {
            if (_featureFlags.Expose_BusinessArea_Pages_CLA_1424.GetValue() || SharedContext.CurrentUser.IsAdmin)
            {
                BusinessAreaLandingPageModel model = GetBusinessAreaLandingPageModel();
                return View(model);
            }
            else
            {
                return View("Forbidden");
            }
        }


        private BusinessAreaLandingPageModel GetBusinessAreaLandingPageModel(bool activeOnly=true)
        {
            BusinessAreaLandingPageModel model = new BusinessAreaLandingPageModel()
            {
                Rows = new List<BusinessAreaTileRowModel>(),
                Notifications = _notificationService.GetNotificationForBusinessArea(BusinessAreaType.PersonalLines).ToModel(activeOnly)
            };
            model.HasActiveNotification = model.Notifications.CriticalNotifications.Any() || model.Notifications.StandardNotifications.Any();

            List<BusinessAreaTileRowDto> rows = _businessAreaService.GetRows(BusinessAreaType.PersonalLines).ToList();

            foreach (BusinessAreaTileRowDto row in rows)
            {
                model.Rows.Add(MapToRowModel(row));
            }

            return model;
        }

        private BusinessAreaTileRowModel MapToRowModel(BusinessAreaTileRowDto row)
        {
            return new BusinessAreaTileRowModel
            {
                ColumnSpan = row.ColumnSpan,
                Sequence = row.Sequence,
                Tiles = MapTilesToModel(row.Tiles, row.ColumnSpan)
            };
        }

        private List<BusinessAreaTileModel> MapTilesToModel(List<BusinessAreaTileDto> tiles, int ColumnSpan)
        {
            List<BusinessAreaTileModel> tileModels = new List<BusinessAreaTileModel>();

            foreach (BusinessAreaTileDto tile in tiles)
            {
                tileModels.Add(new BusinessAreaTileModel
                {
                    Title = tile.Title,
                    TileColor = tile.TileColor,
                    ImageName = tile.ImageName,
                    LinkText = tile.LinkText,
                    Hyperlink = tile.Hyperlink,
                    BootstrapSpan = (ColumnSpan == 2) ? "6" : "4" // row should contain either 2 or 3 columns; translate to what that means to bootstrap
                });
            }

            return tileModels;
        }


        //I put this method here because i could leverage the prebuilt GetBusinessAreaLandingPageModel() method which gives me a personalLines BusinessAreaLandingPageModel
        [HttpGet]
        public ActionResult GetLibertyBellHtml(BusinessAreaType businessAreaType, bool activeOnly)
        {
            BusinessAreaLandingPageModel model = GetBusinessAreaLandingPageModel(activeOnly);
            return PartialView("_LibertyBellPopover", model);
        }
    }
}