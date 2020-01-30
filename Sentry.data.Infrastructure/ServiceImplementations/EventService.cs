using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sentry.data.Infrastructure
{
    public class EventService : IEventService
    {
        public void PublishSuccessEventByConfigId(string eventType, string userId, string reason, int configId)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, configId, 0,  0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByDatasetId(string eventType, string userId, string reason, int datasetId)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, datasetId, 0, 0, 0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByDataAsset(string eventType, string userId, string reason, int dataAssetId, string lineCde = null, string search = null)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, 0, 0, dataAssetId, lineCde, search), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEvent(string eventType, string userId, string reason, string lineCde = null, string search = null)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, 0, 0, 0, lineCde, search), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByNotificationId(string eventTypeDescription, string userId, string reason, int notificationId)
        {
            Task.Factory.StartNew(() => SaveEvent(eventTypeDescription, userId, reason, 0, 0, notificationId,  0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }


        private Event CreateEvent(EventType eventType, Status status, string userId, string reason, int? datasetId, int? configId, int? notificationId, int? dataAssetId, string lineCde, string search)
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
                Notification = notificationId,
                DataAsset = dataAssetId,
                Line_CDE = lineCde,
                Search = search
            };
        }

        private void SaveEvent(string eventType, string userId, string reason, int datasetId, int configId, int notificationId, int dataAssetId, string lineCde = null, string search = null)
        {
            using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
            {
                EventType et = _datasetContext.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();
                Status status = _datasetContext.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault();

                if (datasetId == 0 && configId != 0)
                {
                    datasetId = _datasetContext.GetById<DatasetFileConfig>(configId).ParentDataset.DatasetId;
                }
                if (configId == 0 && datasetId != 0)
                {
                    //Need to ensure object has not been deleted (i.e. Deleted Report event will be submitted after successfuly deleted)
                    Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
                    DatasetFileConfig dfc = (ds == null) ? null : ds.DatasetFileConfigs.FirstOrDefault();
                    configId = (dfc == null) ? 0 : dfc.ConfigId;
                }

                Event evt = CreateEvent(et, status, userId, reason, datasetId, configId, notificationId, dataAssetId, lineCde, search);

                _datasetContext.Add(evt);
                _datasetContext.SaveChanges();
            }
        }

    }
}
