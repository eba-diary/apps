using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Common;
using Sentry.data.Infrastructure;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Sentry.Web.CachedObsidianUserProvider;
using Sentry.Common.Logging;
using StructureMap;

namespace Sentry.data.SpamFactory
{
    class Watch
    {

        public class UserEvent
        {
            public string email { get; set; }

            public List<Event> events { get; set; }
        }

        public static IContainer container;
        /// <summary>
        /// 
        /// </summary>
        public static IDatasetContext _datasetContext;
        public static IAssociateInfoProvider _associateInfoProvider;


        public static async Task Run(string interval)
        {
            try
            {
                using (container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    _datasetContext = container.GetInstance<IDatasetContext>();
                    _associateInfoProvider = container.GetInstance<IAssociateInfoProvider>();


                    List<Event> events;
                    Console.WriteLine("Running " + interval);

                    switch (interval)
                    {
                        case "Hourly":
                            events = _datasetContext.EventsSince(DateTime.Now.AddHours(-1), true);
                            break;

                        case "Daily":
                            events = _datasetContext.EventsSince(DateTime.Now.AddDays(-1), true);
                            break;

                        case "Weekly":
                            events = _datasetContext.EventsSince(DateTime.Now.AddDays(-7), true);
                            break;

                        case "Instant":
                        default:
                            events = _datasetContext.EventsSince(DateTime.Now.AddYears(-20), false);
                            break;
                    }

                    List<UserEvent> userEvents = new List<UserEvent>();

                    foreach (Event _event in events)
                    {
                        var subsThatMatch = from _sub in _datasetContext.GetAllSubscriptions()
                                            where _sub.Dataset.DatasetId == _event.Dataset.DatasetId &&
                                                    _sub.EventType.Type_ID == _event.EventType.Type_ID &&
                                                    _sub.Interval == _datasetContext.GetInterval(interval)
                                            select _sub;

#if (DEBUG)

                    if (_event.UserWhoStartedEvent != null && (_event.UserWhoStartedEvent == "082698" || _event.UserWhoStartedEvent == "072984"))

#else

                        if (_event.UserWhoStartedEvent != null )

#endif
                        {

                            Console.WriteLine("UserWhoStartedEvent : " + _event.UserWhoStartedEvent);
                            var user = _associateInfoProvider.GetAssociateInfo(_event.UserWhoStartedEvent);

                            UserEvent ue;
                            if (userEvents.Any(x => x.email == user.WorkEmailAddress))
                            {
                                ue = userEvents.FirstOrDefault(x => x.email == user.WorkEmailAddress);
                                ue.events.Add(_event);
                            }
                            else
                            {
                                ue = new UserEvent();
                                ue.events = new List<Event>();
                                ue.events.Add(_event);
                                ue.email = user.WorkEmailAddress;

                                userEvents.Add(ue);
                            }
                        }

                        foreach (DatasetSubscription ds in subsThatMatch)
                        {
#if (DEBUG)
                         if (ds.SentryOwnerName == "082698" || ds.SentryOwnerName == "072984")
#endif
                            Console.WriteLine("ds.SentryOwnerName : " + ds.SentryOwnerName);
                            var user = _associateInfoProvider.GetAssociateInfo(ds.SentryOwnerName);

                            if (_event.UserWhoStartedEvent != ds.SentryOwnerName)
                            {
                                UserEvent ue;
                                if (userEvents.Any(x => x.email == user.WorkEmailAddress))
                                {
                                    ue = userEvents.FirstOrDefault(x => x.email == user.WorkEmailAddress);
                                    ue.events.Add(_event);
                                }
                                else
                                {
                                    ue = new UserEvent();
                                    ue.events = new List<Event>();
                                    ue.events.Add(_event);
                                    ue.email = user.WorkEmailAddress;

                                    userEvents.Add(ue);
                                }
                            }
#if (DEBUG)
                        }
#endif
                        }
                    }

                    foreach (Event _event in events)
                    {
                        _event.IsProcessed = true;
                    }

                    EmailService es = new EmailService();

                    foreach (UserEvent ue in userEvents)
                    {
                        es.SendEmail(ue.email, "Test Events", ue.events);
                    }

                    _datasetContext.SaveChanges();

                    //Event e = new Event();
                    //e.EventType_Desc = "Created File";
                    //e.Status_Desc = "Progress";
                    //e.TimeCreated = DateTime.Now;
                    //e.TimeNotified = DateTime.Now;
                    //e.IsProcessed = false;
                    //e.UserWhoStartedEvent = "082698";
                    //e.DataFile_ID = 1; //  <-- Change this to whatever you want, Dataset, Datafile
                    //e.Reason = "";

                    //await Utilities.CreateEventAsync(e);
                }
            }
            catch(Exception ex)
            {
                Logger.Error("", ex);
            }
            

        }
    }
}
