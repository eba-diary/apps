using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;
using Amazon.S3.IO;
using StructureMap;

namespace Sentry.data.Infrastructure
{
#pragma warning disable S101 // Types should be named in camel case
    class SASServiceProvider : ISASService
#pragma warning restore S101 // Types should be named in camel case
    {
        private CookieContainer _cookies = null;
        
        private CookieContainer cookies
        {
            get
            {
                if (null == _cookies)
                {
                    _cookies = new CookieContainer();
                    
                }

                return _cookies;
            }
        }

        private IContainer Container { get; set; }


        public void ConvertToSASFormat(int datafileId, string filename, string delimiter, int guessingrows)
        {
            DatasetFile df = null;
            try
            {
                using (Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();

                    df = _datasetContext.GetById<DatasetFile>(datafileId);

                    //Throw error and stop processing if datafile record is not found
                    if (df == null)
                    {
                        throw new ArgumentException($"No DatasetFile Found - ID:{datafileId}");
                    }

                    StringBuilder url = GernerateSASURL(filename, df.Dataset.DatasetCategories.First().Name, delimiter, guessingrows);

                
                    //JCG TODO: Revisit after SAS fixes issue around initial logon attempt fails, additional attempts succeed.
                    Retry.Do(() => CallSASConvertSTP(url), TimeSpan.FromSeconds(15), 2);
                }
            }
            catch
            {
                throw;
            }
        }

        private string GenerateSchemaFile(string schema, string delimiter)
        {
            Guid result;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(DateTime.Now.ToString()));
                result = new Guid(hash);
            }
            

            string outfilename = result.ToString() + ".sas";
            
            //Generate SAS length in includes file
            StringBuilder lengthline = new StringBuilder();
            StringBuilder inputline = new StringBuilder();
            lengthline.Append("length ");
            inputline.Append("input ");
            int truncatedColCount = 1;
            foreach (string col in schema.Split(Convert.ToChar(delimiter)))
            {
                //Need to follow sas variable name restrictions
                //http://support.sas.com/documentation/cdl/en/lrcon/62955/HTML/default/viewer.htm#a000998953.htm
                string colName = null;
                if (col.Length >= 32)
                {
                    //Take first 30 characters and append and underscore (_) and an incremetor to eliminate potential duplicate column names
                    colName = col.Substring(0, 30) + $"_{truncatedColCount}";
                    truncatedColCount++;
                }
                else
                {
                    colName = col;
                }

                lengthline.Append($"{colName} ");
                inputline.Append($"'{colName}'N ");
            }
            lengthline.Append("$ 1024;");
            inputline.Append(";");

            //Combine all lines into final includes file
            StringBuilder outIncludesFile = new StringBuilder();
            outIncludesFile.AppendLine(lengthline.ToString());
            outIncludesFile.AppendLine(inputline.ToString());

            using (StreamWriter sw = new StreamWriter($"\\\\sentry.com\\appfs_nonprod\\sasrepository\\datasets\\datasetmanagement\\schemafiles\\{outfilename}", true))
            {
                sw.Write(outIncludesFile.ToString());
            }

            return outfilename;
        }

        private void CallSASConvertSTP(StringBuilder url)
        {
            HttpWebRequest httpRequest = WebRequest.Create(url.ToString()) as HttpWebRequest;

            httpRequest.CookieContainer = cookies;
            httpRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            httpRequest.Timeout = (int)TimeSpan.FromMinutes(120).TotalMilliseconds;
            HttpWebResponse httpResponse = httpRequest.GetResponse() as HttpWebResponse;
            string responsecontent = string.Empty;

            using (var stream = httpResponse.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
               responsecontent = reader.ReadToEnd();
                if (!(String.IsNullOrEmpty(responsecontent)))
                {
                    throw new WebException("Error Executing SAS Conversion", new WebException(responsecontent));
                }
            }
        }

        private static StringBuilder GernerateSASURL(string filename, string category, string delimiter, int guessingrows)
        {
            StringBuilder url = new StringBuilder();
            url.Append(Configuration.Config.GetHostSetting("PushToSASUrl"));
            url.Append(Uri.EscapeUriString("&_username="));
            url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("ServiceAccountID")));
            url.Append(Uri.EscapeUriString("&_password="));
            url.Append("{sas001}" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Configuration.Config.GetHostSetting("ServiceAccountPass"))));
            url.Append(Uri.EscapeUriString("&_program="));
            if (Path.GetExtension(filename) == ".csv")
            {
                url.Append(Configuration.Config.GetHostSetting("SASCsvStpFolder"));
                url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("SASCsvStpName")));
            }
            url.Append(Uri.EscapeUriString("&ROOT_DIR="));
            url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("PushToSASTargetPath")));
            url.Append(Uri.EscapeUriString("&FILE_NAME="));
            url.Append(Uri.EscapeUriString(Path.GetFileNameWithoutExtension(filename)));
            url.Append(Uri.EscapeUriString("&FILE_EXT="));
            url.Append(Uri.EscapeUriString(Path.GetExtension(filename)));
            url.Append(Uri.EscapeUriString("&CATEGORY="));
            url.Append(Uri.EscapeUriString(category));
            url.Append(Uri.EscapeUriString("&DELIMITER="));
            url.Append(Uri.EscapeUriString(delimiter));
            url.Append(Uri.EscapeUriString("&GUESSINGROWS="));
            url.Append(Uri.EscapeUriString(guessingrows.ToString()));

            //Future enhancement is to add this to end of url, then return error output for user to debug issue.
            //url.Append(Uri.EscapeUriString("&_DEBUG=LOG"));

            Sentry.Common.Logging.Logger.Info($"URL: {url.ToString()}");
            return url;
        }

        /// <summary>
        /// SAS is picky about file names.  This method removes problem characters.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GenerateSASFileName(string filename)
        {
            
            string fn = Path.GetFileNameWithoutExtension(filename);

            //Replace all characters, except digit and upper\lower case letters, with an underscore
            fn = Regex.Replace(fn, @"[^0-9a-zA-Z]+", "_");

            //Prefix the file name with an underscore
            if (!(Regex.IsMatch(fn, @"^([A-Za-z]|_)"))){
                fn = "_" + fn;
            }            

            //Substring to 32 characters if over
            if (fn.Length > 32)
            {
                fn = fn.Substring(0, 32);
            }

            fn = fn + Path.GetExtension(filename);

            return fn;
        }


        public static class Retry
        {
            public static void Do(
                Action action,
                TimeSpan retryInterval,
                int retryCount = 3)
            {
                Do<object>(() =>
                {
                    action();
                    return null;
                }, retryInterval, retryCount);
            }

            public static T Do<T>(
                Func<T> action,
                TimeSpan retryInterval,
                int retryCount = 3)
            {
                var exceptions = new Exception();

                for (int retry = 0; retry < retryCount; retry++)
                {
                    try
                    {
                        if (retry > 0)
                            Thread.Sleep(retryInterval);
                        return action();
                    }
                    catch (Exception ex)
                    {
                        if (retry == 0)
                        {
                            //exceptions.Add(ex);
                            exceptions = ex;
                        }                        
                    }
                }

                throw new WebException(exceptions.Message, new Exception(exceptions.InnerException.ToString()));
            }
        }
    }
}
