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
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, configId, null,  0, 0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByDatasetId(string eventType, string userId, string reason, int datasetId)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, datasetId, 0, null, 0, 0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByDataAsset(string eventType, string userId, string reason, int dataAssetId, string lineCde = null, string search = null)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, 0, null, dataAssetId, 0, lineCde, search), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEvent(string eventType, string userId, string reason, string lineCde = null, string search = null)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, 0, 0, null, 0, 0, lineCde, search), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventByNotificationId(string eventTypeDescription, string userId, string reason, Notification notification)
        {
            Task.Factory.StartNew(() => SaveEvent(eventTypeDescription, userId, reason, 0, 0, notification,  0, 0, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void PublishSuccessEventBySchemaId(string eventType, string userId, string reason, int datasetId, int schemaId)
        {
            Task.Factory.StartNew(() => SaveEvent(eventType, userId, reason, datasetId, 0, null, 0, schemaId, null, null), TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private void SaveEvent(string eventType, string userId, string reason, int datasetId, int configId, Notification notification, int dataAssetId, int schemaId, string lineCde = null, string search = null)
        {
            using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
            {
                EventType et = _datasetContext.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();
                Status status = _datasetContext.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault();

                if (datasetId == 0 && configId != 0)
                {
                    datasetId = _datasetContext.GetById<DatasetFileConfig>(configId).ParentDataset.DatasetId;
                }

                GetConfigIdAndReason(eventType,datasetId, schemaId, ref configId, ref reason);
                Event evt = CreateEvent(et, status, userId, reason, datasetId, configId, notification, dataAssetId, schemaId, lineCde, search);
                _datasetContext.Add(evt);
                _datasetContext.SaveChanges();
            }
        }

        //GET CONFIGID AND CREATE A REASON FOR EVENT
        private void GetConfigIdAndReason(string eventType, int datasetId, int schemaId, ref int configId, ref string reason)
        {
            configId = 0;

            using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
            {
                if (configId == 0 && datasetId != 0)
                {
                    Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
                    Schema schema = _datasetContext.GetById<Schema>(schemaId); 

                    DatasetFileConfig dfc;
                    if (ds != null)
                    {
                        if (schemaId == 0)
                        {
                            dfc = ds.DatasetFileConfigs.FirstOrDefault();
                        }
                        else
                        {
                            dfc = ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId);
                        }

                        reason = CreateReason(eventType, ds, schema);
                    }
                    else
                    {
                        dfc = null;
                    }

                    configId = (dfc == null) ? 0 : dfc.ConfigId;
                }
            }
        }

        //CREATE A DETAILED REASON HERE ONE TIME SO THIS CAN BE USED BY DataFeedProvider and EmailService
        private string CreateReason(string eventType, Dataset ds, Schema schema)
        {
            string reason = eventType;      //DEFAULT TO eventType in case none of the overrides happen below

            if (schema != null && eventType == GlobalConstants.EventType.CREATE_DATASET_SCHEMA)        
            {
                //SCHEMA SCENARIO
                reason = "A new schema called " + schema.Name + " was created under " + ds.DatasetName + " in " + ds.DatasetCategories.First().Name;
            }
            else if(eventType == GlobalConstants.EventType.CREATED_REPORT || eventType == GlobalConstants.EventType.CREATED_DATASET)        
            {
                //DATASET OR SCHEMA
                string whatAmI = (eventType == GlobalConstants.EventType.CREATED_REPORT) ? "exhibit" : "dataset";

                if (ds.DatasetCategories != null)
                {
                    reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created in " + ds.DatasetCategories.First().Name;
                }
                else
                {
                    reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created";
                }
            }

            return reason;
        }


        private Event CreateEvent(EventType eventType, Status status, string userId, string reason, int? datasetId, int? configId, Notification notification, int? dataAssetId,  int? schemaId, string lineCde, string search)
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
                Notification = notification,
                DataAsset = dataAssetId,
                SchemaId = schemaId,
                Line_CDE = lineCde,
                Search = search
            };
        }

    }
}
