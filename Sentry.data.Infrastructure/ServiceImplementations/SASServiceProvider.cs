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

        public event EventHandler<TransferProgressEventArgs> OnPushToProgressEvent;

        public void ConvertToSASFormat(string filename, string category)
        {
            try
            {
                OnPushToProgress(new TransferProgressEventArgs(filename, 0, "Converting"));

                StringBuilder url = GernerateSASURL(filename, category);

                OnPushToProgress(new TransferProgressEventArgs(filename, 50, "Converting"));

                //JCG TODO: Revisit after SAS fixes issue around initial logon attempt fails, additional attempts succeed.
                Retry.Do(() => CallSASConvertSTP(url), TimeSpan.FromSeconds(15), 2);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                OnPushToProgress(new TransferProgressEventArgs(filename, 100, "Converting"));
            }

        }

        private void CallSASConvertSTP(StringBuilder url)
        {
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
                    throw new WebException("Error Executing SAS Conversion", new Exception(responsecontent));
                }
            }
        }

        private static StringBuilder GernerateSASURL(string filename, string category)
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
            url.Append(Uri.EscapeUriString("&FILE_NAME="));
            url.Append(Uri.EscapeUriString(Path.GetFileNameWithoutExtension(filename)));
            url.Append(Uri.EscapeUriString("&FILE_EXT="));
            url.Append(Uri.EscapeUriString(Path.GetExtension(filename)));
            url.Append(Uri.EscapeUriString("&CATEGORY="));
            url.Append(Uri.EscapeUriString(category));

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


        protected virtual void OnPushToProgress(TransferProgressEventArgs e)
        {
            EventHandler<TransferProgressEventArgs> handler = OnPushToProgressEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }


    }
}
