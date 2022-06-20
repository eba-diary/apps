using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Data;
using Sentry.data.Core;
using Sentry.DataTables.QueryableAdapter;
using Sentry.DataTables.Mvc;
using Sentry.DataTables.Shared;
using DoddleReport.Web;
using DoddleReport;
using Sentry.Core;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using Newtonsoft.Json;

namespace Sentry.data.Web.Controllers
{
    /*[AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]*/
    public class AdminController : BaseController
    {
        private IKafkaConnectorProvider _connectorProvider;

        public AdminController(IKafkaConnectorProvider connectorProvider)
        {
            _connectorProvider = connectorProvider;
        }

        // GET: Admin
        public async Task<ActionResult> Index()
        {
            Dictionary<string, string> myDict =
            new Dictionary<string, string>();

            HttpResponseMessage response = await _connectorProvider.GetRequestAsync("/connectors/SRPL_TEST_QUOTES_CONNECT_EAST_2_1/status").ConfigureAwait(false);
            var test = response.Content.ReadAsStringAsync().Result;
            ConfluentConnectorRootDTO testObject = JsonConvert.DeserializeObject<ConfluentConnectorRootDTO>(test);


            myDict.Add("1", "Reprocess Data Files");
            myDict.Add("2", "File Processing Logs");
            myDict.Add("3", "Parquet Null Rows");
            myDict.Add("4", "General Raw Query Parquet");

            return View(myDict);
        }
        
        public ActionResult GetAdminTest(string viewId)
        {
            string viewPath = "";
            switch (viewId)
            {
                case "1":
                    viewPath = "_DataFileReprocessing";
                    break;
                case "2":
                    viewPath = "_AdminTest2";
                    break;
                case "3":
                    viewPath = "_AdminTest3";
                    break;
                case "4":
                    viewPath = "_AdminTest4";
                    break;
            }

            return PartialView(viewPath);
        }
    }
}