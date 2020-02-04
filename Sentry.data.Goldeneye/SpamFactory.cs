﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.Common.Logging;
using StructureMap;
using Hangfire;

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

        public static void Run(string interval)
        {
            try
            {
                IContainer container;
                IDatasetContext _datasetContext;
                IAssociateInfoProvider _associateInfoProvider;

                using (container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    _datasetContext = container.GetInstance<IDatasetContext>();
                    _associateInfoProvider = container.GetInstance<IAssociateInfoProvider>();
                    IEmailService es = container.GetInstance<IEmailService>();

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
                        if (_event.UserWhoStartedEvent != null && int.TryParse(_event.UserWhoStartedEvent.Trim(), out n) && _event.IsProcessed == false)
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
                        -IF AUTHOR is SUBSCRIBED, SEND AS WELL!!!
                        //********************************************************************************************************************************/
                        var subsThatMatch = _datasetContext.GetAllSubscriptions().Where(w =>    w.Dataset.DatasetId     == _event.Dataset
                                                                                                && w.EventType.Type_ID  == _event.EventType.Type_ID
                                                                                                && w.Interval           == _datasetContext.GetInterval(interval) );
                        foreach (DatasetSubscription ds in subsThatMatch)
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

