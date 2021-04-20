using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.Common.Logging;
using StructureMap;
using Hangfire;
using Sentry.Web.Security.Obsidian;

namespace Sentry.data.Goldeneye
{
    [Queue("spamfactory")]
    class SpamFactory
    {
        public class UserEvent
        {
            public string email { get; set; }

            public List<Event> events { get; set; }
        }

        /*********************************************************************************************************************************
        --Pass in current subscriptions and add in subscriptions for those part of the PersonalLines Critical notification AD Group (DSC_BA_PL_CRITICAL_NOTIFICATION)
        --If another Business Area is added later, need to modify if statement at beginning to allow entry and use correct HostSetting 
        *********************************************************************************************************************************/
        private static void ADAttack(List<Subscription> subs, Event e)
        {
            //ENSURE that we are ONLY doing PersonalLines right now, if they create a TDM Notification or some other Notication we do not want to do the ADAttack
            if( ( (BusinessAreaType)e.Notification.ParentObject == BusinessAreaType.PersonalLines)  && 
                e.Notification.MessageSeverity == data.Core.GlobalEnums.NotificationSeverity.Critical                 
            )
            {
                ObsidianAdService o = new ObsidianAdService();
                o.Url = Configuration.Config.GetHostSetting("ObsidianAdServiceUrl");

                cdtGetUsersByGroupRequest oRequest = new cdtGetUsersByGroupRequest();
                oRequest.LogicalDomainName = "Intranet";

                //determine which BusinessAreaType it is based on Notification ParentObject
                if((BusinessAreaType) e.Notification.ParentObject == BusinessAreaType.PersonalLines)
                {
                    oRequest.GroupNameQuery = Configuration.Config.GetHostSetting("DSC_BA_PL_CRITICAL_NOTIFICATION");       //PersonalLines AD GROUP
                }

                if(oRequest.GroupNameQuery != null)
                {
                    cdtGetUsersByGroupResponse users = o.GetUsersByGroup(oRequest);
                    foreach (cdtLDAPUser user in users.UserList)
                    {
                        //check to see if this user is already in the list, if they are, we don't want to add them again
                        List<Subscription> userSubs = subs.Where(w => w.SentryOwnerName == user.UserEmployeeId && w.Interval.Interval_ID == 1).ToList();
                        if (userSubs.Count == 0)
                        {
                            Subscription newSub = new Subscription()
                            {
                                SentryOwnerName = user.UserEmployeeId
                            };

                            subs.Add(newSub);
                        }
                    }
                }
            }
        }


