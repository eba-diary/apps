using Sentry.data.Core;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sentry.data.Infrastructure
{
    public class EventService : IEventService
    {
        private readonly IDatasetContext _context;
        private readonly IUserService _userService;
        private readonly Lazy<Status> _successStatus;

        public EventService(IDatasetContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
            _successStatus = new Lazy<Status>(() => _context.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault());
        }

        public async Task PublishSuccessEventByConfigId(string eventType, string reason, int configId)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, DataConfig = configId }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventByDatasetId(string eventType, string reason, int datasetId)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Dataset = datasetId }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventByDataAsset(string eventType, string reason, int dataAssetId, string lineCde, string search)
{
            await SaveEvent(eventType, new Event() { Reason = reason, DataAsset = dataAssetId, Line_CDE = lineCde, Search = search }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEvent(string eventType, string reason, string lineCde = null, string search = null)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Line_CDE = lineCde, Search = search }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventByNotificationId(string eventType, string reason, Notification notification)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Notification = notification }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventBySchemaId(string eventType, string reason, int datasetId, int schemaId)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Dataset = datasetId, SchemaId = schemaId }).ConfigureAwait(false);
        }

        private Task SaveEvent(string eventType, Event evt)
        {
            return Task.Factory.StartNew(() => {
                evt.EventType = _context.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();
                evt.Status = _successStatus.Value;
                evt.UserWhoStartedEvent = _userService.GetCurrentUser().AssociateId;

                if (evt.Dataset.GetValueOrDefault() == 0 && evt.DataConfig.GetValueOrDefault() != 0)
                {
                    evt.Dataset = _context.GetById<DatasetFileConfig>(evt.DataConfig).ParentDataset.DatasetId;
                }

                AddConfigIdAndReason(evt);

                _context.Add(evt);
                _context.SaveChanges();
            });
        }

        private void AddConfigIdAndReason(Event evt)
        {
            if (evt.DataConfig == 0 && evt.Dataset.GetValueOrDefault() != 0)
            {
                Dataset ds = _context.GetById<Dataset>(evt.Dataset);

                DatasetFileConfig dfc = null;
                if (ds != null)
                {
                    dfc = evt.SchemaId.GetValueOrDefault() == 0
                        ? ds.DatasetFileConfigs.FirstOrDefault()
                        : ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == evt.SchemaId);

                    AddReason(evt, ds);
                }

                evt.DataConfig = dfc == null ? 0 : dfc.ConfigId;
            }
        }
        private void AddReason(Event evt, Dataset ds)
        {
            string reason = evt.EventType.Description;      //DEFAULT TO eventType in case none of the overrides happen below
            Schema schema = _context.GetById<Schema>(evt.SchemaId);

            if (schema != null && evt.EventType.Description == GlobalConstants.EventType.CREATE_DATASET_SCHEMA)
            {
                //SCHEMA SCENARIO
                reason = "A new schema called " + schema.Name + " was created under " + ds.DatasetName + " in " + ds.DatasetCategories.First().Name;
            }
            else if (evt.EventType.Description == GlobalConstants.EventType.CREATED_REPORT || evt.EventType.Description == GlobalConstants.EventType.CREATED_DATASET)
            {
                //DATASET OR SCHEMA
                string whatAmI = (evt.EventType.Description == GlobalConstants.EventType.CREATED_REPORT) ? "exhibit" : "dataset";

                if (ds.DatasetCategories != null)
                {
                    reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created in " + ds.DatasetCategories.First().Name;
                }
                else
                {
                    reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created";
                }
            }

            evt.Reason = reason;
        }

        //private void SaveEvent(string eventType, string userId, string reason, int datasetId, int configId, Notification notification, int dataAssetId, int schemaId, string lineCde = null, string search = null)
        //{
        //    Task.Factory.StartNew(() => {
        //        EventType et = _context.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();

        //        if (datasetId == 0 && configId != 0)
        //        {
        //            datasetId = _context.GetById<DatasetFileConfig>(configId).ParentDataset.DatasetId;
        //        }

        //        GetConfigIdAndReason(eventType, datasetId, schemaId, ref configId, ref reason);

        //        Event evt = new Event()
        //        {
        //            EventType = et,
        //            Status = _successStatus.Value,
        //            DataConfig = configId,
        //            Dataset = datasetId,
        //            UserWhoStartedEvent = userId,
        //            Reason = reason,
        //            Notification = notification,
        //            DataAsset = dataAssetId,
        //            SchemaId = schemaId,
        //            Line_CDE = lineCde,
        //            Search = search
        //        };

        //        _context.Add(evt);
        //        _context.SaveChanges();
        //    });
        //}

        //GET CONFIGID AND CREATE A REASON FOR EVENT
        //private void GetConfigIdAndReason(string eventType, int datasetId, int schemaId, ref int configId, ref string reason)
        //{
        //    configId = 0;

        //    using (IDatasetContext _datasetContext = Bootstrapper.Container.GetNestedContainer().GetInstance<IDatasetContext>())
        //    {
        //        if (configId == 0 && datasetId != 0)
        //        {
        //            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
        //            Schema schema = _datasetContext.GetById<Schema>(schemaId); 

        //            DatasetFileConfig dfc;
        //            if (ds != null)
        //            {
        //                if (schemaId == 0)
        //                {
        //                    dfc = ds.DatasetFileConfigs.FirstOrDefault();
        //                }
        //                else
        //                {
        //                    dfc = ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId);
        //                }

        //                reason = CreateReason(eventType, ds, schema);
        //            }
        //            else
        //            {
        //                dfc = null;
        //            }

        //            configId = (dfc == null) ? 0 : dfc.ConfigId;
        //        }
        //    }
        //}

        //CREATE A DETAILED REASON HERE ONE TIME SO THIS CAN BE USED BY DataFeedProvider and EmailService
        //private string CreateReason(string eventType, Dataset ds, Schema schema)
        //{
        //    string reason = eventType;      //DEFAULT TO eventType in case none of the overrides happen below

        //    if (schema != null && eventType == GlobalConstants.EventType.CREATE_DATASET_SCHEMA)        
        //    {
        //        //SCHEMA SCENARIO
        //        reason = "A new schema called " + schema.Name + " was created under " + ds.DatasetName + " in " + ds.DatasetCategories.First().Name;
        //    }
        //    else if(eventType == GlobalConstants.EventType.CREATED_REPORT || eventType == GlobalConstants.EventType.CREATED_DATASET)        
        //    {
        //        //DATASET OR SCHEMA
        //        string whatAmI = (eventType == GlobalConstants.EventType.CREATED_REPORT) ? "exhibit" : "dataset";

        //        if (ds.DatasetCategories != null)
        //        {
        //            reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created in " + ds.DatasetCategories.First().Name;
        //        }
        //        else
        //        {
        //            reason = "A new " + whatAmI + " called " + ds.DatasetName + " was Created";
        //        }
        //    }

        //    return reason;
        //}
    }
}
