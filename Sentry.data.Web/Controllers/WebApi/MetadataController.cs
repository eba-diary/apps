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
using Sentry.data.Web.Models.ApiModels.Dataset;
using Sentry.data.Web.Models.ApiModels.Config;
using Sentry.data.Web.Models.ApiModels.Schema;
using Sentry.data.Web.WebAPI;
using Sentry.WebAPI.Versioning;
using Sentry.Common.Logging;
using Newtonsoft.Json;

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
        private IDatasetService _datasetService;
        private ISchemaService _schemaService;
        private ISecurityService _securityService;

        public MetadataController(MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, 
                                IAssociateInfoProvider associateInfoService, UserService userService,
                                IConfigService configService, IDatasetService datasetService,
                                ISchemaService schemaService, ISecurityService securityService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
            _configService = configService;
            _datasetService = datasetService;
            _schemaService = schemaService;
            _securityService = securityService;
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

        #region Dataset_Endpoints
        /// <summary>
        /// List of all datasets
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<DatasetInfoModel>))]
        public async Task<IHttpActionResult> GetDatasets()
        {
            try
            {
                List<DatasetDto> dtoList = _datasetService.GetAllDatasetDto();
                List<DatasetInfoModel> modelList = dtoList.ToApiModel();
                return Ok(modelList);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getdatasets_internalservererror", ex);
                return InternalServerError();
            }
        }

        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/config")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<ConfigInfoModel>))]
        public async Task<IHttpActionResult> GetDatasetConfigs(int datasetId)
        {
            try
            {
                List<DatasetFileConfigDto> dtoList = _configService.GetDatasetFileConfigDtoByDataset(datasetId);
                if (dtoList == null)
                {
                    Logger.Info($"metadataapi_getdatasetconfigs_badrequest - datasetid:{datasetId}");
                }
                List<ConfigInfoModel> modelList = dtoList.ToModel();
                return Ok(modelList);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getdatasetconfigs_internalservererror", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Get list of schema for dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaInfoModel>))]
        public async Task<IHttpActionResult> GetSchemaByDataset(int datasetId)
        {
            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
            {
                return Unauthorized();
            }

            try
            {
                List<DatasetFileConfigDto> dtoList = _configService.GetDatasetFileConfigDtoByDataset(datasetId);
                List<SchemaInfoModel> modelList = dtoList.ToSchemaModel();
                return Ok(modelList);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getschemabydataset_internalservererror - datasetid:{datasetId}", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Get schema metadata
        /// </summary>
        /// <param name="SchemaID">Schema Id assigned to given schema</param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaInfoModel))]
        public async Task<IHttpActionResult> GetSchema(int datasetId, int schemaId)
        {
            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
            {
                return Unauthorized();
            }

            try
            {
                DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
                if (dto == null)
                {
                    return NotFound();
                }
                SchemaInfoModel model = dto.ToSchemaModel();
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Error($"metdataapi_getschema_internalserverserror - datasetId:{datasetId} schemaId{schemaId}", ex);
                return InternalServerError();
            }            
        }

        /// <summary>
        /// Get list of revisions for schema
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaRevisionModel>))]
        public async Task<IHttpActionResult> GetSchemaRevisionBySchema(int datasetId, int schemaId)
        {
            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
            {
                return Unauthorized();
            }

            try
            {
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Any(w => w.Schema.SchemaId == schemaId))
                {
                    Logger.Info($"metadataapi_getschemarevisionbyschema_notfound schema - datasetId:{datasetId} schemaId:{schemaId}");
                    return NotFound();
                }

                List<SchemaRevisionDto> revisionDto = _schemaService.GetSchemaRevisionDtoBySchema(schemaId);
                if (revisionDto == null)
                {
                    Logger.Info($"metadataapi_getschemarevisionbyschema_notfound revision - datasetId:{datasetId} schemaId:{schemaId}");
                    return NotFound();
                }
                List<SchemaRevisionModel> modelList = revisionDto.ToModel();
                return Ok(modelList);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getschemarevisionbyschema_internalservererror - datasetId:{datasetId} schemaId{schemaId}", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Get schema revision detail
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/{revisionId}/fields")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaRevisionDetailModel>))]
        public async Task<IHttpActionResult> GetSchemaRevision(int datasetId, int schemaId, int revisionId)
        {
            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
            {
                return Unauthorized();
            }

            try
            {
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Any(w => w.Schema.SchemaId == schemaId))
                {
                    Logger.Info($"metadataapi_getschemarevision_notfound - datasetid:{datasetId} schemaid:{schemaId}");
                    return NotFound();
                }

                SchemaRevisionDto revisiondto = _schemaService.GetSchemaRevisionDtoBySchema(schemaId).First(w => w.RevisionId == revisionId);
                if (revisiondto == null)
                {
                    Logger.Info($"metadataapi_getschemarevision_notfound - datasetid:{datasetId} schemaid:{schemaId} revisionid:{revisionId}");
                    return NotFound();
                }

                SchemaRevisionDetailModel revisionDetailModel = revisiondto.ToSchemaDetailModel();
                revisionDetailModel.fields = _schemaService.GetBaseFieldDtoBySchemaRevision(revisionId);
                return Ok(revisionDetailModel);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getschemarevision_badrequest - datasetid:{datasetId} schemaid:{schemaId}", ex);
                return InternalServerError();
            }
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

            if (config.Schemas.Any(x => x.SchemaIsPrimary))
            {
                DataElement schemarev = config.Schemas.FirstOrDefault(x => x.SchemaIsPrimary);

                if (schemarev.HiveTable != null)
                {
                    return Ok(new { HiveDatabaseName = schemarev.HiveDatabase, HiveTableName = schemarev.HiveTable });
                }
                else
                {
                    return NotFound();
                }

            }
            else
            {
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

        #endregion

        #region Schema_Endpoints
        
        /// <summary>
        /// Gets schema information
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaInfoModel))]
        public async Task<IHttpActionResult> GetSchemaInfo(int schemaId)
        {
            try
            {
                SchemaDto dto = _configService.GetSchemaDto(schemaId);
                SchemaInfoModel model = dto.ToModel();
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Get list all revisions for schema
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet]

        [Route("schema/{schemaId}/revisions")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaInfoModel>))]
        public async Task<IHttpActionResult> GetSchemaRevisions(int schemaId)
        {
            try
            {
                List<SchemaRevisionDto> revisionsList = _schemaService.GetSchemaRevisionDtoBySchema(schemaId);
                List<SchemaRevisionModel> modelList = revisionsList.ToModel();
                return Ok(modelList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
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
            SchemaDetaiApilDTO dto = _configService.GetSchemaDetailDTO(SchemaID);
            SchemaDetailModel sdm = new SchemaDetailModel(dto);

            return Ok(sdm);
        }
        #endregion

        #region Private Methods
        private async Task<IHttpActionResult> GetColumnSchema(DatasetFileConfig config, int SchemaID)
        {
            try
            {
                if (config.Schemas.Any())
                {
                    var a = config.Schemas.ToList();

                    DataElement schema = null;

                    if (SchemaID != 0)
                    {
                        schema = config.Schemas.Where(x => x.DataElement_ID == SchemaID).FirstOrDefault();
                    }
                    else
                    {
                        if (config.Schemas.Any(x => x.SchemaIsPrimary))
                        {
                            schema = config.Schemas.Where(x => x.SchemaIsPrimary).OrderBy(x => x.SchemaRevision).FirstOrDefault();
                        }
                        else
                        {
                            schema = config.Schemas.OrderBy(x => x.SchemaRevision).FirstOrDefault();
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

                            r.DataType = (!String.IsNullOrEmpty(b.DataType)) ? b.DataType.ToUpper() : SchemaDatatypes.VARCHAR.ToString();
                            if (b.Precision != null) { r.Precision = b.Precision ?? null; }
                            if (b.Scale != null) { r.Scale = b.Scale ?? null; }
                            //r.Precision = (b.Precision != null && !String.IsNullOrEmpty(b.Precision)) ? b.Precision : null;
                            //r.Scale = (b.Scale != null && !String.IsNullOrEmpty(b.Scale)) ? b.Scale : null;
                            if (b.Nullable != null) { r.Nullable = b.Nullable ?? null; }
                            if (b.Length != null) { r.Length = b.Length ?? null; }
                            if (b.OrdinalPosition != null) { r.Position = Int32.Parse(b.OrdinalPosition); }
                            if (b.FieldFormat != null) { r.Format = b.FieldFormat ?? null; }
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
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

    }
}
