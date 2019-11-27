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
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
    public class BusinessAreaController : BaseController
    {
        private readonly IBusinessAreaService _businessAreaService;
        private readonly IEventService _eventService;
        private readonly INotificationService _notificationService;

        public BusinessAreaController(IBusinessAreaService busAreaService, IEventService eventService, INotificationService notificationService)
        {
            _businessAreaService = busAreaService;
            _eventService = eventService;
            _notificationService = notificationService;
        }

        public ActionResult PersonalLines()
        {
            // TODO: should we create an event for someone viewing the personal lines landing page??            

            BusinessAreaLandingPageModel model = new BusinessAreaLandingPageModel()
            {
                Rows = new List<BusinessAreaTileRowModel>(),
                //Notifications = BuildMockNotifications() // temporary!!
                Notifications = _notificationService.GetNotificationForBusinessArea(BusinessAreaType.PersonalLines).ToModel()
            };
            model.HasActiveNotification = model.Notifications.CriticalNotifications.Any() || model.Notifications.StandardNotifications.Any();

            List<BusinessAreaTileRowDto> rows = _businessAreaService.GetRows(BusinessAreaType.PersonalLines).ToList();

            foreach (BusinessAreaTileRowDto row in rows)
            {
                model.Rows.Add(MapToRowModel(row));
            }

            return View(model);
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

    }
}