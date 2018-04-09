using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace Sentry.data.Web.Controllers
{
    public class LineageController : ApiController
    {
        private IDataAssetProvider _dataAssetProvider;
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;
        private List<DataAsset> das;

        public LineageController(IDataAssetProvider dap, MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _dataAssetProvider = dap;
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetLineageFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceElement_NME","SourceObject_NME", "SourceObjectField_NME" ,"Source_TXT", "Transformation_TXT" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME);

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetSourceElementsFor(int? DataAsset_ID, string DataElement_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceElement_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID).Select(x => x.SourceElement_NME).Distinct();

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }



        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetDataElementsFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceObject_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME).Select(x => x.DataElement_NME).Distinct();
                            
                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }




        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetSourceObjectsFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceObject_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME).Select(x => x.SourceObject_NME).Distinct();

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetDataObjectsFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceObject_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME).Select(x => x.DataObject_NME).Distinct();

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetSourceFieldDetailsFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceObjectField_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME).Select(x => x.SourceObjectField_NME).Distinct();

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetDataFieldDetailsFor(int? DataAsset_ID, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                List<string> fields = new List<string>() {
                    "SourceObject_NME" };

                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, fields, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME).Select(x => x.DataObjectField_NME).Distinct();

                var businessTerms = _dsContext.BusinessTerms(DataElementCode.Lineage, DataAsset_ID);

                var reply = from a in allLineage
                            join b in businessTerms
                            on a equals b
                            select a;

                if(reply.Count() != 0)
                {
                    return Ok(reply);
                }
                else
                {
                    return Ok(allLineage);
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }
    }
}
