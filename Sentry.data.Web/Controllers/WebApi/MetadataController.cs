using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace Sentry.data.Web.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_METADATA)]
    public class MetadataController : BaseWebApiController
    {
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;
        private IConfigService _configService;

        public MetadataController(MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, 
                                IAssociateInfoProvider associateInfoService, UserService userService,
                                IConfigService configService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
            _configService = configService;
        }

        public class OutputSchema
        {
            public List<SchemaRow> rows { get; set; }
            public int RowCount { get; set; }
            public string HiveTableName { get; set; }
            public string HiveDatabaseName { get; set; }
            public int FileExtension { get; set; }
        }

        public class Metadata
        {
            public double DataLastUpdated { get; set; }
            public string Description { get; set; }
            public DropLocation DFSDropLocation { get; set; }
            public string DFSCronJob { get; set; }

            public DropLocation S3DropLocation { get; set; }
            public string S3CronJob { get; set; }

            public List<DropLocation> OtherJobs { get; set; }
            public List<string> CronJobs { get; set; }

         //   public int Views { get; set; }
         //   public int Downloads { get; set; }
        }

        public class DropLocation
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public int JobId { get; set; }
            public Boolean IsEnabled { get; set; }
        }

        /// <summary>
        /// gets schema metadata
        /// </summary>
        /// <param name="SchemaID">Schema Id assigned to given schema</param>
        /// <returns></returns>
        [HttpGet]
        [Route("schemas/{SchemaID}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK,null,typeof(SchemaModel))]
        public async Task<IHttpActionResult> GetBasicMetadataInformationForSchema(int SchemaID)
        {
            SchemaDTO dto = _configService.GetSchemaDTO(SchemaID);
            SchemaModel sm = new SchemaModel(dto);
            return Ok(sm);            
        }

        /// <summary>
        /// gets dataset metadata
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{DatasetConfigID}")]
        public async Task<IHttpActionResult> GetBasicMetadataInformationFor(int DatasetConfigID)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            return await GetMetadata(config);
        }


        private async Task<IHttpActionResult> GetMetadata(DatasetFileConfig config)
        {
            try
            {
                Event e = new Event();
                e.EventType = _dsContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                e.Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = RequestContext.Principal.Identity.Name;
                e.DataConfig = config.ConfigId;
                e.Reason = "Viewed Schema for Dataset";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                Metadata m = new Metadata();

                m.Description = config.Description;
                //m.DFSDropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<DfsBasic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();

                //m.Views = _dsContext.Events.Where(x => x.Reason == "Viewed Schema for Dataset" && x.DataConfig == DatasetConfigID).Count();
                //m.Downloads = _dsContext.Events.Where(x => x.EventType.Description == "Downloaded Data File" && x.DataConfig == DatasetConfigID).Count();

                if (config.DatasetFiles.Any())
                {
                    m.DataLastUpdated = config.DatasetFiles.Max(x => x.ModifiedDTM).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                }

                if (config.RetrieverJobs.Any(x => x.DataSource.Is<S3Basic>()))
                {
                   // m.S3DropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<S3Basic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();
                }

               
                if (config.RetrieverJobs.Any(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()))
                {
                    m.OtherJobs = config.RetrieverJobs.Where(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()).OrderBy(x => x.Id)
                        .Select(x => new DropLocation() { Location = x.IsEnabled ? x.Schedule : "Disabled", Name = x.DataSource.SourceType, JobId = x.Id, IsEnabled = x.IsEnabled }).ToList();
                }


                return Ok(m);

            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// gets primary hive table
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <param name="SchemaID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{DatasetConfigID}/schemas/{SchemaID}/hive")]
        public async Task<IHttpActionResult> GetPrimaryHiveTableFor(int DatasetConfigID, int SchemaID = 0)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            if (config.Schema.Any(x => x.SchemaIsPrimary))
            {
                DataElement schemarev = config.Schema.FirstOrDefault(x => x.SchemaIsPrimary);

                if (schemarev.HiveTable != null)
                {
                    return Ok(new { HiveDatabaseName = schemarev.HiveDatabase, HiveTableName = schemarev.HiveTable });
                }
                else
                {
                    return NotFound();
                }
                
            }
            else {
                return NotFound();
            }
        }


        /// <summary>
        /// gets column schema metadata
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <param name="SchemaID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{DatasetConfigID}/schemas/{SchemaID}/columns")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(OutputSchema))]
        public async Task<IHttpActionResult> GetColumnSchemaInformationFor(int DatasetConfigID, int SchemaID = 0)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            return await GetColumnSchema(config, SchemaID);
        }


        /// <summary>
        /// ges column schema metadate form schema
        /// </summary>
        /// <param name="SchemaID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("schemas/{SchemaID}/columns")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaDetailModel))]
        public async Task<IHttpActionResult> GetColumnSchemaInformationForSchema(int SchemaID)
        {
            SchemaDetailDTO dto = _configService.GetSchemaDetailDTO(SchemaID);
            SchemaDetailModel sdm = new SchemaDetailModel(dto);

            return Ok(sdm);
        }



        private async Task<IHttpActionResult> GetColumnSchema(DatasetFileConfig config, int SchemaID)
        {
            try
            {
                if (config.Schema.Any())
                {
                    var a = config.Schema.ToList();

                    DataElement schema = null;

                    if (SchemaID != 0)
                    {
                        schema = config.Schema.Where(x => x.DataElement_ID == SchemaID).FirstOrDefault();
                    }
                    else
                    {
                        if (config.Schema.Any(x => x.SchemaIsPrimary))
                        {
                            schema = config.Schema.Where(x => x.SchemaIsPrimary).OrderBy(x => x.SchemaRevision).FirstOrDefault();
                        }
                        else
                        {
                            schema = config.Schema.OrderBy(x => x.SchemaRevision).FirstOrDefault();
                        }
                    }

                    if (schema.DataObjects.Count > 0)
                    {
                        OutputSchema s = new OutputSchema();

                        s.FileExtension = config.FileExtension.Id;

                        s.rows = new List<SchemaRow>();

                        if (schema.DataObjects.Any(x => x.RowCount != 0))
                        {
                            s.RowCount = Convert.ToInt32(schema.DataObjects.FirstOrDefault().RowCount);
                        }

                        foreach (DataObjectField b in schema.DataObjects.FirstOrDefault().DataObjectFields)
                        {
                            SchemaRow r = new SchemaRow()
                            {
                                Name = b.DataObjectField_NME,
                                DataObjectField_ID = b.DataObjectField_ID,
                                Description = b.DataObjectField_DSC,
                                LastUpdated = b.LastUpdt_DTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
                            };

                            r.DataType = (!String.IsNullOrEmpty(b.DataType)) ? b.DataType.ToUpper() : "VARCHAR";
                            if (b.Precision != null) { r.Precision = b.Precision ?? null; }
                            if (b.Scale != null) { r.Scale = b.Scale ?? null; }
                            //r.Precision = (b.Precision != null && !String.IsNullOrEmpty(b.Precision)) ? b.Precision : null;
                            //r.Scale = (b.Scale != null && !String.IsNullOrEmpty(b.Scale)) ? b.Scale : null;
                            if (b.Nullable != null) { r.Nullable = b.Nullable ?? null; }
                            if (b.Length != null) { r.Length = b.Length ?? null; }
                            if (b.OrdinalPosition != null) { r.Position = Int32.Parse(b.OrdinalPosition); }
                            s.rows.Add(r);
                        }

                        return Ok(s);
                    }
                    else
                    {
                        OutputSchema s = new OutputSchema();
                        s.rows = new List<SchemaRow>();
                        s.FileExtension = config.FileExtension.Id;
                        return Ok(s);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }







    }
}
