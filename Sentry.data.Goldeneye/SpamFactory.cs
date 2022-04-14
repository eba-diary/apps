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

        private static void LogAttack(string severity, string message )
        {
            Console.WriteLine(message);

            if(severity.ToUpper() == "FATAL")
            {
                Logger.Fatal(message);
            }
            else
            {
                Logger.Info(message);
            }

        }

        //DETERMINE IF BUSINESSAREA PL
        private static bool IsBusinessArea(Event e)
        {
            return (e.EventType.Group == EventTypeGroup.BusinessArea.GetDescription() && e.Notification != null);
        }

        //DETERMINE IF BUSINESSAREA DSC : these will have a group = BUSINESSAREA_DSC and a valid Notification
        private static bool IsBusinessAreaDSC(Event e)
        {

            return (e.EventType.Group == EventTypeGroup.BusinessAreaDSC.GetDescription() && e.Notification != null);
        }

        //DETERMINE IF BUSINESSAREA DSC CHILDREN EVENT TYPES
        private static bool IsBusinessAreaDSCReleaseNotesNews(Event e)
        {

            return (    (   e.EventType.Group == EventTypeGroup.BusinessAreaDSCReleaseNotes.GetDescription() ||
                            e.EventType.Group == EventTypeGroup.BusinessAreaDSCNews.GetDescription()
                        )
                        && e.Notification != null);
        }

        //DETERMINE IF BUSINESSAREA DSC : these will have a group = BUSINESSAREA_DSC but will be EventTypes related to DATASET Events
        private static bool IsBusinessArea_DSC_Dataset(Event e)
        {
            return (e.EventType.Group == EventTypeGroup.BusinessAreaDSC.GetDescription()
                    && (e.EventType.Description == GlobalConstants.EventType.CREATED_DATASET || e.EventType.Description == GlobalConstants.EventType.CREATE_DATASET_SCHEMA || e.EventType.Description == GlobalConstants.EventType.CREATED_REPORT));
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
                    
                    LogAttack("INFO","Starting Spam Factry RUN " + interval);

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
                            LogAttack("INFO","SpamFactory EventID: " + _event.EventID + " Author:" + _event.UserWhoStartedEvent.Trim() );
                            var user = _associateInfoProvider.GetAssociateInfo(_event.UserWhoStartedEvent.Trim());
                            authorProcessed = true;

                            if (user != null)
                            {
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
                        else if ( IsBusinessArea(_event) || IsBusinessAreaDSC(_event) )     
                        {
                            subsThatMatch.AddRange
                            (
                                _datasetContext.GetAllSubscriptionsForReal().Where
                                (w =>
                                         (w as BusinessAreaSubscription)?.BusinessAreaType == (BusinessAreaType)_event.Notification.ParentObject
                                         && w.EventType.Description == _notificationService.FindEventTypeParent(_event.EventType).Description               //if they have a subscription to e.g. NOTIFICATION_CRITICAL and the _event is a NOTIFICATION_CRITICAL_ADD then they should still get an email
                                         && w.Interval == _datasetContext.GetInterval(interval)

                                ).ToList()
                            );

                            //Call ADAttack for INSTANT BUSINESSAREA EVENTS ONLY to determine if we should email anyone in the CriticalNotificationsADGroup
                            if (_event.Notification.ParentObject == (int)BusinessAreaType.PersonalLines && interval == "Instant")
                            {
                                ADAttack(subsThatMatch, _event);
                            }

                        }
                        else if (IsBusinessArea_DSC_Dataset(_event))
                        {
                            subsThatMatch.AddRange
                           (
                               _datasetContext.GetAllSubscriptionsForReal().Where
                               (w =>
                                        w.EventType.Description == _event.EventType.Description
                                        && w.Interval == _datasetContext.GetInterval(interval)

                               ).ToList()
                           );

                        }
                        else if (IsBusinessAreaDSCReleaseNotesNews(_event))
                        {
                            List<Subscription> subs = _datasetContext.GetAllSubscriptionsForReal();
                            subsThatMatch.AddRange( subs.Where(w => w.EventType == _event.EventType && w.Interval == _datasetContext.GetInterval(interval)).ToList() );
                        }



                        foreach (Subscription ds in subsThatMatch)
                        {
#if (DEBUG)
                         if (ds.SentryOwnerName == "082698" || ds.SentryOwnerName == "072984" || ds.SentryOwnerName == "072186")
#endif
                            LogAttack("INFO","SpamFactory: Subscription Processing:  EventID: " + _event.EventID + " Subscriber:" + ds.SentryOwnerName );
                            var user = _associateInfoProvider.GetAssociateInfo(ds.SentryOwnerName);

                            if (user != null)
                            {
                                LogAttack("INFO", "SpamFactory: Subscription Processing:  WorkEmailAddress:" + user.WorkEmailAddress);

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
                                            LogAttack("FATAL", "Spam Factory: Error processing Subscribers for event.  " + ex.InnerException);
                                            throw;
                                        }
                                        userEvents.Add(ue);
                                    }
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
                        if (!_event.IsProcessed)
                        {
                            _event.IsProcessed = true;
                            _event.TimeNotified = DateTime.Now;
                        }
                    }

                    //STEP 4: SEND UserEvent EMAILS
                    foreach (UserEvent ue in userEvents)
                    {
                        LogAttack("INFO", $"Preparing to send {ue.email}  {ue.events.Count} events.");
                        es.SendEmail(ue.email, interval, " Events", ue.events);
                        LogAttack("INFO", ue.email + " is being sent " + ue.events.Count + " events.");
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

