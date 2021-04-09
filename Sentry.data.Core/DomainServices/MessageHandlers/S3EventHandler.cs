using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Sentry.data.Core
{
    public class S3EventHandler : IMessageHandler<string>
    {
        #region Declarations
        private readonly IDataFlowProvider _dataFlowProvider;
        private readonly IDataFeatures _dataFeatures;
        #endregion

        #region Constructor
        public S3EventHandler(IDataFlowProvider dataFlowProvider, IDataFeatures dataFeatures)
        {
            _dataFlowProvider = dataFlowProvider;
            _dataFeatures = dataFeatures;
        }
        #endregion

        void IMessageHandler<string>.Handle(string msg)
        {
            throw new NotImplementedException();
        }

        async Task IMessageHandler<string>.HandleAsync(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper() == "S3EVENT")
                {
                    S3Event s3Event = JsonConvert.DeserializeObject<S3Event>(msg);
                    Logger.Info("s3eventhandler processing S3EVENT message: " + JsonConvert.SerializeObject(s3Event));

                    switch (s3Event.PayLoad.eventName.ToUpper())
                    {
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_PUT:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_POST:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COPY:
                        case GlobalConstants.AWSEventNotifications.S3Events.OBJECTCREATED_COMPLETEMULTIPARTUPLOAD:
                            
                            if (!FilterEvent(s3Event))
                            {
                                Logger.Info($"S3EventHandler processing AWS event - {JsonConvert.SerializeObject(s3Event)}");
                                await _dataFlowProvider.ExecuteDependenciesAsync(s3Event.PayLoad.s3.bucket.name, s3Event.PayLoad.s3.Object.key, s3Event.PayLoad).ConfigureAwait(false);
                            }
                            else
                            {
                                Logger.Debug($"S3EventHandler skipped event due to refactoring");
                            }
                            break;
                        default:
                            Logger.Info($"s3eventhandler not configured to handle AWS event type ({s3Event.EventType}), skipping event.");
                            break;
                    }
                }
                else
                {
                    Logger.Info($"s3eventhandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"s3eventhandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }
        }

        private bool FilterEvent(S3Event s3Event)
        {
            
            /* If both refactoring featureflag have no value, then we do not want to filter event (return false) */
            if (string.IsNullOrEmpty(_dataFeatures.CLA2671_RefactorEventsToJava.GetValue()) && string.IsNullOrEmpty(_dataFeatures.CLA2671_RefactoredDataFlows.GetValue()))
            {
                return false;
            }

            string bucket = s3Event.PayLoad.s3.bucket.name;
            string key = s3Event.PayLoad.s3.Object.key;

            if (!string.IsNullOrEmpty(_dataFeatures.CLA2671_RefactorEventsToJava.GetValue()))
            {
                Logger.Debug($"s3eventhandler = Event refactoring is active (bucket:{bucket}:::key:{key}:::refactoredevents:{_dataFeatures.CLA2671_RefactorEventsToJava.GetValue()})");

                /* RefactorEventsToJava will contain data like the following
                    sentry-data-test-dataset-ae2||temp-file/raw,sentry-data-test-datasets-ae2||temp-file/s3drop
                 */
                var firstSplit = _dataFeatures.CLA2671_RefactorEventsToJava.GetValue().Split(',');
                List<Tuple<string, string>> eventFilterList = firstSplit.Select(item => item.Split(new string[] { "||" }, StringSplitOptions.None)).Select(t => Tuple.Create(t[0], t[1])).ToList();

                foreach (var item in eventFilterList)
                {
                    if (bucket == item.Item1 && key.StartsWith(item.Item2))
                    {
                        return true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_dataFeatures.CLA2671_RefactoredDataFlows.GetValue()))
            {
                Logger.Debug($"s3eventhandler = Dataflow refactoring is active (bucket:{bucket}:::key:{key}:::refactoreddataflows:{_dataFeatures.CLA2671_RefactoredDataFlows.GetValue()})");

                string[] dataflowList = _dataFeatures.CLA2671_RefactoredDataFlows.GetValue().Split(',');

                /* base on key, determine where the dataflow Id resides */
                int nthStrategy = DetermineParsingStrategy(bucket, key);

                //do not filter event if we do not find a parsing strategy
                if (nthStrategy == 0)
                {
                    return false;
                }

                /* Parse dataflow Id based on parsing strategy */
                int charIndex = Helpers.ParsingHelpers.GetNthIndex(key, '/', nthStrategy);
                var dataflowId = key.Substring(charIndex + 1, 7);

                if (dataflowList.Contains(dataflowId))
                {
                    return true;
                }
            }

            return false;
        }

        private int DetermineParsingStrategy(string bucket, string key)
        {
            if (key.StartsWith("temp-file/") || (bucket.StartsWith("sentry-data") && bucket.EndsWith("droplocation-ae2")))
            {
                return 3;
            }
            if (key.StartsWith("droplocation/"))
            {
                return 2;
            }

            return 0;
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("S3EventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("S3EventHandlerInitialized");
        }
    }
}
