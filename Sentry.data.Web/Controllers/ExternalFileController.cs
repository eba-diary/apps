using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Sentry.Configuration;
using System.IO;

namespace Sentry.data.Web.Controllers
{
    public class ExternalFileController : Controller
    {
        private IODCFileProvider _odcFileProvider;

        public ExternalFileController(IODCFileProvider odc)
        {
            _odcFileProvider = odc;
        }

        public ActionResult DataModel(string dataModelName, string filename)
        {
            string filenameAndPath = Path.Combine(Config.GetHostSetting("DataModelPath"), dataModelName, filename);
            string mimeType = MimeMapping.GetMimeMapping(filename);
            return new FilePathResult(filenameAndPath, mimeType);
        }

        public ActionResult ODCFile(ComponentElement ce)
        {
            string xml = _odcFileProvider.GetXMLString(ce);
            string file = ce.Name.Replace(" ", "_");
            return File(Encoding.UTF8.GetBytes(xml), "text/xml", string.Format("{0}.odc", file));
        }
    }
}