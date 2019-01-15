using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using Sentry.data.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace Sentry.data.Web.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_LINEAGE)]
    public class LineageController : BaseWebApiController
    {

        public readonly MetadataRepositoryService _metadataRepositoryService;

        public readonly IDataAssetContext _dataAssetContext;
        public readonly IDatasetContext _dataSetContext;

        public readonly IAssociateInfoProvider _associateInfoService;
        public readonly UserService _userService;

        public LineageController(MetadataRepositoryService metadataRepositoryService, IDataAssetContext dataAssetContext, IDatasetContext datasetContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dataAssetContext = dataAssetContext;
            _dataSetContext = datasetContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        private class SearchTerms {

            public String Business_Term { get; set; }
            public String Consumption_Layer { get; set; }
            public String Lineage_Table { get; set; }
            public int Results_Returned { get; set; }
        }


        /// <summary>
        /// gets a lineage for dataAsset id
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("lineage")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> GetLineageFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(DataElement_NME) || !String.IsNullOrWhiteSpace(DataObject_NME) || !String.IsNullOrWhiteSpace(DataObjectField_NME))
                {
                    var allLineage = _dataAssetContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE);


                    Event e = new Event();
                    e.EventType = _dataSetContext.EventTypes.Where(w => w.Description == "Search").FirstOrDefault();
                    e.Status = _dataSetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
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
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// Not sure what the difference is on this one and the GetLineageFor method...one of these should be updated
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("populateFirst")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> PopulateFirstList(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "")
        {
            try
            {
                var allLineage = _dataAssetContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME)
                    .GroupBy(x => new { x.DataElement_NME, x.DataObject_NME, x.DataObjectField_NME }).Select(x => x.First()).ToList();

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }


        /// <summary>
        /// gets description for lineage
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("description")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> GetDescriptionFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var doDesc = _dataAssetContext.Description(DataAsset_ID, DataObject_NME, DataObjectField_NME, LineCDE);

                if(doDesc.DataObject_DSC == null && doDesc.DataObjectField_DSC == null)
                {
                    //This should not return a NotFound because it returns a 404 meaning the page could not be found. The page was found because it was ablt to get into this method.
                    //it should really return a 204 (successful but with no content.)
                    //return NoContent(); // - this uses the base web api controller.
                    return NotFound(); 
                }
                else
                {
                    return Ok(doDesc);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// get elements from dataset
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("elements")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> GetDataElementsFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            { 
                var allLineage = _dataAssetContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Select(x => x.DataElement_NME).Distinct().OrderBy(x => x);
                            
                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }


        /// <summary>
        /// gets lineage objects
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("objects")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> GetDataObjectsFor(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var allLineage = _dataAssetContext.Lineage(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Select(x => x.DataObject_NME).Distinct().OrderBy(x => x);

                return Ok(allLineage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// gets lineage business term description
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("businessTermDescription")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> BusinessTermDescription(int? DataAsset_ID = null, string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var businessTermDescription = _dataAssetContext.BusinessTermDescription(DataElementCode.BusinessTerm, DataAsset_ID, DataObjectField_NME, LineCDE).Distinct().First();

                return Ok(businessTermDescription);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }


        /// <summary>
        /// gets lineage business term
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("businessTerms")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> BusinessTerms(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var businessTerms = _dataAssetContext.BusinessTerms(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(businessTerms);
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// get lineage layers
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("layers")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> Layers(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var layers = _dataAssetContext.ConsumptionLayers(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(layers);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

        /// <summary>
        /// gets lineage tables
        /// </summary>
        /// <param name="DataAsset_ID"></param>
        /// <param name="DataElement_NME"></param>
        /// <param name="DataObject_NME"></param>
        /// <param name="DataObjectField_NME"></param>
        /// <param name="LineCDE"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("lineageTables")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task<IHttpActionResult> LineageTables(int? DataAsset_ID = null, string DataElement_NME = "", string DataObject_NME = "", string DataObjectField_NME = "", string LineCDE = "")
        {
            try
            {
                var layers = _dataAssetContext.LineageTables(DataElementCode.Lineage, DataAsset_ID, DataElement_NME, DataObject_NME, DataObjectField_NME, LineCDE).Distinct().Where(x => x.Length > 1).OrderBy(x => x);

                return Ok(layers);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return NotFound();
            }
        }

    }
}
