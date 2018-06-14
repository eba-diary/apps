using Newtonsoft.Json;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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

        private class SearchTerms {

            public String Business_Term { get; set; }
            public String Consumption_Layer { get; set; }
            public String Lineage_Table { get; set; }
            public int Results_Returned { get; set; }
        }



        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetLineageFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(DataElement_NME) || !String.IsNullOrWhiteSpace(DataObject_NME) || !String.IsNullOrWhiteSpace(DataObjectField_NME))
                {
                    var allLineage = _dsContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE);


                    Event e = new Event();
                    e.EventType = _dsContext.EventTypes.Where(w => w.Description == "Search").FirstOrDefault();
                    e.Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = RequestContext.Principal.Identity.Name;
                    e.DataAsset = DataAsset_ID;
                    e.Line_CDE = LineCDE;

                    e.Search = JsonConvert.SerializeObject(new SearchTerms() {
                        Business_Term = HttpUtility.UrlDecode(DataObjectField_NME),
                        Consumption_Layer = HttpUtility.UrlDecode(DataElement_NME),
                        Lineage_Table = HttpUtility.UrlDecode(DataObject_NME),
                        Results_Returned = allLineage.Count
                    });

                    e.Reason = "Searched Lineage";
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
                

                    return Ok(allLineage);
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> PopulateFirstList(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME)
                    .GroupBy(x => new { x.DataElement_NME, x.DataObject_NME, x.DataObjectField_NME }).Select(x => x.First()).ToList();

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
        public async Task<IHttpActionResult> GetDescriptionFor(int? DataAsset_ID = null, string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var doDesc = _dsContext.Description(DataAsset_ID, DataObject_NME, DataObjectField_NME, LineCDE);

                if(doDesc.DataObject_DSC == null && doDesc.DataObjectField_DSC == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(doDesc);
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetDataElementsFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            { 
                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Select(x => x.DataElement_NME).Distinct().OrderBy(x => x);
                            
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
        public async Task<IHttpActionResult> GetDataObjectsFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var allLineage = _dsContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Select(x => x.DataObject_NME).Distinct().OrderBy(x => x);

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
        public async Task<IHttpActionResult> BusinessTermDescription(int? DataAsset_ID = null, string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var businessTermDescription = _dsContext.BusinessTermDescription(DataElementCode.BusinessTerm, DataAsset_ID, DataObjectField_NME, LineCDE).Distinct().First();

                return Ok(businessTermDescription);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }



        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> BusinessTerms(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var businessTerms = _dsContext.BusinessTerms(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(businessTerms);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> Layers(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var layers = _dsContext.ConsumptionLayers(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(layers);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> LineageTables(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var layers = _dsContext.LineageTables(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(layers);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

    }
}
