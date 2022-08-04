using Sentry.data.Core;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sentry.data.Infrastructure
{
    public class EventService : IEventService
    {
        private readonly IInstanceGenerator _contextGenerator;
        private readonly IUserService _userService;

        public EventService(IInstanceGenerator contextGenerator, IUserService userService)
        {
            _contextGenerator = contextGenerator;
            _userService = userService;
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

        public async Task PublishSuccessEvent(string eventType, string reason, string search = null)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Search = search }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventByNotificationId(string eventType, string reason, Notification notification)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Notification = notification }).ConfigureAwait(false);
        }

        public async Task PublishSuccessEventBySchemaId(string eventType, string reason, int datasetId, int schemaId)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Dataset = datasetId, SchemaId = schemaId }).ConfigureAwait(false);
        }

        public async Task PublishEventByDatasetFileDelete(string eventType, string reason, int datasetId, int schemaId, string deleteDetail)
        {
            await SaveEvent(eventType, new Event() { Reason = reason, Dataset = datasetId, SchemaId = schemaId, DeleteDetail = deleteDetail }).ConfigureAwait(false);
        }

        public async Task PublishEventByDatasetFileDelete(string eventType, string reason, string deleteDetail)
        {
            await SaveEvent(eventType, new Event() { Reason = reason,  DeleteDetail = deleteDetail }).ConfigureAwait(false);
        }



        private Task SaveEvent(string eventType, Event evt)
        {
            string associateId = _userService.GetCurrentUser().AssociateId;

            return Task.Factory.StartNew(() => {
                using (IDatasetContext ctx = _contextGenerator.GenerateInstance<IDatasetContext>())
                {
                    evt.EventType = ctx.EventTypes.Where(w => w.Description == eventType).FirstOrDefault();
                    evt.Status = ctx.EventStatus.Where(w => w.Description == GlobalConstants.Statuses.SUCCESS).FirstOrDefault();
                    evt.UserWhoStartedEvent = associateId;

                    AddIdsAndReason(evt, ctx);

                    ctx.Add(evt);
                    ctx.SaveChanges();
                }
            });
        }

        private void AddIdsAndReason(Event evt, IDatasetContext ctx)
        {
            if (evt.Dataset.GetValueOrDefault() == 0 && evt.DataConfig.GetValueOrDefault() != 0)
            {
                DatasetFileConfig datasetFileConfig = ctx.GetById<DatasetFileConfig>(evt.DataConfig);
                evt.Dataset = datasetFileConfig.ParentDataset.DatasetId;
                evt.SchemaId = datasetFileConfig.Schema.SchemaId;
            }
            else if (evt.Dataset.GetValueOrDefault() != 0 && evt.DataConfig.GetValueOrDefault() == 0)
            {
                Dataset ds = ctx.GetById<Dataset>(evt.Dataset);

                if (ds != null)
                {
                    DatasetFileConfig dfc = evt.SchemaId.GetValueOrDefault() == 0 ? ds.DatasetFileConfigs.FirstOrDefault() : ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == evt.SchemaId);

                    if (dfc != null)
                    {
                        evt.DataConfig = dfc.ConfigId;
                    }

                    AddReason(evt, ds);
                }
            }
        }
        private void AddReason(Event evt, Dataset ds)
        {
            Schema schema = evt.SchemaId.GetValueOrDefault() != 0 ? ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == evt.SchemaId)?.Schema : null;

            if (schema != null && evt.EventType.Description == GlobalConstants.EventType.CREATE_DATASET_SCHEMA)
            {
                //SCHEMA SCENARIO
                evt.Reason = $"A new schema called {schema.Name} was created under {ds.DatasetName} in {ds.DatasetCategories.First().Name}";
            }
            else if (evt.EventType.Description == GlobalConstants.EventType.CREATED_REPORT || evt.EventType.Description == GlobalConstants.EventType.CREATED_DATASET)
            {
                //DATASET OR SCHEMA
                string whatAmI = (evt.EventType.Description == GlobalConstants.EventType.CREATED_REPORT) ? "exhibit" : "dataset";

                evt.Reason = $"A new {whatAmI} called {ds.DatasetName} was created";

                if (ds.DatasetCategories?.Any() == true)
                {
                    evt.Reason += $" in {ds.DatasetCategories.First().Name}";
                }
            }
            else if(evt.EventType.Description == GlobalConstants.EventType.CREATED_FILE)
            {
                if(schema != null)
                {
                    evt.Reason = $"Data has been added for {schema.Name} under {ds.DatasetName} ";
                }
                else
                {
                    evt.Reason = $"Data has been added under {ds.DatasetName} ";
                }
                
            }
            else if(evt.EventType.Description == GlobalConstants.EventType.DATASETFILE_DELETE_S3)
            {
                evt.Reason = $"{GlobalConstants.EventType.DATASETFILE_DELETE_S3} step has been submitted successfully belonging to {ds.DatasetName}.";
            }
            else if (evt.EventType.Description == GlobalConstants.EventType.DATASETFILE_UPDATE_OBJECT_STATUS)
            {
                evt.Reason = $"{GlobalConstants.EventType.DATASETFILE_UPDATE_OBJECT_STATUS} step has been completed successfully.";
            }
        }
    }
}
