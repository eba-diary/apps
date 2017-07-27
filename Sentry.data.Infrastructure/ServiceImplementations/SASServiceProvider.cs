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

namespace Sentry.data.Infrastructure
{
    class SASServiceProvider : ISASService
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

        public void ConvertToSASFormat(string filename, string category)
        {
            StringBuilder url = new StringBuilder();
            url.Append(Configuration.Config.GetHostSetting("PushToSASUrl"));
            url.Append(Uri.EscapeUriString("&_username="));
            url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("PushToSASUser")));
            url.Append(Uri.EscapeUriString("&_password="));
            url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("PushToSASPass")));
            url.Append(Uri.EscapeUriString("&_program="));
            if(Path.GetExtension(filename) == ".csv")
            {
                url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("SASCsvStpFolder")));
                url.Append(Uri.EscapeUriString(Configuration.Config.GetHostSetting("SASCsvStpName")));
            }            
            url.Append(Uri.EscapeUriString("&FILE_NAME="));
            url.Append(Uri.EscapeUriString(Path.GetFileNameWithoutExtension(filename)));
            url.Append(Uri.EscapeUriString("&FILE_EXT="));
            url.Append(Uri.EscapeUriString(Path.GetExtension(filename)));
            url.Append(Uri.EscapeUriString("&CATEGORY="));
            url.Append(Uri.EscapeUriString(category));

            Sentry.Common.Logging.Logger.Debug($"URL: {url.ToString()}");

            HttpWebRequest httpRequest = WebRequest.Create(url.ToString()) as HttpWebRequest;

            httpRequest.CookieContainer = cookies;
            httpRequest.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            HttpWebResponse httpResponse = httpRequest.GetResponse() as HttpWebResponse;
            string responsecontent = string.Empty;

            using (var stream = httpResponse.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                responsecontent = reader.ReadToEnd();
                if (!(String.IsNullOrEmpty(responsecontent)))
                {
                    throw new WebException("SAS Error", new Exception(responsecontent));
                }
            }
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
            fn = "_" + fn;

            fn = fn + Path.GetExtension(filename);

            return fn;
        }
    }
}
