using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Messaging.Common;
using System;
using System.Threading.Tasks;
using Hangfire;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class DfsEventHandler : IMessageHandler<string>
    {
        #region Constructor
        public DfsEventHandler() { }
        #endregion
               
        void IMessageHandler<string>.Handle(string msg)
        {
            BaseEventMessage baseEvent = JsonConvert.DeserializeObject<BaseEventMessage>(msg);
            try
            {
                if (baseEvent.EventType.ToUpper() == "DFSEVENT")
                {
                    DfsEvent DfsEventMsg = JsonConvert.DeserializeObject<DfsEvent>(msg);
                    Logger.Info("DfsEventHandler processing DFSEVENT message: " + JsonConvert.SerializeObject(DfsEventMsg));

                    string fileFullPath = Path.GetFileName(DfsEventMsg.PayLoad.FullPath);
                    //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop location
                    BackgroundJob.Enqueue<RetrieverJobService>(RetrieverJobService => RetrieverJobService.RunRetrieverJob(DfsEventMsg.PayLoad.JobId, JobCancellationToken.Null, fileFullPath));
                }
                else
                {
                    Logger.Info($"DfsEventHandler not configured to handle {baseEvent.EventType.ToUpper()} event type, skipping event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"DfsEventHandler failed to process message: EventType:{baseEvent.EventType.ToUpper()} - Msg:({msg})", ex);
            }
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("DfsEventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("DfsEventHandlerInitialized");
        }

        private void TaskException(Task t)
        {
            Logger.Fatal("Exception occurred on main Windows Service Task. Stopping Service immediately.", t.Exception);
        }
    }
}
