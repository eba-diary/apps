using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Sentry.Common.Logging;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Migration;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Models.ApiModels.Dataset;
using Sentry.data.Web.Models.ApiModels.Migration;
using Sentry.data.Web.Models.ApiModels.Schema;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_METADATA)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class MetadataController : BaseWebApiController
    {
        private readonly IDatasetContext _dsContext;
        private readonly UserService _userService;
        private readonly IConfigService _configService;
        private readonly IDatasetService _datasetService;
        private readonly ISchemaService _schemaService;
        private readonly ISecurityService _securityService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly Lazy<IDatasetFileService> _datasetFileService;
        private readonly Lazy<IDataApplicationService> _dataApplicationService;

        public MetadataController(IDatasetContext dsContext, UserService userService,
                                IConfigService configService, IDatasetService datasetService,
                                ISchemaService schemaService, ISecurityService securityService,
                                IMessagePublisher messagePublisher, Lazy<IDatasetFileService> datasetFileService,
                                Lazy<IDataApplicationService> dataApplicationService)
        {
            _dsContext = dsContext;
            _userService = userService;
            _configService = configService;
            _datasetService = datasetService;
            _schemaService = schemaService;
            _securityService = securityService;
            _messagePublisher = messagePublisher;
            _datasetFileService = datasetFileService;
            _dataApplicationService = dataApplicationService;
        }

        public IDatasetFileService DatasetFileService
        {
            get { return _datasetFileService.Value; }
        }

        public IDataApplicationService DataApplicationService
        {
            get { return _dataApplicationService.Value; }
        }

        #region Classes
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
            public Metadata()
            {
                DataFlows = new List<DataFlow>();
                HiveViews = new List<string>();
                SnowflakeViews = new List<string>();
            }
            public double DataLastUpdated { get; set; }
            public string Description { get; set; }
            public DropLocation DFSDropLocation { get; set; }
            public string DFSCronJob { get; set; }

            public DropLocation S3DropLocation { get; set; }
            public string S3CronJob { get; set; }

            public List<DropLocation> OtherJobs { get; set; }
            public List<string> CronJobs { get; set; }
            public List<DataFlow> DataFlows { get; set; }
            public string HiveDatabase { get; set; }
            public List<string> HiveViews { get; set; }
            public int SchemaId { get; set; }
            public int DatasetId { get; set; }
            public List<string> SnowflakeViews { get; set; }

            public string ControlMTriggerName { get; set; }

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

        public class KafkaMessage
        {
            public string Topic { get; set; }
            public string Key { get; set; }
            public string Message { get; set; }
        }

        public class DataFlow
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string DetailUrl { get; set; }
            public List<DropLocation> RetrieverJobs { get; set; }
            public bool PopulatesMultipleSchema { get; set; }
            public ObjectStatusEnum ObjectStatus { get; set; }
            public string DeleteIssuer { get; set; }
            public DateTime DeleteIssueDTM { get; set; }

            //USED BY KNOCKOUT TO DISPLAY TOPIC NAME ON _SchemaAbout.cshtml
            public int IngestionType { get; set; }
            public string TopicName { get; set; }
            public string S3ConnectorName { get; set; }

        }
        #endregion


        #region Dataset_Endpoints

        /* This code will be used within next two iterations*/
        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v20220609)]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(DatasetMigrationResponseModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, typeof(string[]))]
        [Route("MigrateDataset")]
        public async Task<IHttpActionResult> MigrateDataset([FromBody] Models.ApiModels.Migration.DatasetMigrationRequestModel model)
        {
            async Task<IHttpActionResult> MigrateDatasetFunction()
            {
                string methodName = $"{nameof(MetadataController).ToLower()}_{nameof(MigrateSchema).ToLower()}";

                List<string> errors = new List<string>();
                if (model == null)
                {
                    Logger.Debug($"{methodName} - Null {nameof(Models.ApiModels.Migration.DatasetMigrationRequestModel)}");
                    return BadRequest($"{nameof(Models.ApiModels.Migration.DatasetMigrationRequestModel)} is required");
                }

                Logger.Debug($"{methodName} - {JsonConvert.SerializeObject(model)}");

                DatasetMigrationRequest request = ToDto(model);

                DatasetMigrationResponseModel responseModel = new DatasetMigrationResponseModel();
                try
                {
                    DatasetMigrationRequestResponse response = await DataApplicationService.MigrateDataset(request);
                    responseModel = ToDatasetMigrationResponseModel(response);
                }
                catch (AggregateException ex)
                {
                    Logger.Info($"{methodName} - AggreateException thrown from {nameof(DataApplicationService)}.{nameof(MigrateDataset)}");
                    foreach (var innerEx in ex.InnerExceptions)
                    {
                        if (innerEx is ArgumentException)
                        {
                            errors.Add(innerEx.Message);
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    Logger.Debug($"{methodName} - {ex.Message}");
                    errors.Add(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"{methodName} - Unhandled exception : {ex.Message}");
                    throw;
                }

        /* This code will be used within next two iterations*/
        //[HttpPost]
        //[ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v20220609)]
        //[Route("dataset")]
        //public async Task<IHttpActionResult> MigrateSchema([FromBody] SchemaMigrationRequestModel model)
        //{
        //    IHttpActionResult MigrateDatasetFunction()
        //    {
        //        SchemaMigrationRequest request = model.ToDto();
        //        DataApplicationService.MigrateDataset(request);
        //        return Ok();
        //    }

        //    return ApiTryCatch(nameof(MetadataController), nameof(MigrateDatasetFunction), null, MigrateDatasetFunction);
        //}


        /// <summary>
        /// List of all datasets
        /// </summary>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<DatasetInfoModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetDatasets()
        {
            IHttpActionResult GetDatasetsFunction()
            {
                List<DatasetSchemaDto> dtoList = _datasetService.GetAllDatasetDto();
                List<DatasetInfoModel> modelList = dtoList.ToApiModel();
                return Ok(modelList);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetDatasetsFunction), null, GetDatasetsFunction);
        }

        /// <summary>
        /// Get list of schema for dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [ApiVersionEnd(Sentry.data.Web.WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaInfoModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchemaByDataset(int datasetId)
        {
            return await GetSchemaByDataset_Internal(datasetId, l => l.ToSchemaModel());
        }

        /// <summary>
        /// Get list of schema for dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<Models.ApiModels.Schema20220609.SchemaInfoModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchemaByDataset20220609(int datasetId)
        {
            return await GetSchemaByDataset_Internal(datasetId, l => l.ToSchemaModel20220609());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "False positive")]
        private async Task<IHttpActionResult> GetSchemaByDataset_Internal<T>(int datasetId, Func<List<DatasetFileConfigDto>,List<T>> func) where T : SchemaInfoModelBase
        {
            IHttpActionResult GetSchemaByDatasetFunction()
            {
                var dtoList = _configService.GetDatasetFileConfigDtoByDataset(datasetId);
                var modelList = func.Invoke(dtoList);
                modelList = modelList.OrderBy(x => x.Name).ToList();
                return Ok(modelList);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetSchemaByDataset), $"datasetid:{datasetId}", GetSchemaByDatasetFunction);
        }

        /// <summary>
        /// Get schema metadata
        /// </summary>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [ApiVersionEnd(Sentry.data.Web.WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaInfoModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchema(int datasetId, int schemaId)
        {
            return await GetSchema_Internal(datasetId, schemaId, d => d.ToSchemaModel());
        }

        /// <summary>
        /// Get schema metadata
        /// </summary>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(Models.ApiModels.Schema20220609.SchemaInfoModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchema20220609(int datasetId, int schemaId)
        {
            return await GetSchema_Internal(datasetId, schemaId, d => d.ToSchemaModel20220609());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "False positive")]
        private async Task<IHttpActionResult> GetSchema_Internal<T>(int datasetId, int schemaId, Func<DatasetFileConfigDto,T> func) where T:SchemaInfoModelBase
        {
            IHttpActionResult GetSchemaFunction()
            {
                DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
                if (dto == null)
                {
                    return Content(System.Net.HttpStatusCode.NotFound, "Schema not found");
                }
                T model = func.Invoke(dto);
                return Ok(model);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetSchema), $"datasetid:{datasetId} schemaId{schemaId}", GetSchemaFunction);
        }

        ///// <summary>
        ///// Return all data files associated with schema.
        ///// </summary>
        ///// <param name="datasetId"></param>
        ///// <param name="schemaId"></param>
        ///// <param name="pageNumber">Default is 1</param>
        ///// <param name="pageSize">Default is 10, Max is 100</param>
        ///// <returns></returns>
        //[HttpGet]
        //[ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        //[Route("~/api/v2/datafile/dataset/{datasetId}/schema/{schemaId}/datafile")]
        //[SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<DatasetFileModel>))]
        //public async Task<IHttpActionResult> GetSchemaDatasetFiles([FromUri] int datasetId, [FromUri] int schemaId, [FromUri] int pageNumber, [FromUri] int pageSize)
        //{
        //    IHttpActionResult GetSchemaDatasetFilesFunction()
        //    {
        //        //DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
        //        //List<DatasetFileDto> dtoList = DatasetFileService.GetAllDatasetFilesBySchema(schemaId, x => x.ParentDatasetFileId == null).ToList();

        //        PageParameters pagingParams = new PageParameters() { PageNumber = pageNumber, PageSize = pageSize };

        //        List<DatasetFileDto> dtoList = DatasetFileService.GetAllDatasetFilesBySchema(schemaId, pagingParams).ToList();

        //        List<DatasetFileModel> modelList = dtoList.ToModel();

        //        return Ok(modelList);
        //    }

        //    return ApiTryCatch("metdataapi", System.Reflection.MethodBase.GetCurrentMethod().Name, $"datasetid:{datasetId} schemaId{schemaId}", GetSchemaDatasetFilesFunction);
        //}

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
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchemaRevisionBySchema(int datasetId, int schemaId)
        {
            //ValidateViewPermissionsForDataset(datasetId);

            IHttpActionResult GetSchemaRevisionBySchemaFunction()
            {
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Where(w => !w.DeleteInd).Any(w => w.Schema.SchemaId == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                List<SchemaRevisionDto> revisionDto = _schemaService.GetSchemaRevisionDtoBySchema(schemaId);

                List<SchemaRevisionModel> modelList = revisionDto.ToModel();
                return Ok(modelList);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetSchemaRevisionBySchema), $"datasetid:{datasetId} schemaId{schemaId}", GetSchemaRevisionBySchemaFunction);
        }

        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(int))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, "Failed schema validation", typeof(List<string>))]
        public async Task<IHttpActionResult> AddSchemaRevision(int datasetId, int schemaId, string revisionName, [FromBody] JObject schemaStructure)
        {
            IHttpActionResult AddSchemaRevisionFunction()
            {
                Logger.Debug($"{nameof(MetadataController)} start method <{nameof(AddSchemaRevision)}>");
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Any(w => w.Schema.SchemaId == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                Logger.Debug($"{nameof(MetadataController)}_{nameof(AddSchemaRevision)} - datasetid:{datasetId}:::schemaId:{schemaId}:::incomingjson:{schemaStructure}");

                JsonSchema schema_v3;
                schema_v3 = deserializeJSONStringtoJsonSchema().GetAwaiter().GetResult();

                List<BaseFieldDto> schemarows_v2 = new List<BaseFieldDto>();
                try
                {
                    int rowCnt = 0;
                    Logger.Debug($"{nameof(MetadataController)} schema conversion to dsc structures starting...");
                    schema_v3.ToDto(schemarows_v2, ref rowCnt);
                    Logger.Debug($"{nameof(MetadataController)} schema conversion to dsc structures ended ");

                    if (!schemarows_v2.Any())
                    {
                        return Content(System.Net.HttpStatusCode.BadRequest, "Schema conversion resulted in 0 fields.  Schema not updated.");
                    }

                    Logger.Debug($"{nameof(MetadataController)} schema dsc validations starting...");
                    _schemaService.ValidateCleanedFields(schemaId, schemarows_v2);
                    Logger.Debug($"{nameof(MetadataController)} schema dsc validations ended");
                }
                catch (SchemaConversionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new SchemaConversionException($"Schema conversion failed", ex);
                }

                int savedRevisionId = _schemaService.CreateAndSaveSchemaRevision(schemaId, schemarows_v2, revisionName, schema_v3.ToJson());

                if (savedRevisionId == 0)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, "Unable to Save Revision");
                }

                Logger.Debug($"{nameof(MetadataController)} end method <{nameof(AddSchemaRevision)}>");
                return Ok(savedRevisionId);
            }

            async Task<JsonSchema> deserializeJSONStringtoJsonSchema()
            {
                try
                {
                    Logger.Debug($"{nameof(MetadataController)} start method <{nameof(deserializeJSONStringtoJsonSchema)}>");
                    JsonSchema result = await JsonSchema.FromJsonAsync(schemaStructure.ToString()).ConfigureAwait(false);
                    Logger.Debug($"{nameof(MetadataController)} end method <{nameof(deserializeJSONStringtoJsonSchema)}>");
                    return result;
                }
                catch (Exception ex)
                {
                    throw new SchemaConversionException($"Incoming json not properly formated", ex);
                }
            }

            return ApiTryCatch(nameof(MetadataController), nameof(AddSchemaRevision), $"datasetid:{datasetId} schemaId{schemaId}", AddSchemaRevisionFunction);
        }

        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("GenerateSchemaFromSampleData")]
        public async Task<IHttpActionResult> GenerateSchema([FromBody] JObject data)
        {
            JsonSchema schema = JsonSchema.FromSampleJson(JsonConvert.SerializeObject(data));
            JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths(schema);
            return Ok(schema);
        }

        /// <summary>
        /// Get the latest schema revision detail
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <response code="401">Unauthroized Access</response>
        /// <returns>Latest field metadata for schema.</returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/latest/fields")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaRevisionDetailModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetLatestSchemaRevisionDetail(int datasetId, int schemaId)
        {
            IHttpActionResult GetLatestSchemaRevisionDetailFunction()
            {
               
                //if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Where(w => !w.DeleteInd).Any(w => w.Schema.SchemaId == schemaId))
                if (!_dsContext.DatasetFileConfigs.Where(w => w.ParentDataset.DatasetId == datasetId && !w.DeleteInd).Select(s => s.Schema.SchemaId).Any(a => a == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                SchemaRevisionDto revisiondto = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);
                if (revisiondto == null)
                {
                    Logger.Warn($"metadataapi_getlatestschemarevisiondetail no revision metadata - datasetid:{datasetId} schemaid:{schemaId}");
                    return Ok(new SchemaRevisionDetailModel());
                }

                SchemaRevisionDetailModel revisionDetailModel = revisiondto.ToSchemaDetailModel();
                List<BaseFieldDto> fieldDtoList = _schemaService.GetBaseFieldDtoBySchemaRevision(revisiondto.RevisionId);
                revisionDetailModel.Fields = fieldDtoList.ToSchemaFieldModel();
                return Ok(revisionDetailModel);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetLatestSchemaRevisionDetail), $"datasetid:{datasetId} schemaId{schemaId}", GetLatestSchemaRevisionDetailFunction);

        }

        /// <summary>
        /// Get the latest schema revisions JSON structure in JSON schema format
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/latest/jsonschema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaRevisionJsonStructureModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden)]
        public async Task<IHttpActionResult> GetLatestSchemaRevisionJsonFormat(int datasetId, int schemaId)
        {
            return ApiTryCatch(nameof(MetadataController), nameof(GetLatestSchemaRevisionJsonFormat), $"datasetid:{datasetId} schemaId{schemaId}", () => Ok(_schemaService.GetLatestSchemaRevisionJsonStructureBySchemaId(datasetId, schemaId).ToModel()));
        }

        /// <summary>
        /// Syncs the hive consumption layer with latest schema metadata
        /// </summary>
        /// <param name="datasetId">Non-Zero value required</param>
        /// <param name="schemaId">0 value will refresh all schemas under dataset. Non-zero value will refresh specific schema under dataset.</param>
        /// <returns></returns>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/syncconsumptionlayer")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(string))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> SyncConsumptionLayer(int datasetId, int schemaId)
        {
            IHttpActionResult SyncConsumptionLayerFunction()
            {
                //If datasetId == 0, fail request
                if (datasetId == 0)
                {
                    return BadRequest("datasetId is required");
                }

                bool isSuccessful = _configService.SyncConsumptionLayer(datasetId, schemaId);

                if (isSuccessful)
                {
                    return Ok("Sync request successfully submitted");
                }
                else
                {
                    return BadRequest("Something went wrong, sync request was unsuccessful.");
                }
            }

            return ApiTryCatch(nameof(MetadataController), nameof(SyncConsumptionLayer), $"datasetid:{datasetId} schemaId{schemaId}", SyncConsumptionLayerFunction);
        }


        /// <summary>
        /// Creates consumption layers for schema Id(s) provided.
        /// </summary>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("CreateConsumptionLayers")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public IHttpActionResult CreateConsumptionLayers(int[] schemaIdList)
        {
            Sentry.Common.Logging.Logger.Info($"{_userService.GetCurrentRealUser().AssociateId} called CreateConsumptionLayers on Dataset Id(s) {schemaIdList}");
            //validate
            foreach (int schemaId in schemaIdList)
            {
                var schema = _dsContext.GetById<Schema>(schemaId);
                if (schema == null)
                {
                    return BadRequest($"Schema with ID \"{schemaId}\" could not be found.");
                }
            }
            //run jobs
            _schemaService.CreateConsumptionLayersForSchemaList(schemaIdList);
            return Ok();
        }

        /// <summary>
        /// gets dataset metadata
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{DatasetConfigID}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(Metadata))]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetBasicMetadataInformationFor(int DatasetConfigID)
        {
            IHttpActionResult GetBasicMetadataInformationForFunction()
            {
                DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

                //Does user have permissions to dataset
                ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

                //return GetMetadataTask(config).Result;                
                return GetMetadataTask(config).GetAwaiter().GetResult();
            }

            async Task<IHttpActionResult> GetMetadataTask(DatasetFileConfig config)
            {
                return await GetMetadata(config);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetBasicMetadataInformationFor), $"datasetFileConfigId:{DatasetConfigID}", GetBasicMetadataInformationForFunction);
        }


        /// <summary>
        /// gets primary hive table
        /// </summary>
        /// <param name="DatasetConfigID"></param>
        /// <param name="SchemaID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{DatasetConfigID}/schemas/{SchemaID}/hive")]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetPrimaryHiveTableFor(int DatasetConfigID, int SchemaID = 0)
        {
            IHttpActionResult GetPrimaryHiveTableForFunction()
            {
                try
                { 
                    DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

                    ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

                    return Ok(new { HiveDatabaseName = config.Schema.HiveDatabase, HiveTableName = config.Schema.HiveTable });
                }
                catch (Exception ex)
                {
                    Logger.Error("metadatacontroller-datasets_schemas_hive failed", ex);
                    return NotFound();
                }
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetPrimaryHiveTableFor), $"datasetFileConfigId:{DatasetConfigID}", GetPrimaryHiveTableForFunction);

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
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetColumnSchemaInformationFor(int DatasetConfigID, int SchemaID = 0)
        {
            IHttpActionResult GetColumnSchemaInformationForFunction()
            {
                DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

                ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

                return GetColumnSchemaTask(config, SchemaID).Result;
            }

            async Task<IHttpActionResult> GetColumnSchemaTask(DatasetFileConfig config, int SchemaId)
            {
                return await GetColumnSchema(config, SchemaID);
            }

            return ApiTryCatch(nameof(MetadataController), nameof(GetColumnSchemaInformationFor), $"datasetFileConfigId:{DatasetConfigID}", GetColumnSchemaInformationForFunction);

        }

        /// <summary>
        /// Update schema metadata
        /// </summary>
        [HttpPut]
        [ApiVersionBegin(WebAPI.Version.v2)]
        [ApiVersionEnd(WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(bool))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public IHttpActionResult UpdateSchema(int datasetId, int schemaId, SchemaInfoModel schemaModel)
        {
            return UpdateSchema_Internal(datasetId, schemaId, schemaModel, s => s.ToDto(datasetId, (x) => _schemaService.GetFileExtensionIdByName(x)));
        }

        /// <summary>
        /// Update schema metadata
        /// </summary>
        [HttpPut]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(bool))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public IHttpActionResult UpdateSchema(int datasetId, int schemaId, Models.ApiModels.Schema20220609.SchemaInfoModel schemaModel)
        {
            return UpdateSchema_Internal(datasetId, schemaId, schemaModel, s => s.ToDto(datasetId, (x) => _schemaService.GetFileExtensionIdByName(x)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "False Positive")]
        private IHttpActionResult UpdateSchema_Internal<T>(int datasetId, int schemaId, T schemaModel, Func<T, FileSchemaDto> func) where T : SchemaInfoModelBase
        {
            IHttpActionResult Updater()
            {
                List<string> validationResults = schemaModel.Validate();
                if (schemaModel.SchemaId != schemaId)
                {
                    validationResults.Add($"The route schemaId {schemaId} does not match the schemaModel.SchemaId {schemaModel.SchemaId}");
                }

                if (validationResults.Any())
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, $"Invalid schema request: {string.Join(" | ", validationResults)}");
                }

                return Ok(_schemaService.UpdateAndSaveSchema(func.Invoke(schemaModel)));
            }

            return ApiTryCatch(nameof(MetadataController), nameof(UpdateSchema), $"datasetid:{datasetId} schemaId{schemaId}", Updater);
        }

        #endregion

        #region Schema_Endpoints




        #endregion

        #region Messaging Endpoints

        /// <summary>
        /// Publish message to messaging queue
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadGateway, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        //[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("PublishMessage")]
        public IHttpActionResult PublishMessage([FromBody] KafkaMessage message)
        {
            string methodName = $"{nameof(MetadataController).ToLower()}_{nameof(PublishMessage).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            Logger.AddContextVariable(new TextVariable("requestcontextguid", DateTime.UtcNow.ToString(GlobalConstants.System.REQUEST_CONTEXT_GUID_FORMAT)));
            Logger.AddContextVariable(new TextVariable("requestcontextmethod", methodName));

            try
            {
                if (message == null)
                {
                    Logger.Error($"{methodName} null message");
                    throw new ArgumentException("message parameter is null");
                }
                else
                {
                    Logger.Info($"{methodName} message:{ JsonConvert.SerializeObject(message) }");
                }

                _messagePublisher.PublishDSCEvent(message.Key, message.Message, message.Topic);                
            }
            catch (KafkaProducerException ex)
            {
                Logger.Error($"{methodName} failure", ex);
                return Content(System.Net.HttpStatusCode.BadGateway, "Unable to produce messages to kafka");
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} failure", ex);
                return InternalServerError();
            }

            Logger.Info($"{methodName} Method End");
            return Ok();

        }
        
        /// <summary>
         /// Publish message to messaging queue
         /// </summary>
         /// <param name="message"></param>
         /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadGateway, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        //[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("PublishMessageAsString")]
        public IHttpActionResult PublishMessageAsString([FromBody] string message)
        {
            try
            {
                KafkaMessage kMsg;

                if (message == null)
                {
                    Logger.Error($"jobcontroller-publishmessage null message");
                    throw new ArgumentException("message parameter is null");
                }
                else
                {
                    Logger.Debug($"jobcontroller-publishmessage message:{message}");

                    kMsg = JsonConvert.DeserializeObject<KafkaMessage>(message);
                }

                _messagePublisher.PublishDSCEvent(kMsg.Key, kMsg.Message, kMsg.Topic);
                return Ok();
            }
            catch (KafkaProducerException ex)
            {
                Logger.Error($"jobcontroller-publishmessageasstring failure", ex);
                return Content(System.Net.HttpStatusCode.BadGateway, "Unable to produce messages to kafka");
            }
            catch (Exception ex)
            {
                Logger.Error($"jobcontroller-publishmessageasstring failure", ex);
                return InternalServerError();
            }
        }
        #endregion

        #region Private Methods
        private SchemaMigrationResponseModel ToSchemaMigrationRequestModel(SchemaMigrationRequestResponse response)
        {
            return new SchemaMigrationResponseModel()
            {
                IsSchemaMigrated = response.MigratedSchema,
                SchemaId = response.TargetSchemaId,
                SchemaMigrationMessage = response.SchemaMigrationReason,
                IsSchemaRevisionMigrated = response.MigratedSchemaRevision,
                SchemaRevisionId = response.TargetSchemaRevisionId,
                SchemaRevisionMigrationMessage = response.SchemaRevisionMigrationReason,
                IsDataFlowMigrated = response.MigratedDataFlow,
                DataFlowId = response.TargetDataFlowId,
                DataFlowMigrationMessage = response.DataFlowMigrationReason
            };
        }

        internal DatasetMigrationResponseModel ToDatasetMigrationResponseModel(DatasetMigrationRequestResponse response)
        {
            DatasetMigrationResponseModel model = new DatasetMigrationResponseModel()
            {
                IsDatasetMigrated = response.IsDatasetMigrated,
                DatasetMigrationReason = response.DatasetMigrationReason,
                DatasetId = response.DatasetId,
                SchemaMigrationResponse = new List<SchemaMigrationResponseModel>()
            };

            foreach (SchemaMigrationRequestResponse schemaResponse in response.SchemaMigrationResponses)
            {
                model.SchemaMigrationResponse.Add(ToSchemaMigrationRequestModel(schemaResponse));
            }

            return model;
        }


        private SchemaMigrationRequest ToDto(SchemaMigrationRequestModel model)
        {
            return new SchemaMigrationRequest()
            {
                SourceSchemaId = model.SourceSchemaId,
                SourceSchemaHasDataFlow = _dsContext.DataFlow.Any(w => w.SchemaId == model.SourceSchemaId && (w.ObjectStatus == ObjectStatusEnum.Active || w.ObjectStatus == ObjectStatusEnum.Disabled)),
                TargetDataFlowNamedEnvironment = model.TargetDataFlowNamedEnviornment,
                TargetDatasetId = model.TargetDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };
        }

        internal DatasetMigrationRequest ToDto(Sentry.data.Web.Models.ApiModels.Migration.DatasetMigrationRequestModel model)
        {
            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = model.SourceDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment,
                TargetDatasetId = model.TargetDatasetId,
                SchemaMigrationRequests = new List<SchemaMigrationRequest>()
            };

            foreach (SchemaMigrationRequestModel schemaMigrationRequestModel in model.SchemaMigrationRequests)
            {
                request.SchemaMigrationRequests.Add(ToDto(schemaMigrationRequestModel));
            }

            return request;
        }

        private async Task<IHttpActionResult> GetColumnSchema(DatasetFileConfig config, int SchemaID)
        {
            if (config.Schema != null)
            {
                int scmId = (SchemaID == 0) ? config.Schema.SchemaId : SchemaID; 
                try
                {
                    //Get Schema for schema level info
                    OutputSchema outSchema = new OutputSchema();
                    FileSchemaDto fileSchemaDto = _schemaService.GetFileSchemaDto(scmId);

                    if(fileSchemaDto == null)
                    {
                        Logger.Info($"metadatacontroller-getcolumnschema - notfound_fileschemadto");
                        return NotFound();
                    }

                    outSchema.FileExtension = fileSchemaDto.FileExtensionId;

                    if (!config.Schema.Revisions.Any())
                    {
                        outSchema.rows = new List<SchemaRow>();
                        outSchema.FileExtension = config.FileExtension.Id;
                        return Ok(outSchema);
                    }

                    //Get SchemaRevision
                    SchemaRevisionDto revisionDto = _schemaService.GetLatestSchemaRevisionDtoBySchema(scmId);

                    if (revisionDto == null)
                    {
                        Logger.Info($"metadatacontroller-getcolumnschema - notfound_schemarevisiondto");
                        return NotFound();
                    }

                    //Get revision fields
                    List<BaseFieldDto> fieldDtoList = _schemaService.GetBaseFieldDtoBySchemaRevision(revisionDto.RevisionId);

                    List<SchemaRow> rows = new List<SchemaRow>();
                    foreach (BaseFieldDto dto in fieldDtoList)
                    {
                        rows.Add(dto.ToModel());
                    }
                    outSchema.rows = rows;

                    return Ok(outSchema);
                }
                catch (Exception ex)
                {
                    Logger.Error($"metadatacontroller-getcolumnschema", ex);
                    return InternalServerError();
                }
            }
            else
            {
                return NotFound();
            }
        }

        private async Task<IHttpActionResult> GetMetadata(DatasetFileConfig config)
        {
            try
            {
                Event e = new Event
                {
                    EventType = _dsContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault(),
                    Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                    UserWhoStartedEvent = RequestContext.Principal.Identity.Name,
                    DataConfig = config.ConfigId,
                    Reason = "Viewed Schema for Dataset"
                };
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                //SET Metadata to pass back to KO
                Metadata m = new Metadata
                {
                    //grab DatasetId and  SchemaId to be used to fill delroy Fields grid
                    DatasetId = config.ParentDataset.DatasetId,
                    SchemaId = config.Schema.SchemaId,
                    Description = config.Description,
                    ControlMTriggerName = config.Schema.ControlMTriggerName
                };

                //m.DFSDropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<DfsBasic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();

                //m.Views = _dsContext.Events.Where(x => x.Reason == "Viewed Schema for Dataset" && x.DataConfig == DatasetConfigID).Count();
                //m.Downloads = _dsContext.Events.Where(x => x.EventType.Description == "Downloaded Data File" && x.DataConfig == DatasetConfigID).Count();

                if (config.DatasetFiles.Any())
                {
                    m.DataLastUpdated = config.DatasetFiles.Max(x => x.ModifiedDTM).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                }

                if (config.RetrieverJobs.Any(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()))
                {
                    m.OtherJobs = config.RetrieverJobs.Where(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()).OrderBy(x => x.Id)
                        .Select(x => new DropLocation() { Location = x.IsEnabled ? x.Schedule : "Disabled", Name = x.DataSource.SourceType, JobId = x.Id, IsEnabled = x.IsEnabled }).ToList();
                }

                foreach (var item in _configService.GetExternalDataFlowsBySchema(config).ToList())
                {
                    DataFlow df = new DataFlow()
                    {
                        Name = item.Item1.Name,
                        Id = item.Item1.Id,
                        DetailUrl = $"{Sentry.Configuration.Config.GetHostSetting("SentryDataBaseUrl")}/DataFlow/{item.Item1.Id}/Detail",
                        PopulatesMultipleSchema = (item.Item1.MappedSchema.Count > 1),
                        ObjectStatus = item.Item1.ObjectStatus,
                        DeleteIssuer = item.Item1.DeleteIssuer,
                        DeleteIssueDTM = item.Item1.DeleteIssueDTM,
                        TopicName = item.Item1.TopicName,
                        IngestionType = item.Item1.IngestionType,
                        S3ConnectorName = item.Item1.S3ConnectorName
                    };
                    List<DropLocation> rjList = new List<DropLocation>();
                    foreach (var job in item.Item2)
                    {
                        rjList.Add(new DropLocation()
                        {
                            Name = (job.IsGeneric) ? job.DataSource.Name : $"{job.DataSource.SourceType} - {job.DataSource.Name}",
                            JobId = job.Id,
                            IsEnabled = job.IsEnabled,
                            Location = job.GetUri().AbsolutePath
                        });                        
                    }

                    rjList.Add(item.Item1.steps.Where(w => w.DataActionType == Core.Entities.DataProcessing.DataActionType.S3Drop || w.DataActionType == Core.Entities.DataProcessing.DataActionType.ProducerS3Drop).Select(s => new DropLocation() { Name = s.ActionName, JobId = s.Id, IsEnabled = true, Location = $"{s.TriggerBucket}/{s.TriggerKey}" }).FirstOrDefault());

                    df.RetrieverJobs = rjList;
                    m.DataFlows.Add(df);
                }

                m.HiveDatabase = config.Schema.HiveDatabase;
                m.HiveViews.Add($"vw_{config.Schema.HiveTable}");
                if (config.Schema.CreateCurrentView)
                {
                    m.HiveViews.Add($"vw_{config.Schema.HiveTable}_cur");
                }

                m.SnowflakeViews.AddRange(
                    config.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().Select(
                        s => $"{s.SnowflakeDatabase}.{s.SnowflakeSchema}.vw_{s.SnowflakeTable}"));
                if (config.Schema.CreateCurrentView)
                {
                    m.SnowflakeViews.AddRange(
                        config.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().Select(
                            s => $"{s.SnowflakeDatabase}.{s.SnowflakeSchema}.vw_{s.SnowflakeTable}_cur"));
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <exception cref="UnauthorizedAccessException">Thrown when user does not have access to dataset</exception>
        /// <exception cref="InternalServerErrorResult">Thrown for unhandled exceptions</exception>
        private void ValidateViewPermissionsForDataset(int datasetId)
        {
            UserSecurity us;
            try
            {
                Dataset ds = _dsContext.GetById<Dataset>(datasetId);
                IApplicationUser user = _userService.GetCurrentUser();
                us = _securityService.GetUserSecurity(ds, user);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadatacontroller-validateviewpermissionsfordataset failed to retrieve UserSecurity object", ex);
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
            }

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset || us.CanManageSchema))
            {
                try
                {
                    IApplicationUser user = _userService.GetCurrentUser();
                    Logger.Info($"metadatacontroller-validateviewpermissionsfordataset unauthorized_access: Id:{user.AssociateId}");
                }
                catch (Exception ex)
                {
                    Logger.Error("metadatacontroller-validateviewpermissionsfordataset unauthorized_access", ex);
                }
                throw new UnauthorizedAccessException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <exception cref="DatasetUnauthorizedAccessException">Thrown when user does not have edit permissions to dataset</exception>
        /// <exception cref="InternalServerErrorResult">Thrown when unhandled exception occurs</exception>
        private void ValidateModifyPermissionsForDataset(int datasetId)
        {
            //Does user have permissions to dataset
            UserSecurity us;
            try
            {
                us = _securityService.GetUserSecurity(_dsContext.GetById<Dataset>(datasetId), _userService.GetCurrentUser());
            }
            catch (Exception ex)
            {
                Logger.Error($"metadatacontroller-validateviewpermissionsfordataset failed to retrieve UserSecurity object", ex);
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
            }

            if (!us.CanEditDataset || !us.CanManageSchema)
            {
                throw new DatasetUnauthorizedAccessException();
            }
        }


        //private static void ToSchemaRows(JsonSchema schema, List<BaseFieldDto> schemaRowList, BaseFieldDto parentSchemaRow = null)
        //{
        //    try
        //    {
        //        switch (schema.Type)
        //        {
        //            case JsonObjectType.Object:
        //                foreach (KeyValuePair<string, JsonSchemaProperty> prop in schema.Properties.ToList())
        //                {
        //                    prop.ToDto(schemaRowList, parentSchemaRow);
        //                }
        //                break;
        //            case JsonObjectType.None:
        //                if (schema.HasReference)
        //                {
        //                    schema.Reference.ToDto(schemaRowList, parentSchemaRow);
        //                }
        //                else
        //                {
        //                    if (parentSchemaRow == null)
        //                    {
        //                        Logger.Warn("Unhandled Scenario");
        //                    }
        //                    else
        //                    {
        //                        parentSchemaRow.Description = "MOCKED OUT";
        //                    }
        //                }
        //                break;
        //            default:
        //                Logger.Warn($"Unhandled Scenario for schema object type of {schema.Type}");
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("ToSchemaRows Error", ex);
        //        throw;
        //    }
        //}

        //private static void ToSchemaRow(KeyValuePair<string, JsonSchemaProperty> prop, List<BaseFieldDto> schemaRowList, BaseFieldDto parentRow = null)
        //{
        //    try
        //    {
        //        FieldDtoFactory fieldFactory = null;

        //        JsonSchemaProperty currentProperty = prop.Value;
        //        Logger.Debug($"Found property:{prop.Key}");
        //        switch (currentProperty.Type)
        //        {
        //            case JsonObjectType.None:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");
        //                if (currentProperty.HasReference)
        //                {
        //                    Logger.Debug($"Detected ref object: property will be defined as STRUCT");
        //                    fieldFactory = new StructFieldDtoFactory(prop, false);
        //                    BaseFieldDto noneStructField = fieldFactory.GetField();

        //                    if (parentRow == null)
        //                    {
        //                        schemaRowList.Add(noneStructField);
        //                    }
        //                    else
        //                    {
        //                        parentRow.ChildFields.Add(noneStructField);
        //                    }

        //                    ToSchemaRows(currentProperty.Reference, schemaRowList, noneStructField);
        //                }
        //                else
        //                {
        //                    Logger.Warn($"No ref object detected");
        //                    Logger.Warn($"{prop.Key} will be defined as STRUCT");
        //                    fieldFactory = new VarcharFieldDtoFactory(prop, false);

        //                    if (parentRow == null)
        //                    {
        //                        schemaRowList.Add(fieldFactory.GetField());
        //                    }
        //                    else
        //                    {
        //                        parentRow.ChildFields.Add(fieldFactory.GetField());
        //                    }
        //                }
        //                break;
        //            case JsonObjectType.Object:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");
        //                Logger.Debug($"Detected ref object: property will be defined as STRUCT");
        //                fieldFactory = new StructFieldDtoFactory(prop, false);
        //                BaseFieldDto objectStructfield = fieldFactory.GetField();

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(objectStructfield);
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(objectStructfield);
        //                }

        //                foreach (KeyValuePair<string, JsonSchemaProperty> nestedProp in currentProperty.Properties)
        //                {
        //                    ToSchemaRow(nestedProp, schemaRowList, objectStructfield);
        //                }

        //                break;
        //            case JsonObjectType.Array:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");

        //                JsonSchema nestedSchema = null;
        //                //While JSON Schema alows an arrays of multiple types, DSC only allows single type.

        //                nestedSchema = prop.FindArraySchema();

        //                //Determine what this is an array of
        //                if (nestedSchema.IsObject)
        //                {
        //                    Logger.Debug($"Detected nested schema as Object");
        //                    Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
        //                    fieldFactory = new StructFieldDtoFactory(prop, true);
        //                }
        //                else
        //                {
        //                    switch (nestedSchema.Type)
        //                    {
        //                        case JsonObjectType.Object:
        //                            Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
        //                            Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
        //                            fieldFactory = new StructFieldDtoFactory(prop, true);
        //                            break;
        //                        case JsonObjectType.Integer:
        //                            Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
        //                            Logger.Debug($"{prop.Key} will be defined as array of INTEGER");
        //                            fieldFactory = new IntegerFieldDtoFactory(prop, true);
        //                            break;
        //                        case JsonObjectType.String:
        //                            Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
        //                            switch (nestedSchema.Format)
        //                            {
        //                                case "date-time":
        //                                    Logger.Debug($"Detected string format of {nestedSchema.Format}");
        //                                    Logger.Debug($"{prop.Key} will be defined as array of TIMESTAMP");
        //                                    fieldFactory = new TimestampFieldDtoFactory(prop, true);
        //                                    break;
        //                                case "date":
        //                                    Logger.Debug($"Detected string format of {nestedSchema.Format}");
        //                                    Logger.Debug($"{prop.Key} will be defined as array of DATE");
        //                                     fieldFactory = new DateFieldDtoFactory(prop, true);
        //                                    break;
        //                                default:
        //                                    Logger.Debug($"No string format detected");
        //                                    Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
        //                                    fieldFactory = new VarcharFieldDtoFactory(prop, true);
        //                                    break;
        //                            }
        //                            break;
        //                        case JsonObjectType.Number:
        //                            Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
        //                            Logger.Debug($"{prop.Key} will be defined as array of DECIMAL");
        //                            fieldFactory = new DecimalFieldDtoFactory(prop, true);
        //                            break;
        //                        case JsonObjectType.None:
        //                            if (nestedSchema.IsAnyType)
        //                            {
        //                                Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} and marked as IsAnyType");
        //                                Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
        //                                fieldFactory = new VarcharFieldDtoFactory(prop, true);
        //                            }
        //                            else
        //                            {
        //                                Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()}");
        //                                Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
        //                                fieldFactory = new VarcharFieldDtoFactory(prop, true);
        //                            }
        //                            break;
        //                        default:
        //                            Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
        //                            Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
        //                            fieldFactory = new VarcharFieldDtoFactory(prop, true);
        //                            break;
        //                    }
        //                }

        //                BaseFieldDto field = fieldFactory.GetField();

        //                ToSchemaRows(nestedSchema, schemaRowList, field);

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(field);
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(field);
        //                }
        //                break;
        //            case JsonObjectType.String:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");

        //                if (!String.IsNullOrWhiteSpace(currentProperty.Format))
        //                {
        //                    switch (currentProperty.Format)
        //                    {
        //                        case "date-time":
        //                            Logger.Debug($"Detected string format of {currentProperty.Format}");
        //                            Logger.Debug($"{prop.Key} will be defined as TIMESTAMP");
        //                            fieldFactory = new TimestampFieldDtoFactory(prop, false);
        //                            break;
        //                        case "date":
        //                            Logger.Debug($"Detected string format of {currentProperty.Format}");
        //                            Logger.Debug($"{prop.Key} will be defined as DATE");
        //                            fieldFactory = new DateFieldDtoFactory(prop, false);
        //                            break;
        //                        default:
        //                            Logger.Warn($"Detected string format of {currentProperty.Format} which is not handled by DSC");
        //                            Logger.Warn($"{prop.Key} will be defined as DATE");
        //                            fieldFactory = new VarcharFieldDtoFactory(prop, false);
        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    Logger.Debug($"No string format detected");
        //                    Logger.Debug($"{prop.Key} will be defined as VARCHAR");
        //                    fieldFactory = new VarcharFieldDtoFactory(prop, false);
        //                }

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(fieldFactory.GetField());
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(fieldFactory.GetField());
        //                }
        //                break;
        //            case JsonObjectType.Integer:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");
        //                Logger.Debug($"{prop.Key} will be defined as INTEGER");

        //                fieldFactory = new IntegerFieldDtoFactory(prop, false);

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(fieldFactory.GetField());
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(fieldFactory.GetField());
        //                }
        //                break;
        //            case JsonObjectType.Number:
        //                Logger.Debug($"Detected type of {currentProperty.Type}");
        //                Logger.Debug($"{prop.Key} will be defined as DECIMAL");
        //                fieldFactory = new DecimalFieldDtoFactory(prop, false);

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(fieldFactory.GetField());
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(fieldFactory.GetField());
        //                }
        //                break;
        //            default:
        //                Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
        //                Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
        //                fieldFactory = new VarcharFieldDtoFactory(prop, true);

        //                if (parentRow == null)
        //                {
        //                    schemaRowList.Add(fieldFactory.GetField());
        //                }
        //                else
        //                {
        //                    parentRow.ChildFields.Add(fieldFactory.GetField());
        //                }
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("ToSchemaRow Error", ex);
        //        throw;
        //    }

        //}

        #endregion


    }
    //public class Try<TResult>
    //{
    //    Func<TResult> action;
    //    List<Func<Exception, TResult>> catchActions = new List<Func<Exception, TResult>>();
    //    public Try(Func<TResult> action)
    //    {
    //        this.action = action;
    //    }
    //    public static Try<TResult> Action<TResult>(Func<TResult> act)
    //    {
    //        return new Try<TResult>(act);
    //    }

    //    public Try<TResult> WithCatch<TException>(Func<TException, TResult> act) where TException : Exception
    //    {
    //        catchActions.Add((Func<Exception, TResult>)act);
    //        return this;
    //    }

    //    public TResult Finally(Action act)
    //    {
    //        try
    //        {
    //            return action();
    //        }
    //        catch (Exception ex)
    //        {
    //            foreach (var catchAction in catchActions)
    //            {
    //                return catchAction(ex);
    //            }
    //            throw ex;
    //        }
    //        finally
    //        {
    //            act();
    //        }
    //    }
    //}
}
