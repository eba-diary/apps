using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Helpers.Paginate;
using Sentry.data.Web.Models.ApiModels.DatasetFile;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Sentry.data.Web.Models.ApiModels;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATAFILE)]
    [WebApiAuthorizeUseApp]
    public class DataFileController : BaseWebApiController
    {
        private readonly IDatasetFileService _datafileService;
        private readonly IDataFeatures _dataFeatures;

        public DataFileController(IDatasetFileService dataFileService, IDataFeatures dataFeatures)
        {
            _datafileService = dataFileService;
            _dataFeatures = dataFeatures;
        }



        /// <summary>
        /// Return all data files associated with schema.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="pageNumber">Default is 1</param>
        /// <param name="pageSize">Default is 1000, Max is 10000</param>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(PagedResponse<DatasetFileModel>))]
        public async Task<IHttpActionResult> GetDataFiles([FromUri] int datasetId, [FromUri] int schemaId, [FromUri] int? pageNumber = 1, [FromUri] int? pageSize = 1000)
        {
            IHttpActionResult GetSchemaDatasetFilesFunction()
            {
                /*****************************************************
                    This pattern for paging was replicated from the following site
                    https://code-maze.com/angular-material-table/
                    If this need expands, there is additional refactoring 
                       that can be done to allow each type to have its own metadata.
                 ******************************************************/
                PageParameters pagingParams = new PageParameters(pageNumber, pageSize);

                PagedList<DatasetFileDto> dtoList = _datafileService.GetAllDatasetFileDtoBySchema(schemaId, pagingParams);

                PagedList<DatasetFileModel> modelList = new PagedList<DatasetFileModel>(dtoList.ToModel(), dtoList.TotalCount, dtoList.CurrentPage, dtoList.PageSize);
                PagedResponse<DatasetFileModel> response = new PagedResponse<DatasetFileModel>(modelList);

                return Ok(response);
            }

            return ApiTryCatch(nameof(DataFileController), nameof(GetDataFiles), $"datasetid:{datasetId} schemaId{schemaId}", GetSchemaDatasetFilesFunction);
        }

        /// <summary>
        /// Update data file metadata
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="dataFileId"></param>
        /// <param name="dataFileModel"></param>
        /// <returns></returns>
        [HttpPut]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/datafile/{dataFileId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task<IHttpActionResult> UpdateDataFile(int datasetId, int schemaId, int dataFileId, [FromBody] DatasetFileModel dataFileModel)
        {
            IHttpActionResult UpdateDataFileFunction()
            {
                List<string> validationResults = dataFileModel.Validate();
                if (dataFileId != dataFileModel.DatasetFileId)
                {
                    validationResults.Add($"The route datasetFileId {dataFileId} does not match the DatasetFileModel.DatasetFileId {dataFileModel.DatasetFileId}");
                }
                if (datasetId != dataFileModel.DatasetId)
                {
                    validationResults.Add($"The route datasetId {datasetId} does not match the DatasetFileModel.DatasetId {dataFileModel.DatasetId}");
                }
                if (schemaId != dataFileModel.SchemaId)
                {
                    validationResults.Add($"The route schemaId {schemaId} does not match the DatasetFileModel.SchemaId {dataFileModel.SchemaId}");
                }

                if (validationResults.Any())
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, $"Invalid request: {string.Join(" | ", validationResults)}");
                }

                DatasetFileDto dto = dataFileModel.ToDto();

                _datafileService.UpdateAndSave(dto);

                return Ok();
            }

            return ApiTryCatch(nameof(DataFileController), nameof(UpdateDataFile), $"datasetid:{dataFileModel.DatasetId} schemaId{dataFileModel.SchemaId} datasetfileId:{dataFileModel.DatasetFileId}", UpdateDataFileFunction);
        }


        [HttpDelete]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        public async Task<IHttpActionResult> DeleteDataFiles([FromUri] int datasetId, [FromUri] int schemaId, [FromUri] string[] userFileNameList=null, [FromUri] string[] userFileIdList=null)
        {
            //FEATURE FLAG CHECK:  REMOVE LATER
            if (!_dataFeatures.CLA4049_ALLOW_S3_FILES_DELETE.GetValue())
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, "Feature not available to this user.");
            }
            
            //STEP 1:  DETERMINE WHAT WAS PASSED IN
            bool userFileNameListPassed = (userFileNameList != null && userFileNameList.Length > 0) ? true : false;
            bool userIdListPassed = (userFileIdList != null && userFileIdList.Length > 0) ? true : false;

            //STEP 2:  VALIDATIONS TO ENSURE THEY DON'T PASS BOTH datasetFileIdList AND datasetFileIdList
            if (userFileNameListPassed && userIdListPassed)    
            {
                return Content(System.Net.HttpStatusCode.NotAcceptable, "Cannot pass " + nameof(userFileNameList) + " AND " + nameof(userFileIdList) + " at the same time.  Please include only " + nameof(userFileNameList) + " OR " + nameof(userFileIdList));
            }


            //STEP 3:  TURN USER LIST INTO DBLIST
            List<DatasetFile> dbList = new List<DatasetFile>();
            if (userFileNameListPassed)
            {
                dbList = _datafileService.GetDatasetFileList(userFileNameList);

                //VALIDATE FILE COUNT
                if (dbList.Count > userFileNameList.Length)
                {
                    return Content(System.Net.HttpStatusCode.NotAcceptable, "No Files were deleted. " + nameof(userFileNameList) + " contained a file that would delete more than one file.  Please only pass filenames that would delete a single file.");
                }
            }
            else
            {
                //VALIDATE NON INTEGERS
                int[] idListINT = System.Array.ConvertAll(userFileIdList, w => int.TryParse(w, out var x) ? x : -1);
                List<int> invalidIds = idListINT.Where(w => w == -1).ToList();
                if (invalidIds.Count > 0)
                {
                    return Content(System.Net.HttpStatusCode.NotAcceptable, nameof(userFileIdList) + " contains non integers.  Please pass all integers.");
                }

                dbList = _datafileService.GetDatasetFileList(idListINT);
            }



            return Ok("Thanks for using DSC!");
        }

    }
}
