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

        public BusinessAreaController(IBusinessAreaService busAreaService, IEventService eventService)
        {
            _businessAreaService = busAreaService;
            _eventService = eventService;
        }

        public ActionResult PersonalLines()
        {
            // TODO: should we create an event for someone viewing the personal lines landing page??


            BusinessAreaLandingPageModel model = new BusinessAreaLandingPageModel()
            {
                Rows = new List<BusinessAreaTileRowModel>(),
                Notifications = BuildMockNotifications() // temporary!!
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

        private SystemNotificationModel BuildMockNotifications()
        {
            SystemNotificationModel model = new SystemNotificationModel
            {
                CriticalNotifications = new List<SystemNotificationItemModel>(),
                StandardNotifications = new List<SystemNotificationItemModel>()
            };

            model.CriticalNotifications.Add(new SystemNotificationItemModel
            {
                Title = "Driver Assignment Data Issue!",
                Message = "Driver Assignments are incorrect in SERA PL and ODS for December only. Measures impacted are inforce counts and loss information. This will impact reports like the insured profile and customer personas. Total inforce counts and loss information are correct, but how the mesasure is associated to the driver is incorrect. This doesn&rsquo;t impact measures for exposures and premium. Defects have been opened to address and additional communication will be sent when the fix has been implemented.",
                NotificationDate = DateTime.Today.ToShortDateString()
            });

            model.CriticalNotifications.Add(new SystemNotificationItemModel
            {
                Title = "A Second Critical Alert!",
                Message = "This is a second critical alert to demo how this would look and act in DSC.",
                NotificationDate = DateTime.Today.AddDays(-1).ToShortDateString()
            });

            model.StandardNotifications.Add(new SystemNotificationItemModel
            {
                Title = "Example of a Standard Notification",
                Message = "This is just a standard notification. It will live underneath the critical notifications and in a carousel.",
                NotificationDate = DateTime.Today.AddHours(-1).ToShortDateString()
            });

            model.StandardNotifications.Add(new SystemNotificationItemModel
            {
                Title = "A Second Standard Notification",
                Message = "This is just another standard notification. It will live underneath the critical notifications and in a carousel.",
                NotificationDate = DateTime.Today.AddDays(-2).ToShortDateString()
            });

            return model;
        }

    }
}