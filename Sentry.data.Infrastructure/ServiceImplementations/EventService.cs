using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sentry.data.Infrastructure
{
    public class EventService : IEventService
    {
        public void PublishSuccessEventByConfigId(string eventTypeDescription, string userId, string reason, int configId)
        {
            IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>();
            int datasetId = _datasetContext.GetById<DatasetFileConfig>(configId).ParentDataset.DatasetId;
            
            Task.Factory.StartNew(() => PublishSuccessEvent(eventTypeDescription, userId, reason, datasetId, configId, 0), TaskCreationOptions.RunContinuationsAsynchronously);
        }


        public void PublishSuccessEventByDatasetId(string eventTypeDescription, string userId, string reason, int datasetId)
        {
            IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>();

            //Need to ensure object has not been deleted (i.e. Deleted Report event will be submitted after successfuly deleted)
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
            DatasetFileConfig dfc = (ds == null) ? null : ds.DatasetFileConfigs.FirstOrDefault();
            int configId = (dfc == null) ? 0 : dfc.ConfigId;

            Task.Factory.StartNew(() => PublishSuccessEvent(eventTypeDescription, userId, reason, datasetId, configId, 0), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEvent(string eventTypeDescription, string userId, string reason)
        {
            Task.Factory.StartNew(() => PublishSuccessEvent(eventTypeDescription, userId, reason, 0, 0, 0), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByNotificationId(string eventTypeDescription, string userId, string reason, int notificationId)
        {
            Task.Factory.StartNew(() => PublishSuccessEvent(eventTypeDescription, userId, reason, 0, 0, notificationId), TaskCreationOptions.RunContinuationsAsynchronously);
        }


        private void PublishSuccessEvent(string eventTypeDescription, string userId, string reason, int datasetId, int configId, int notificationId)
        {
            using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
            {
                EventType et = _datasetContext.EventTypes.Where(w => w.Description == eventTypeDescription).FirstOrDefault();
                Status status = _datasetContext.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault();

                Event evt = CreateAndSaveEvent(et, status, userId, reason, datasetId, configId, notificationId);

                _datasetContext.Add(evt);
                _datasetContext.SaveChanges();
            }
        }

        
        private Event CreateAndSaveEvent(EventType eventType, Status status, string userId, string reason, int? datasetId, int? configId, int? notificationId)
        {
            return new Event()
            {
                EventType = eventType,
                Status = status,
                TimeCreated = DateTime.Now,
                TimeNotified = DateTime.Now,
                IsProcessed = false,
                DataConfig = configId,
                Dataset = datasetId,
                UserWhoStartedEvent = userId,
                Reason = reason,
                Notification = notificationId
            };
        }

    }
}
