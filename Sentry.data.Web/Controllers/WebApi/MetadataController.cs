using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Sentry.Common.Logging;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Factories.Fields;
using Sentry.data.Web.Models.ApiModels.Dataset;
using Sentry.data.Web.Models.ApiModels.Schema;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;
        private IConfigService _configService;
        private IDatasetService _datasetService;
        private ISchemaService _schemaService;
        private ISecurityService _securityService;
        private IDataFlowService _dataFlowService;

        public MetadataController(MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext,
                                IAssociateInfoProvider associateInfoService, UserService userService,
                                IConfigService configService, IDatasetService datasetService,
                                ISchemaService schemaService, ISecurityService securityService,
                                IDataFlowService dataFlowService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
            _configService = configService;
            _datasetService = datasetService;
            _schemaService = schemaService;
            _securityService = securityService;
            _dataFlowService = dataFlowService;
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

        public class KafkaMessage
        {
            public string Key { get; set; }
            public string Message { get; set; }
        }
        #endregion


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
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
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

        /// <summary>
        /// Get list of schema for dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaInfoModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchemaByDataset(int datasetId)
        {

            try
            {
                List<DatasetFileConfigDto> dtoList = _configService.GetDatasetFileConfigDtoByDataset(datasetId);
                List<SchemaInfoModel> modelList = dtoList.ToSchemaModel();
                return Ok(modelList);
            }
            catch (DatasetNotFoundException)
            {
                return NotFound();
            }
            catch (DatasetUnauthorizedAccessException duax)
            {
                throw new UnauthorizedAccessException("Unauthroized Access to Dataset", duax);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getschemabydataset_internalservererror - datasetid:{datasetId}", ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get schema metadata
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaInfoModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchema(int datasetId, int schemaId)
        {

            try
            {
                DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
                if (dto == null)
                {
                    return Content(System.Net.HttpStatusCode.NotFound, "Schema not found");
                }
                SchemaInfoModel model = dto.ToSchemaModel();
                return Ok(model);
            }
            catch (DatasetNotFoundException)
            {
                return Content(System.Net.HttpStatusCode.NotFound, "Dataset not found");
            }
            catch (DatasetUnauthorizedAccessException duax)
            {
                throw new UnauthorizedAccessException("Unauthroized Access to Dataset", duax);
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
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetSchemaRevisionBySchema(int datasetId, int schemaId)
        {
            ValidateViewPermissionsForDataset(datasetId);

            try
            {
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Where(w => !w.DeleteInd).Any(w => w.Schema.SchemaId == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                List<SchemaRevisionDto> revisionDto = _schemaService.GetSchemaRevisionDtoBySchema(schemaId);

                List<SchemaRevisionModel> modelList = revisionDto.ToModel();
                return Ok(modelList);
            }
            catch (DatasetNotFoundException)
            {
                Logger.Info($"metadataapi_getschemarevisionbyschema_notfound schema - datasetId:{datasetId} schemaId:{schemaId}");
                return Content(System.Net.HttpStatusCode.NotFound, "Dataset not found");
            }
            catch (SchemaNotFoundException)
            {
                Logger.Info($"metadataapi_getschemarevisionbyschema_notfound revision - datasetId:{datasetId} schemaId:{schemaId}");
                return Content(System.Net.HttpStatusCode.NotFound, "Schema not found");
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getschemarevisionbyschema_internalservererror - datasetId:{datasetId} schemaId{schemaId}", ex);
                return InternalServerError();
            }
        }

        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(int))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, null)]
        public async Task<IHttpActionResult> AddSchemaRevision(int datasetId, int schemaId, string revisionName, [FromBody] JObject schemaStructure)
        {
            try
            {
                ValidateModifyPermissionsForDataset(datasetId);

                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Any(w => w.Schema.SchemaId == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                int configId = _configService.GetDatasetFileConfigDtoByDataset(datasetId).Where(w => w.Schema.SchemaId == schemaId).First().ConfigId;


                //string jobj = schemaStructure.ToString();

                //JSchema schema_v1 = JSchema.Parse(schemaStructure.ToString());
                //JsonSchemaDriller(schema_v1);

                //JsonSchemaDriller_v2(schema);
                //bool valid = schemaStructure.IsValid(schema);

                JsonSchema schema_v3 = await JsonSchema.FromJsonAsync(schemaStructure.ToString());
                //string schema = JsonSchemaDriller_v3(schema_v3);

                List<BaseFieldDto> schemarows_v2 = new List<BaseFieldDto>();
                ToSchemaRows(schema_v3, schemarows_v2);

                //_configService.UpdateFields(configId, schemaId, schemarows_v2, schema_v3.ToJson());
                int savedRevisionId = _schemaService.CreateAndSaveSchemaRevision(schemaId, schemarows_v2, revisionName, schema_v3.ToJson());

                //SchemaRevisionDto revisiondto = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);
                //SchemaRevisionDetailModel revisionDetailModel = revisiondto.ToSchemaDetailModel();
                //List<BaseFieldDto> fieldDtoList = _schemaService.GetBaseFieldDtoBySchemaRevision(revisiondto.RevisionId);
                //revisionDetailModel.Fields = fieldDtoList.ToSchemaFieldModel();
                if (savedRevisionId == 0)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, "Unable to Save Revision");
                }
                return Ok(savedRevisionId);
            }
            catch (DatasetUnauthorizedAccessException)
            {
                Logger.Debug($"{nameof(SchemaService).ToLower()}_{nameof(AddSchemaRevision).ToLower()}_unauthorizedexception dataset - datasetId:{datasetId} schemaId:{schemaId}");
                return Content(System.Net.HttpStatusCode.Unauthorized, "Unauthroized Access to Dataset");
            }
            catch (SchemaUnauthorizedAccessException)
            {
                Logger.Debug($"{nameof(SchemaService).ToLower()}_{nameof(AddSchemaRevision).ToLower()}_unauthorizedexception schema - datasetId:{datasetId} schemaId:{schemaId}");
                return Content(System.Net.HttpStatusCode.Unauthorized, "Unauthroized Access to Schema");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/GenerateSchemaFromSampleData")]
        public async Task<IHttpActionResult> GenerateSchema(int datasetId, int schemaId, [FromBody] JObject data)
        {
            var schema = JsonSchema.FromSampleJson(JsonConvert.SerializeObject(data));
            string schema2 = JsonSchemaReferenceUtilities.ConvertPropertyReferences(schema.ToJson());
            return Ok(JsonConvert.DeserializeObject<JsonSchema>(schema2));
        }

        public static void ToSchemaRows(JsonSchema schema, List<BaseFieldDto> schemaRowList, BaseFieldDto parentSchemaRow = null)
        {
            try
            {
                switch (schema.Type)
                {
                    case JsonObjectType.Object:
                        foreach (KeyValuePair<string, JsonSchemaProperty> prop in schema.Properties.ToList())
                        {
                            ToSchemaRow(prop, schemaRowList, parentSchemaRow);
                        }
                        break;
                    case JsonObjectType.None:
                        if (schema.HasReference)
                        {
                            ToSchemaRows(schema.Reference, schemaRowList, parentSchemaRow);
                        }
                        else
                        {
                            if (parentSchemaRow == null)
                            {
                                Logger.Warn("Unhandled Scenario");
                            }
                            else
                            {
                                parentSchemaRow.Description = "MOCKED OUT";
                            }
                        }
                        break;
                    default:
                        Logger.Warn($"Unhandled Scenario for schema object type of {schema.Type}");
                        break;
                }
            }
            catch(Exception ex)
            {
                Logger.Error("ToSchemaRows Error", ex);
                throw;
            }            
        }

        public static void ToSchemaRow(KeyValuePair<string, JsonSchemaProperty> prop, List<BaseFieldDto> schemaRowList, BaseFieldDto parentRow = null)
        {
            try
            {
                FieldDtoFactory fieldFactory = null;

                JsonSchemaProperty currentProperty = prop.Value;
                Logger.Debug($"Found property:{prop.Key}");
                switch (currentProperty.Type)
                {
                    case JsonObjectType.None:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        if (currentProperty.HasReference)
                        {
                            Logger.Debug($"Detected ref object: property will be defined as STRUCT");
                            fieldFactory = new StructFieldDtoFactory(prop, false);
                            BaseFieldDto noneStructField = fieldFactory.GetField();

                            if (parentRow == null)
                            {
                                schemaRowList.Add(noneStructField);
                            }
                            else
                            {
                                parentRow.ChildFields.Add(noneStructField);
                            }

                            ToSchemaRows(currentProperty.Reference, schemaRowList, noneStructField);
                        }
                        else
                        {
                            Logger.Warn($"No ref object detected");
                            Logger.Warn($"{prop.Key} will be defined as STRUCT");
                            fieldFactory = new VarcharFieldDtoFactory(prop, false);

                            if (parentRow == null)
                            {
                                schemaRowList.Add(fieldFactory.GetField());
                            }
                            else
                            {
                                parentRow.ChildFields.Add(fieldFactory.GetField());
                            }
                        }
                        break;
                    case JsonObjectType.Object:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"Detected ref object: property will be defined as STRUCT");
                        fieldFactory = new StructFieldDtoFactory(prop, false);
                        BaseFieldDto objectStructfield = fieldFactory.GetField();

                        if (parentRow == null)
                        {                            
                            schemaRowList.Add(objectStructfield);                           
                        }
                        else
                        {
                            parentRow.ChildFields.Add(objectStructfield);
                        }

                        foreach (KeyValuePair<string, JsonSchemaProperty> nestedProp in currentProperty.Properties)
                        {
                            ToSchemaRow(nestedProp, schemaRowList, objectStructfield);
                        }

                        break;
                    case JsonObjectType.Array:
                        Logger.Debug($"Detected type of {currentProperty.Type}");

                        JsonSchema nestedSchema = null;
                        //While JSON Schema alows an arrays of multiple types, DSC only allows single type.

                        if (currentProperty.Items.Count == 0 && currentProperty.Item == null)
                        {
                            JsonSchema refSchema = currentProperty.ParentSchema.Definitions.Where(w => w.Key.ToUpper() == prop.Key.ToUpper()).FirstOrDefault().Value;
                            if (refSchema == null)
                            {
                                throw new Exception("Not valid schema: Array does not contain items");
                            }
                            else
                            {
                                nestedSchema = refSchema;
                            }
                        }
                        else if (currentProperty.Items.Count == 0 && currentProperty.Item != null)
                        {
                            nestedSchema = currentProperty.Item;
                        }
                        else
                        {
                            if (currentProperty.Items.Count > 1)
                            {
                                Logger.Warn($"Schema contains multiple items within array ({prop.Key}) - taking first Item");
                            }
                            nestedSchema = currentProperty.Items.First();
                        }

                        //Determine what this is an array of
                        if (nestedSchema.IsObject)
                        {
                            Logger.Debug($"Detected nested schema as Object");
                            Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
                            fieldFactory = new StructFieldDtoFactory(prop, true);
                        }
                        else
                        {
                            switch (nestedSchema.Type)
                            {
                                case JsonObjectType.Object:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
                                    fieldFactory = new StructFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.Integer:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of INTEGER");
                                    fieldFactory = new IntegerFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.String:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    switch (currentProperty.Format)
                                    {
                                        case "date-time":
                                            Logger.Debug($"Detected string format of {currentProperty.Format}");
                                            Logger.Debug($"{prop.Key} will be defined as array of TIMESTAMP");
                                            fieldFactory = new TimestampFieldDtoFactory(prop, true);
                                            break;
                                        case "date":
                                            Logger.Debug($"Detected string format of {currentProperty.Format}");
                                            Logger.Debug($"{prop.Key} will be defined as array of DATE");
                                            fieldFactory = new DateFieldDtoFactory(prop, true);
                                            break;
                                        default:
                                            Logger.Debug($"No string format detected");
                                            Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                            fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                            break;
                                    }
                                    break;
                                case JsonObjectType.Number:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of DECIMAL");
                                    fieldFactory = new DecimalFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.None:
                                    if (nestedSchema.IsAnyType)
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} and marked as IsAnyType");
                                        Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                        fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    }
                                    else
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()}");
                                        Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                        fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    }
                                    break;
                                default:
                                case JsonObjectType.File:
                                case JsonObjectType.Null:
                                case JsonObjectType.Boolean:
                                    Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
                                    Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
                                    fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    break;
                            }
                        }

                        BaseFieldDto field = fieldFactory.GetField();

                        ToSchemaRows(nestedSchema, schemaRowList, field);

                        if (parentRow == null)
                        {
                            schemaRowList.Add(field);
                        }
                        else
                        {
                            parentRow.ChildFields.Add(field);
                        }
                        break;
                    case JsonObjectType.String:
                        Logger.Debug($"Detected type of {currentProperty.Type}");

                        if (!String.IsNullOrWhiteSpace(currentProperty.Format))
                        {
                            switch (currentProperty.Format)
                            {
                                case "date-time":
                                    Logger.Debug($"Detected string format of {currentProperty.Format}");
                                    Logger.Debug($"{prop.Key} will be defined as TIMESTAMP");
                                    fieldFactory = new TimestampFieldDtoFactory(prop, false);
                                    break;
                                case "date":
                                    Logger.Debug($"Detected string format of {currentProperty.Format}");
                                    Logger.Debug($"{prop.Key} will be defined as DATE");
                                    fieldFactory = new DateFieldDtoFactory(prop, false);
                                    break;
                                default:
                                    Logger.Warn($"Detected string format of {currentProperty.Format} which is not handled by DSC");
                                    Logger.Warn($"{prop.Key} will be defined as DATE");
                                    fieldFactory = new VarcharFieldDtoFactory(prop, false);
                                    break;
                            }
                        }
                        else
                        {
                            Logger.Debug($"No string format detected");
                            Logger.Debug($"{prop.Key} will be defined as VARCHAR");
                            fieldFactory = new VarcharFieldDtoFactory(prop, false);
                        }

                        if (parentRow == null)
                        {
                            schemaRowList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    case JsonObjectType.Integer:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"{prop.Key} will be defined as INTEGER");

                        fieldFactory = new IntegerFieldDtoFactory(prop, false);

                        if (parentRow == null)
                        {
                            schemaRowList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    case JsonObjectType.Number:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"{prop.Key} will be defined as DECIMAL");
                        fieldFactory = new DecimalFieldDtoFactory(prop, false);

                        if (parentRow == null)
                        {
                            schemaRowList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    default:
                    case JsonObjectType.File:
                    case JsonObjectType.Null:
                    case JsonObjectType.Boolean:
                        Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
                        Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
                        fieldFactory = new VarcharFieldDtoFactory(prop, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ToSchemaRow Error", ex);
                throw;
            }
            
        }

        //private static string JsonSchemaDriller_v3(JsonSchema schema, string parentBreadCrumb = null)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    switch (schema.Type)
        //    {
        //        case JsonObjectType.Object:

        //            sb.AppendLine(JsonSchema_PropertiesDriller_v3(schema.Properties.ToList(), parentBreadCrumb));
        //            break;
        //        case JsonObjectType.Array:

        //            foreach (JsonSchema prop in schema.Items)
        //            {
        //                sb.AppendLine(prop.ToString());
        //                JsonSchemaDriller_v3(prop);
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return sb.ToString();
        //}

        //private static string JsonSchema_PropertiesDriller_v3(List<KeyValuePair<string, JsonSchemaProperty>> propertyList, string parentBreadCrumb = null)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    string currentBreadCrumb = null;

        //    foreach (KeyValuePair<string, JsonSchemaProperty> prop in propertyList)
        //    {
        //        JsonSchemaProperty currentProperty = prop.Value;
        //        //if (currentProperty.Type == JsonObjectType.Object)
        //        //{
        //        //    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} ({prop.Value.Type.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";
        //        //    sb.AppendLine(currentBreadCrumb);
        //        //    Debug.WriteLine(currentBreadCrumb);
        //        //}
        //        //else if (currentProperty.Type == JsonObjectType.Array)
        //        //{
        //        //    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} ({prop.Value.Type.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";
        //        //    sb.AppendLine(currentBreadCrumb);
        //        //    Debug.WriteLine(currentBreadCrumb);
        //        //    foreach (JsonSchema nestedSchema in currentProperty.Items)
        //        //    {
        //        //        JsonSchemaDriller_v3(nestedSchema, currentBreadCrumb);
        //        //    }
        //        //}
        //        //else
        //        //{
        //            switch (currentProperty.Type)
        //            {
        //                case JsonObjectType.Object:
        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} ({prop.Value.Type.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";
        //                    //sb.AppendLine(currentBreadCrumb);
        //                    //Debug.WriteLine(currentBreadCrumb);
        //                    break;
        //                case JsonObjectType.Array:
        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} ({prop.Value.Type.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";
        //                    //sb.AppendLine(currentBreadCrumb);
        //                    //Debug.WriteLine(currentBreadCrumb);
        //                    foreach (JsonSchema nestedSchema in currentProperty.Items)
        //                    {
        //                        JsonSchemaDriller_v3(nestedSchema, currentBreadCrumb);
        //                    }
        //                    break;
        //                case JsonObjectType.String:
        //                    StringBuilder extraProperties = new StringBuilder();
        //                    if (!String.IsNullOrWhiteSpace(currentProperty.Format)){
        //                        switch (currentProperty.Format)
        //                        {
        //                            case "date-time":
        //                                extraProperties.Append($", DSC-DataType:TIMESTAMP");
        //                                break;
        //                            case "date":
        //                                extraProperties.Append($", DSC-DataType:DATE");
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        extraProperties.Append($", DSC-DataType:STRING");
        //                        if (currentProperty.MaxLength != null)
        //                        {
        //                            //default to 8000 if maxlenght is not specified
        //                            int stringLength = (currentProperty.MaxLength) ?? 8000;
        //                            extraProperties.Append($", Length:{stringLength.ToString()}");
        //                        }
        //                    }

        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} (json-type:{prop.Value.Type.ToString()}{extraProperties.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";


        //                    break;
        //                default:
        //                case JsonObjectType.Number:
        //                case JsonObjectType.File:
        //                case JsonObjectType.Null:
        //                case JsonObjectType.Integer:
        //                case JsonObjectType.Boolean:
        //                case JsonObjectType.None:
        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {prop.Key} ({prop.Value.Type.ToString()})" : $"{prop.Key} ({prop.Value.Type.ToString()})";
        //                    break;
        //            }
        //            sb.AppendLine(currentBreadCrumb);
        //            Debug.WriteLine(currentBreadCrumb);
        //        //}
        //    }

        //    return (sb.Length > 0)? sb.ToString() : String.Empty;
        //}

        //private static void JsonSchemaDriller_v2(JSchema schema, string parentBreadCrumb = null)
        //{
        //    string currentBreadCrumb = null;

        //    switch (schema.Type)
        //    {
        //        case JSchemaType.Object:
        //            currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {entry.Key} ({entry.Value.Type.ToString()})" : $"{entry.Key} ({entry.Value.Type.ToString()})";
        //            if (schema.Properties.Any())
        //            {
        //                JsonSchemaProperitiesDriller_v2(schema.Properties);
        //            }
        //            //else if (schema.ExtensionData.Any())
        //            //{
        //            //    Debug.WriteLine("Yay");
        //            //    JsonSchemaDriller_v2(JSchema.Parse(schema.ExtensionData.ToString()));
        //            //}

        //            break;
        //        case JSchemaType.Array:
        //            foreach (JSchema arraySchema in schema.Items)
        //            {
        //                JsonSchemaDriller_v2(arraySchema);
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private static void JsonSchemaProperitiesDriller_v2(IDictionary<string, JSchema> propertyList, string parentBreadCrumb = null)
        //{
        //    foreach (KeyValuePair<string, JSchema> prop in propertyList)
        //    {
        //        Debug.WriteLine($"{prop.Key} ({prop.Value.Type.ToString()})");
        //        if (prop.Value.Type == JSchemaType.Object)
        //        {                    
        //            JsonSchemaDriller_v2(prop.Value);
        //        }
        //        else if (prop.Value.Type == JSchemaType.Array)
        //        {
        //            foreach(JSchema arraySchema in prop.Value.Items)
        //            {
        //                JsonSchemaDriller_v2(arraySchema);
        //            }
        //        }
        //    }
        //}

        //private static string JsonSchemaDriller(JSchema schema, string parentBreadCrumb = null)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string currentBreadCrumb = null;

        //    switch (schema.Type)
        //    {
        //        case JSchemaType.Object:
        //            foreach (KeyValuePair<string, JSchema> entry in schema.Properties)
        //            {
        //                if (entry.Value.Type == JSchemaType.Object || entry.Value.Type == JSchemaType.Array)
        //                {
        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {entry.Key} ({entry.Value.Type.ToString()})" : $"{entry.Key} ({entry.Value.Type.ToString()})";
        //                    Debug.WriteLine(currentBreadCrumb);
        //                    sb.AppendLine(JsonSchemaDriller(entry.Value, currentBreadCrumb));
        //                }
        //                else
        //                {
        //                    currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {entry.Key} ({entry.Value.Type.ToString()})" : $"{entry.Key} ({entry.Value.Type.ToString()})";
        //                    Debug.WriteLine(currentBreadCrumb);
        //                    sb.AppendLine($"{currentBreadCrumb}");
        //                }
        //            }
        //            break;
        //        case JSchemaType.Array:
        //            currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb : $"Array ({JSchemaType.Array.ToString()})";
        //            Debug.WriteLine(currentBreadCrumb);
        //            foreach (JSchema item in schema.Items)
        //            {
        //                sb.AppendLine(JsonSchemaDriller(item, currentBreadCrumb));
        //            }                    
        //            break;
        //        default:
        //            break;
        //    }

        //    return sb.ToString();

        //    //foreach(KeyValuePair<string, JSchema> entry in schema.Properties)
        //    //{
        //    //    string currentBreadCrumb = null;

        //    //    if (entry.Value.Type == JSchemaType.Object)
        //    //    {
        //    //        currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {entry.Key} ({JSchemaType.Object.ToString()})" : $"{entry.Key} ({JSchemaType.Object.ToString()})";
        //    //        sb.AppendLine(JsonSchemaDriller(entry.Value, currentBreadCrumb));
        //    //    }
        //    //    else
        //    //    {
        //    //        currentBreadCrumb = (parentBreadCrumb != null) ? parentBreadCrumb + $" -> {entry.Key} ({entry.Value.Type.ToString()})" : $"{entry.Key} ({entry.Value.Type.ToString()})";
        //    //        sb.AppendLine($"{currentBreadCrumb}");
        //    //    }
        //    //}
        //}

        /// <summary>
        /// Get the latest schema revision detail
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="schemaId"></param>
        /// <response code="401">Unauthroized Access</response>
        /// <returns>Latest field metadata for schema.</returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/latest/fields")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaRevisionDetailModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetLatestSchemaRevisionDetail(int datasetId, int schemaId)
        {
            ValidateViewPermissionsForDataset(datasetId);

            try
            {
                if (!_configService.GetDatasetFileConfigDtoByDataset(datasetId).Where(w => !w.DeleteInd).Any(w => w.Schema.SchemaId == schemaId))
                {
                    throw new SchemaNotFoundException();
                }

                SchemaRevisionDto revisiondto = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);
                if (revisiondto == null)
                {
                    Logger.Info($"metadataapi_getlatestschemarevisiondetail_notfound revision - datasetid:{datasetId} schemaid:{schemaId}");
                    return Content(System.Net.HttpStatusCode.NotFound, "Schema revisions not found");
                }

                SchemaRevisionDetailModel revisionDetailModel = revisiondto.ToSchemaDetailModel();
                List<BaseFieldDto> fieldDtoList = _schemaService.GetBaseFieldDtoBySchemaRevision(revisiondto.RevisionId);
                revisionDetailModel.Fields = fieldDtoList.ToSchemaFieldModel();
                return Ok(revisionDetailModel);
            }
            catch (DatasetNotFoundException)
            {
                return Content(System.Net.HttpStatusCode.NotFound, "Dataset not found");
            }
            catch (SchemaNotFoundException)
            {
                return Content(System.Net.HttpStatusCode.NotFound, "Schema not found");
            }
            catch (SchemaUnauthorizedAccessException authex)
            {
                throw new UnauthorizedAccessException("Unauthroized Access to Schema", authex);
            }
            catch (Exception ex)
            {
                Logger.Error($"metadataapi_getlatestschemarevisiondetail_badrequest - datasetid:{datasetId} schemaid:{schemaId}", ex);
                return InternalServerError();
            }
        }

        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/revision/latest/jsonschema")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(JObject))]
        public async Task<IHttpActionResult> GetLatestSchemaRevisionJsonFormat(int datasetId, int schemaId)
        {
            SchemaRevisionDto revisiondto = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);

            JsonSchema schema = await JsonSchema.FromJsonAsync(revisiondto.JsonSchemaObject);

            return Ok(schema);
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
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> SyncConsumptionLayer(int datasetId, int schemaId)
        {
            try
            {
                //If datasetId == 0, fail request
                if (datasetId == 0)
                {
                    return BadRequest("datasetId is required");
                }

                ValidateModifyPermissionsForDataset(datasetId);

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
            catch (DatasetUnauthorizedAccessException duaEx)
            {
                throw new UnauthorizedAccessException("Unauthroized Access to Dataset", duaEx);
            }
            catch (Exception ex)
            {
                Logger.Error("configcontroller-syncconsumptionlayer failed", ex);
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
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(Metadata))]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetBasicMetadataInformationFor(int DatasetConfigID)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            //Does user have permissions to dataset
            ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

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
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetPrimaryHiveTableFor(int DatasetConfigID, int SchemaID = 0)
        {
            try
            {
                DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

                ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

                return Ok(new { HiveDatabaseName = config.Schema.HiveDatabase, HiveTableName = config.Schema.HiveTable });
            }
            catch(Exception ex)
            {
                Logger.Error("metadatacontroller-datasets_schemas_hive failed", ex);
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
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetColumnSchemaInformationFor(int DatasetConfigID, int SchemaID = 0)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            ValidateViewPermissionsForDataset(config.ParentDataset.DatasetId);

            return await GetColumnSchema(config, SchemaID);
        }

        #endregion

        #region Schema_Endpoints
        
        ///// <summary>
        ///// Gets schema information
        ///// </summary>
        ///// <param name="schemaId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        //[Route("schema/{schemaId}")]
        //[SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaInfoModel))]
        //public async Task<IHttpActionResult> GetSchemaInfo(int schemaId)
        //{
        //    try
        //    {
        //        SchemaDto dto = _schemaService.GetFileSchemaDto(schemaId);
        //        SchemaInfoModel model = dto.ToModel();
        //        return Ok(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.ToString());
        //    }
        //}

        ///// <summary>
        ///// Get list all revisions for schema
        ///// </summary>
        ///// <param name="schemaId"></param>
        ///// <returns></returns>
        //[HttpGet]

        //[Route("schema/{schemaId}/revisions")]
        //[SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<SchemaInfoModel>))]
        //public async Task<IHttpActionResult> GetSchemaRevisions(int schemaId)
        //{
        //    try
        //    {
        //        List<SchemaRevisionDto> revisionsList = _schemaService.GetSchemaRevisionDtoBySchema(schemaId);
        //        List<SchemaRevisionModel> modelList = revisionsList.ToModel();
        //        return Ok(modelList);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.ToString());
        //    }
        //}


        /// <summary>
        /// ges column schema metadate form schema
        /// </summary>
        /// <param name="SchemaID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("schemas/{SchemaID}/columns")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SchemaDetailModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetColumnSchemaInformationForSchema(int SchemaID)
        {
            DatasetFileConfig dfc = _dsContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == SchemaID).FirstOrDefault();

            if (dfc != null)
            {
                ValidateViewPermissionsForDataset(dfc.ParentDataset.DatasetId);
            }
            else
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
            }

            SchemaDetaiApilDTO dto = _configService.GetSchemaDetailDTO(SchemaID);
            SchemaDetailModel sdm = new SchemaDetailModel(dto);

            return Ok(sdm);
        }
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
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("PublishMessage")]
        public IHttpActionResult PublishMessage([FromBody] KafkaMessage message)
        {
            try
            {
                if (message == null)
                {
                    Logger.Error($"jobcontroller-publishmessage null message");
                    throw new ArgumentException("message parameter is null");
                }
                else
                {
                    Logger.Debug($"jobcontroller-publishmessage message:{ JsonConvert.SerializeObject(message) }");
                }

                _dataFlowService.PublishMessage(message.Key, message.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Error($"jobcontroller-publishmessage failure", ex);
                return InternalServerError();
            }
        }
        
        /// <summary>
         /// Publish message to messaging queue
         /// </summary>
         /// <param name="message"></param>
         /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.Unauthorized, null, null)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
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
                    Logger.Debug($"jobcontroller-publishmessage message:{message.ToString()}");

                    kMsg = JsonConvert.DeserializeObject<KafkaMessage>(message);
                }

                _dataFlowService.PublishMessage(kMsg.Key, kMsg.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Error($"jobcontroller-publishmessage failure", ex);
                return InternalServerError();
            }
        }
        #endregion

        #region Private Methods
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
                us = _securityService.GetUserSecurity(_dsContext.GetById<Dataset>(datasetId), _userService.GetCurrentUser());
            }
            catch (Exception ex)
            {
                Logger.Error($"metadatacontroller-validateviewpermissionsfordataset failed to retrieve UserSecurity object", ex);
                throw new HttpResponseException(System.Net.HttpStatusCode.InternalServerError);
            }

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
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

            if (!us.CanEditDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }
        }
        #endregion


    }
}
