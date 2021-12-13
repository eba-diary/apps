using Sentry.data.Core;
using Sentry.data.Web.Models.ApiModels.DatasetFile;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATAFILE)]
    [WebApiAuthorizeUseApp]
    public class DataFileController : BaseWebApiController
    {
        private readonly IDatasetFileService _datafileService;

        public DataFileController(IDatasetFileService dataFileService)
        {
            _datafileService = dataFileService;
        }



        /// <summary>
        /// Return all data files associated with schema.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="pageNumber">Default is 1</param>
        /// <param name="pageSize">Default is 10, Max is 1000</param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<DatasetFileModel>))]
        public async Task<IHttpActionResult> GetDataFiles([FromUri] int datasetId, [FromUri] int schemaId, [FromUri] int? pageNumber, [FromUri] int? pageSize)
        {
            IHttpActionResult GetSchemaDatasetFilesFunction()
            {
                //DatasetFileConfigDto dto = _configService.GetDatasetFileConfigDtoByDataset(datasetId).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
                //List<DatasetFileDto> dtoList = DatasetFileService.GetAllDatasetFilesBySchema(schemaId, x => x.ParentDatasetFileId == null).ToList();

                PageParameters pagingParams = new PageParameters() { PageNumber = pageNumber, PageSize = pageSize };

                List<DatasetFileDto> dtoList = _datafileService.GetAllDatasetFilesBySchema(schemaId, pagingParams).ToList();

                List<DatasetFileModel> modelList = dtoList.ToModel();

                return Ok(modelList);
            }

            return ApiTryCatch(nameof(DataFileController), nameof(GetDataFiles), $"datasetid:{datasetId} schemaId{schemaId}", GetSchemaDatasetFilesFunction);
        }

        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/datafile/{dataFileId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task<IHttpActionResult> UpdateDataFile([FromBody] DatasetFileModel dataFileModel)
        {
            IHttpActionResult UpdateDataFileFunction()
            {
                DatasetFileDto dto = dataFileModel.ToDto();

                _datafileService.UpdateAndSave(dto);

                return Ok();
            }

            return ApiTryCatch(nameof(DataFileController), nameof(UpdateDataFile), $"datasetid:{dataFileModel.DatasetId} schemaId{dataFileModel.SchemaId} datasetfileId:{dataFileModel.DatasetFileId}", UpdateDataFileFunction);
        }
    }
}
