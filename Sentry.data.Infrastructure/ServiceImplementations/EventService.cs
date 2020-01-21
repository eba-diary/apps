﻿using Sentry.Common.Logging;
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
            Task.Factory.StartNew(() => PublishSuccessEvent(eventType, userId, reason, 0, configId), TaskCreationOptions.RunContinuationsAsynchronously);
        }


        public void PublishSuccessEventByDatasetId(string eventType, string userId, string reason, int datasetId)
        {
            Task.Factory.StartNew(() => PublishSuccessEvent(eventType, userId, reason, datasetId, 0), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEvent(string eventType, string userId, string reason)
        {
            Task.Factory.StartNew(() => PublishSuccessEvent(eventType, userId, reason, 0, 0), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByNotificationId(string eventType, string userId, string reason, int notificationId)
        {
            Task.Factory.StartNew(() => PublishSuccessEventNotification(eventType, userId, reason, notificationId), TaskCreationOptions.RunContinuationsAsynchronously);
        }


        private void PublishSuccessEvent(string eventType, string userId, string reason, int datasetId, int configId)
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
                    DatasetFileConfig dfc = (ds == null)? null : ds.DatasetFileConfigs.FirstOrDefault();
                    configId = (dfc == null)? 0 : dfc.ConfigId;
                }

                Event evt = CreateAndSaveEvent(et, status, userId, reason, datasetId, configId);

                _datasetContext.Add(evt);
                _datasetContext.SaveChanges();
            }
        }

        private void PublishSuccessEventNotification(string eventType, string userId, string reason, int notificationId)
        {
            using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
            {
                EventType et = _datasetContext.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();
                Status status = _datasetContext.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault();

                
                Event evt = CreateAndSaveEvent(et, status, userId, reason, 0, 0);

                _datasetContext.Add(evt);
                _datasetContext.SaveChanges();
            }
        }






        private Event CreateAndSaveEvent(EventType eventType, Status status, string userId, string reason, int? datasetId, int? configId)
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
                Reason = reason
            };
        }

    }
}
