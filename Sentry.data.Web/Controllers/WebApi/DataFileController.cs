﻿using Sentry.data.Core;
using Sentry.data.Core.Helpers.Paginate;
using Sentry.data.Web.Models.ApiModels;
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

        /// <summary>
        /// Directions:  Please pass either deleteFilesModel.UserFileIdList OR deleteFilesModel.UserFileNameList.  Both cannot be passed at same time.
        /// Warning:  Even though this is a POST, this will delete passed in list.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="deleteFilesParamModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/schema/{schemaId}/Delete")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.Forbidden)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        public IHttpActionResult DeleteDataFiles(int datasetId, int schemaId, [FromBody] DeleteFilesParamModel deleteFilesParamModel)
        {
            //SECURITY CHECK
            UserSecurity us = _datafileService.GetUserSecurityForDatasetFile(datasetId);
            if (!us.CanDeleteDatasetFile)
            {
                return Content(System.Net.HttpStatusCode.Forbidden, "Feature not available to this user.");
            }

            string simpleError = _datafileService.ValidateDeleteDataFilesParams(datasetId, schemaId, deleteFilesParamModel.ToDto());
            if (simpleError != null)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, simpleError);
            }

            //TURN USER LIST INTO DBLIST
            List<DatasetFile> dbList;

            //IF userFileNameList PASSED
            if (deleteFilesParamModel.UserFileNameList != null && deleteFilesParamModel.UserFileNameList.Length > 0)
            {
                dbList = _datafileService.GetDatasetFileList(datasetId, schemaId, deleteFilesParamModel.UserFileNameList);
            }
            else  //userIdList PASSED
            {
                //VALIDATE LESS THAN 1
                if (deleteFilesParamModel.UserFileIdList.Any(w => w < 1 ) )
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, nameof(deleteFilesParamModel.UserFileIdList) + " contains an item less than one.  Please pass items greater than zero.");
                }
                dbList = _datafileService.GetDatasetFileList(datasetId, schemaId, deleteFilesParamModel.UserFileIdList);
            }


            //VALIDATE: ANYTHING TO DELETE
            if(dbList != null && dbList.Count == 0)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Nothing found to delete.");
            }

            //FINAL STEP :  CALL SERVICE TO DELETE METADATA AND DPP TO DELETE
            _datafileService.Delete(datasetId, schemaId, dbList);

            return Ok("Delete Successful.  Thanks for using DSC!");
        }
    }
}