        public static void Run(string interval)
        {
            try
            {
                IContainer container;
                IDatasetContext _datasetContext;
                IAssociateInfoProvider _associateInfoProvider;

                INotificationService _notificationService;


                

                using (container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    _datasetContext = container.GetInstance<IDatasetContext>();
                    _associateInfoProvider = container.GetInstance<IAssociateInfoProvider>();
                    IEmailService es = container.GetInstance<IEmailService>();
                    _notificationService = container.GetInstance<NotificationService>();

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
#if (DEBUG)
                        if (_event.UserWhoStartedEvent != null && (_event.UserWhoStartedEvent == "082698" || _event.UserWhoStartedEvent == "072984" || _event.UserWhoStartedEvent == "072186"))
#else
                        /*********************************************************************************************************************************
                        STEP 1:  GENERATE AUTHOR UserEvent
                        -CREATE UserEvents for UserWhoStarted (Author) ONLY ONE TIME!!!   
                        *********************************************************************************************************************************/
                        int n;
                        bool authorProcessed = false;
                        if (_event.UserWhoStartedEvent != null && int.TryParse(_event.UserWhoStartedEvent.Trim(), out n) && !_event.IsProcessed)
#endif
                        {
                            Console.WriteLine("UserWhoStartedEvent : " + _event.UserWhoStartedEvent);
                            var user = _associateInfoProvider.GetAssociateInfo(_event.UserWhoStartedEvent.Trim());
                            authorProcessed = true;

                            UserEvent ue;
                            //attach Event to existing UserEvent if it already exists
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


                        /*********************************************************************************************************************************
                        STEP 2: GENERATE Subscriber UserEvent
                        -Create UserEvents for people that subscribed to the _event generated by the UserWhoStarted (Author)
                        -IF AUTHOR is SUBSCRIBED, SEND AS WELL (ONLY IF they didn't get UserEvent from above)
                        //********************************************************************************************************************************/

                        //Create a new List here in the event there are no subscriptions anywhere so foreach loop below doesn't get an exception
                        List<Subscription> subsThatMatch = new List<Subscription>();
                        if (_event.EventType.Group == EventTypeGroup.DataSet.GetDescription())                                              //DATASET
                        {
                            subsThatMatch.AddRange
                            (
                                _datasetContext.GetAllSubscriptionsForReal().Where
                                (w =>
                                    (w as DatasetSubscription)?.Dataset.DatasetId == _event.Dataset
                                    && w.EventType.Type_ID == _event.EventType.Type_ID
                                    && w.Interval == _datasetContext.GetInterval(interval)
                                ).ToList()
                            );
                        }
                        else if (_event.EventType.Group == EventTypeGroup.BusinessArea.GetDescription() && _event.Notification != null)     //BUSINESSAREA
                        {
                            subsThatMatch.AddRange
                            (
                                _datasetContext.GetAllSubscriptionsForReal().Where
                                (w =>
                                         (w as BusinessAreaSubscription)?.BusinessAreaType == (BusinessAreaType)_event.Notification.ParentObject
                                         && w.EventType.Description == _notificationService.FindEventTypeParent(_event.EventType).Description
                                         && w.Interval == _datasetContext.GetInterval(interval)

                                ).ToList()
                            );

                            //Call ADAttack for INSTANT BUSINESSAREA EVENTS ONLY to determine if we should email anyone in the CriticalNotificationsADGroup
                            if(interval == "Instant")
                            {
                                ADAttack(subsThatMatch, _event);
                            }
                            
                        }

                        

                        foreach (Subscription ds in subsThatMatch)
                        {
#if (DEBUG)
                         if (ds.SentryOwnerName == "082698" || ds.SentryOwnerName == "072984" || ds.SentryOwnerName == "072186")
#endif
                            Console.WriteLine("ds.SentryOwnerName : " + ds.SentryOwnerName);
                            var user = _associateInfoProvider.GetAssociateInfo(ds.SentryOwnerName);

                            //UserEvent for Subscribers OR AuthorSubscribers who didn't process yet
                            if
                            (   _event.UserWhoStartedEvent.Trim() != ds.SentryOwnerName.Trim()
                                || (_event.UserWhoStartedEvent.Trim() == ds.SentryOwnerName.Trim() && !authorProcessed) 
                            )
                            {
                                UserEvent ue;
                                //attach Event to existing UserEvent if a personal already has one, essentially there is an ARRAY of events
                                if (userEvents.Any(x => x.email == user.WorkEmailAddress))
                                {
                                    ue = userEvents.FirstOrDefault(x => x.email == user.WorkEmailAddress);
                                    ue.events.Add(_event);
                                }
                                else
                                {
                                    try
                                    {
                                        ue = new UserEvent();
                                        ue.events = new List<Event>();
                                        ue.events.Add(_event);
                                        ue.email = user.WorkEmailAddress;
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error("Error creating UserEvent", ex);
                                        throw;
                                    }                                   
                                    userEvents.Add(ue);
                                }
                            }
#if (DEBUG)
                        }
#endif
                        }
                    }

                    //STEP 3: MARK EVENTS as PROCESSED
                    foreach (Event _event in events)
                    {
                        if (_event.IsProcessed == false)
                        {
                            _event.IsProcessed = true;
                            _event.TimeNotified = DateTime.Now;
                        }
                    }

                    //STEP 4: SEND UserEvent EMAILS
                    foreach (UserEvent ue in userEvents)
                    {
                        Logger.Debug($"{ue.email} is being sent {ue.events.Count} events.");
                        es.SendEmail(ue.email, interval," Events", ue.events);
                        Console.WriteLine(ue.email + " is being sent " + ue.events.Count + " events.");
                    }

                    //Committing event changes to database
                    _datasetContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("", ex);
                Console.Write(ex);
            }
        }
    }
}

