﻿using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATAFLOW)]
    public class DataFlowController : BaseWebApiController
    {
        private readonly IDataFlowService _dataFlowService;
        private readonly IUserService _userService;
        private readonly Lazy<IDataApplicationService> _dataApplicationService;

        public DataFlowController(IDataFlowService dataFlowservice, Lazy<IDataApplicationService> dataApplicationService,
            IUserService userService)
        {
            _dataFlowService = dataFlowservice;
            _dataApplicationService = dataApplicationService;
            _userService = userService;
        }

        private IDataFlowService DataFlowService
        {
            get { return _dataFlowService; }
        }

        private IDataApplicationService DataApplicaitonService
        {
            get { return _dataApplicationService.Value; }
        }

        ////// GET api/<controller>
        ////public IEnumerable<string> Get()
        ////{
        ////    return new string[] { "value1", "value2" };
        ////}

        ////// GET api/<controller>/5
        ////public string Get(int id)
        ////{
        ////    return "value";
        ////}

        ////// POST api/<controller>
        ////public void Post([FromBody] string value)
        ////{
        ////}

        ////// PUT api/<controller>/5
        ////public void Put(int id, [FromBody] string value)
        ////{
        ////}

        ////// DELETE api/<controller>/5
        ////public void Delete(int id)
        ////{
        ////}

        /// <summary>
        /// Retrieve dataflow detail metadata with the id(s) provided
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="storagecode"></param>
        /// <returns></returns>
        [ApiVersionBegin(WebAPI.Version.v2)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public IHttpActionResult RetrieveDataFlowMetadata([FromUri] int? datasetId, [FromUri] int? schemaId, [FromUri] string storagecode)
        {
            // Dictionary to manage and check method params
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add(new KeyValuePair<string, object>(nameof(datasetId), datasetId));
            parameters.Add(new KeyValuePair<string, object>(nameof(schemaId), schemaId));
            parameters.Add(new KeyValuePair<string, object>(nameof(storagecode), storagecode));

            int counter = 0; // counter to check for excess amount of passed in params

            foreach (var item in parameters)
            {
                if (item.Value != null) counter++;
            }

            KeyValuePair<string, object> itemCheck = new KeyValuePair<string, object>();

            if (counter == 0 || counter > 1)
            {
                return BadRequest(); // returns BadRequest result in the case of excessive or invalid parms
            } else
            {
                foreach (var item in parameters)
                {
                    if (item.Value != null)
                    {
                        itemCheck = new KeyValuePair<string, object>(item.Key, item.Value);
                    }
                }
            }

            Expression<Func<DataFlow, bool>> expression = null;

            switch (itemCheck.Key)
            {
                case "datasetId":
                    expression = w => w.DatasetId == (int)itemCheck.Value;
                    break;
                case "schemaId":
                    expression = w => w.SchemaId == (int)itemCheck.Value;
                    break;
                case "storagecode":
                    expression = w => w.FlowStorageCode == (string)itemCheck.Value;
                    break;
            }

            List<DataFlowDetailDto> dtoList = _dataFlowService.GetDataFlowDetailDto(expression);
            List<Models.ApiModels.Dataflow.DataFlowDetailModel> modelList = new List<Models.ApiModels.Dataflow.DataFlowDetailModel> ();

            DataFlowExtensions.MapToDetailModelList(dtoList, modelList);

            return Ok();
        }

        

        /// <summary>
        /// Create new dataflow (v3) for each dataflow provided
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        [ApiVersionBegin(WebAPI.Version.v2)]
        /*[WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("updatedataflows")]
        [HttpPost]
        public IHttpActionResult UpdateDataFlows(int[] idList)
        {
            DataFlowService.UpgradeDataFlows(idList);

            return Ok();
        }

        /// <summary>
        /// Delete each dataflow associated with id(s) provided
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        [ApiVersionBegin(WebAPI.Version.v2)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("deletedataflows")]
        [HttpPost]
        public IHttpActionResult DeleteDataFlows(int[] idList)
        {

            bool isSuccessful = DataApplicaitonService.DeleteDataFlow_Queue(idList.ToList(), _userService.GetCurrentUser().AssociateId, true);

            if (!isSuccessful)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "Unable to queue all deletes");
            }
            return Ok();
        }
    }
}