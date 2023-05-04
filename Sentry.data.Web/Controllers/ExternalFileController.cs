using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Sentry.Configuration;
using System.IO;
using System.Net;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web.Controllers
{
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.USE_APP)]
    public class ExternalFileController : BaseController
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

        public ActionResult External(string filePath, string fileName)
        {
            string filenameAndPath = filePath;
            string mimeType = MimeMapping.GetMimeMapping(fileName);
            return new FilePathResult(filenameAndPath, mimeType);
        }

        //  We can't directly open a network file using Javascript, eg
        //      window.open("\\SomeNetworkPath\ExcelFile\QExcelFile.xls");
        //
        //  Instead, we need to get Javascript to call this groovy helper class which loads such a file, then sends it to the stream.  
        //      window.open("/DownloadExternalFile?filename=//SomeNetworkPath/ExcelFile/QExcelFile.xls");
        //
        public void DownloadExternalFile(string pathAndFilename)
        {

            string userName = "SHOESD01\\" + SharedContext.CurrentUser.AssociateId;

            List<DirectoryUtilities.AccessRights> expectedRights = new List<DirectoryUtilities.AccessRights>()
            {
                DirectoryUtilities.AccessRights.List_Folder_and_Read_Data
            };

            if (DirectoryUtilities.HasPermission(userName, pathAndFilename, expectedRights))
            {
                var context = System.Web.HttpContext.Current;

                string filename = System.IO.Path.GetFileName(pathAndFilename);      //  eg  "QExcelFile.xls"

                context.Response.ClearContent();

                using (WebClient webClient = new WebClient())
                using (Stream stream = webClient.OpenRead(pathAndFilename))
                {
                    // Process image...
                    byte[] data1 = new byte[stream.Length];
                    stream.Read(data1, 0, data1.Length);

                    context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", filename));
                    context.Response.BinaryWrite(data1);

                    context.Response.Flush();
                    context.Response.SuppressContent = true;
                    context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        [HttpGet]
        public JsonResult HasReadPermissions(string pathAndFilename)
        {
            List<DirectoryUtilities.AccessRights> expectedRights = new List<DirectoryUtilities.AccessRights>()
            {
                DirectoryUtilities.AccessRights.List_Folder_and_Read_Data
            };

            //Ensure service account has necessary access to path
            try
            {
                DirectoryUtilities.HasPermission("SHOESD01\\" + Config.GetHostSetting("ServiceAccountID"), pathAndFilename, expectedRights);
            }
            catch (Exception ex)
            {
                Configuration.Logging.Logger.Logger.Error($"Service account does not have access to exhibit directory - ServiceAccount:{Config.GetHostSetting("ServiceAccountID")} Path:{pathAndFilename}",ex);
                throw new HttpException(401, "Service Account does not have access to directory");
            }


            //Check users permissions
            string userName = "SHOESD01\\" + SharedContext.CurrentUser.AssociateId;
            if (DirectoryUtilities.HasPermission(userName, pathAndFilename, expectedRights))
            {
                return Json(new { Success = true, HasPermission = true}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, HasPermission = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}