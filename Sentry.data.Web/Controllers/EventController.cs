using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Sentry.Common.Logging;
using Sentry.data.Core;

namespace Sentry.data.Web.Controllers
{


    [SessionState(SessionStateBehavior.ReadOnly)]
    public class EventController : BaseController
    {

        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public JsonResult PublishSuccessEventByDatasetId(string eventType, string reason, int datasetId)
        {
            try
            {
                string IsTypeValid = MapToEventType(eventType);

                if (IsTypeValid != null)
                {
                    _eventService.PublishSuccessEventByDatasetId(IsTypeValid, SharedContext.CurrentUser.AssociateId, reason, datasetId);
                }
                else
                {
                    return Json(Json(new { Success = false, Message = "Invalid Event Type" }), JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                Logger.Error($"eventcontroller-publishsuccesseventbydatasetid failed eventtype:{eventType} reason:{reason} datasetid:{datasetId}", ex);
                return Json(Json(new { Success = false, Message = "Failed to pushish event" }), JsonRequestBehavior.AllowGet);
            }
            
            return Json(Json(new { Success = true, Message = "Event published successfully" }), JsonRequestBehavior.AllowGet);
        }

        private string MapToEventType(string eventType)
        {
            FieldInfo[] arry = GetConstants(typeof(GlobalConstants.EventType));

            foreach (var item in arry)
            {
                if (eventType.ToLower() == item.GetValue(item).ToString().ToLower())
                {
                    return item.GetValue(item).ToString();
                }
            }

            return null;
        }

        private FieldInfo[] GetConstants(System.Type type)
        {
            ArrayList constants = new ArrayList();

            FieldInfo[] fieldInfos = type.GetFields(
                // Gets all public and static fields

                BindingFlags.Public | BindingFlags.Static |
                // This tells it to get the fields from all base types as well

                BindingFlags.FlattenHierarchy);

            // Go through the list and only pick out the constants
            foreach (FieldInfo fi in fieldInfos)
                // IsLiteral determines if its value is written at 
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set 
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true 
                //   but a const field would have only IsLiteral equal to true
                if (fi.IsLiteral && !fi.IsInitOnly)
                    constants.Add(fi);

            // Return an array of FieldInfos
            return (FieldInfo[])constants.ToArray(typeof(FieldInfo));
        }
    }
}