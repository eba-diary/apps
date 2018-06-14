using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System.IO;
using Newtonsoft.Json;
using StructureMap;
using Amazon.S3;
using System.Net.Mail;
using Sentry.data.DatasetLoader.Entities;
using System.Text.RegularExpressions;
using System.Security.Principal;
using Sentry.data.Web.Helpers;
using Sentry.data.Common;
using System.Web;
using System.Threading.Tasks;
using System.Threading;

namespace Sentry.data.DatasetLoader
{
    public class Core
    {

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public async void OnStart(string[] args)
        {
            Logger.Debug("DatasetLoader Starting...");

            //setup a token to allow a task to be cancelled
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            await Task.Factory.StartNew(() => this.DoWork(args), TaskCreationOptions.LongRunning).ContinueWith(TaskException, TaskContinuationOptions.OnlyOnFaulted);

        }

        private async Task DoWork(string[] args)
        {
            Logger.Info("DatasetLoader task started.");
            string _file = null;
            string _path = null;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-f":
                            i++;
                            if (args.Length <= i) throw new ArgumentException(args[i]);
                            _file = args[i];
                            break;

                        case "-p":
                            i++;
                            if (args.Length <= i) throw new ArgumentException(args[i]);
                            _path = args[i];
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (ArgumentException e)
            {
                Logger.Error("Incorrect Parameter", e);
                //return (int)ExitCodes.Failure;
            }

            string SystemDir = null;
            string SystemName = null;
            string path = null;
            bool isBundled = false;

            path = _path.Replace(@"d:\share", Config.GetHostSetting("FileShare"));

            Logger.Info($"Processing: {path}");

            try
            {
                //determine if the incoming files is a bundled file response object
                if (Directory.GetParent(path).Name == "bundle")
                {
                    Logger.Info("Setting Variables for Bundle Request");
                    SystemName = Directory.GetParent(Directory.GetParent(path).ToString()).Name;
                    SystemDir = Directory.GetParent(Directory.GetParent(path).ToString()).FullName + @"\";
                    isBundled = true;
                }
                else
                {
                    Logger.Info("Setting Variables for Request");
                    SystemDir = Directory.GetParent(path).FullName + @"\";
                    SystemName = Directory.GetParent(path).Name;
                }

                Logger.Info("Starting Logger Task");
                await Loader.Run(SystemDir, SystemName, path, isBundled);

            }
            catch (Exception e)
            {
                Logger.Error("File Access Error", e);
            }            

        }

        internal void OnStop()
        {
            Logger.Info("Windows Service stopping...");
            //request the task to cancel itself
            _tokenSource.Cancel();
        }

        private void TaskException(Task t)
        {
            Logger.Fatal("Exception occurred on main Windows Service Task. Stopping Service immediately.", t.Exception);
            Environment.Exit(10001);
        }

        private static void SingleFileProcessor(List<DatasetFileConfig> systemMetaFiles, string _path, IDatasetService upload, IDatasetContext dscontext)
        {
            throw new System.NotImplementedException();

            //int configMatch = 0;
            ////Pick correct meta file for processing
            ////foreach (SystemConfig sc in systemMetaFiles)
            ////{

            //List<DatasetFileConfig> fcList = Utilities.GetMatchingDatasetFileConfigs(systemMetaFiles, _path);

            //FileInfo fi = new FileInfo(_path);

            //foreach (DatasetFileConfig fc in fcList.Where(w => w.IsGeneric == false))
            //{

            //    Logger.Debug($"Found non-generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");

            //    Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);

            //    DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);

            //    Utilities.RemoveProcessedFile(df, new FileInfo(_path));

            //    configMatch++;

            //    break;
            //}

            //if (configMatch == 0)
            //{
            //    DatasetFileConfig fc = systemMetaFiles.Where(w => w.IsGeneric == true).FirstOrDefault();
            //    Logger.Debug($"Using generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");
            //    Logger.Debug($"Retrieving Dataset associated with DatasetFileConfig: ID-{fc.ParentDataset.DatasetId}");
            //    Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);
            //    Logger.Debug("Processing DatasetFile");
            //    DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);

            //    Logger.Debug("Removing successful processed file");
            //    Utilities.RemoveProcessedFile(df, new FileInfo(_path));

            //    //ProcessGeneralFile(upload, dscontext, new FileInfo(_path));
            //    //StringBuilder message = new StringBuilder();
            //    //message.AppendLine("Configuration Not Defined for File");
            //    //message.AppendLine($"Path: {Path.GetFullPath(_path)}");

            //    //Logger.Error(message.ToString());

            //    //SendNotification(null, (int)ExitCodes.Failure, 0, message.ToString(), Path.GetFileName(_path));
            //}

        }

        private static string GetTargetFileName(DatasetFileConfig dfc, FileInfo fileInfo)
        {
            string outFileName = null;
            // Are we overwritting target file
            if (dfc.OverwriteDatafile)
            {
                // Non-Regex and TargetFileName is null
                // Use SearchCriteria value
                if (!(dfc.IsRegexSearch) && String.IsNullOrWhiteSpace(dfc.TargetFileName))
                {
                    outFileName = dfc.SearchCriteria;
                }
                // Non-Regex and TargetFileName has value
                // Use TargetFileName value
                else if (!(dfc.IsRegexSearch) && !(String.IsNullOrWhiteSpace(dfc.TargetFileName)))
                {
                    outFileName = dfc.TargetFileName;
                }
                // Regex and TargetFileName has value
                // Use TargetFileName value
                else if (dfc.IsRegexSearch && !(String.IsNullOrWhiteSpace(dfc.TargetFileName)))
                {
                    outFileName = dfc.TargetFileName;
                }
                // Regex and TargetFileName is null - Use input file name
                else if (dfc.IsRegexSearch && String.IsNullOrWhiteSpace(dfc.TargetFileName))
                {
                    outFileName = fileInfo.Name;
                }
            }

            return outFileName;
        }

        private static string GetFileOwner(FileInfo fileInfo)
        {
            var fs = File.GetAccessControl(fileInfo.FullName);
            var sid = fs.GetOwner(typeof(SecurityIdentifier));
            var ntAccount = sid.Translate(typeof(NTAccount));

            //remove domain
            var outowner = ntAccount.ToString().Replace(@"SHOESD01\", "");

            return outowner;
        }

        private static void RemoveProcessedFile(Entities.DatasetMetadata datasetMetadata, FileInfo fi)
        {
            try
            {
                File.Delete(fi.FullName);
            }
            catch (Exception e)
            {
                //Allow application to continue without error.  Log message for BI Portal SOS group.
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Failed to delete input file:");
                builder.AppendLine($"DatasetName: {datasetMetadata.datasetName}");
                builder.AppendLine($"File Location: {fi.FullName}");

                Logger.Error(builder.ToString());
            }

        }

        private static void RemoveProcessedFile(DatasetFile df, FileInfo fi)
        {
            try
            {
                File.Delete(fi.FullName);
            }
            catch (Exception e)
            {
                //Allow application to continue without error.  Log message for BI Portal SOS group.
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Failed to delete input file:");
                builder.AppendLine($"DatasetFile_ID: {df.DatasetFileId}");
                builder.AppendLine($"File_NME: {df.FileName}");
                builder.AppendLine($"File Location: {fi.FullName}");

                Logger.Error(builder.ToString());
            }

        }

        private static Dataset RegisterWithDatasetMgmt(Dataset ds, Entities.DatasetMetadata datasetMetadata, IDatasetContext dscontext, FileInfo fi)
        {
            try
            {
                //Dataset ds = CreateDataset(datasetMetadata, fi);

                //DatasetFile df = CreateDatasetFile();



                if (datasetMetadata.overwrite == true)
                {
                    if (dscontext.s3KeyDuplicate(ds.S3Key) == true)
                    {
                        ds = dscontext.GetByS3Key(ds.S3Key);
                        ds.ChangedDtm = DateTime.Now;
                    }
                }
                else
                {
                    if (dscontext.s3KeyDuplicate(ds.S3Key) == true)
                    {
                        throw new Exception("Config Overwrite property is set to FALSE and file already exists on S3");
                    }
                }

                //Write dataset to database
                dscontext.Merge(ds);
                dscontext.SaveChanges();

                return ds;

            }
            catch (Exception e)
            {

                StringBuilder builder = new StringBuilder();
                builder.Append("Failed to record metadata to Dataset Management.");
                builder.Append($"DatasetName: {datasetMetadata.datasetName}");
                builder.Append($"Category: {datasetMetadata.category}");
                builder.Append($"Description: {datasetMetadata.description}");
                builder.Append($"CreateUser: {datasetMetadata.createUser}");
                builder.Append($"Owner: {datasetMetadata.owner}");
                builder.Append($"Frequency: {datasetMetadata.frequency}");

                Logger.Error(builder.ToString(), e);

                SendNotification(datasetMetadata, (int)ExitCodes.DatabaseError, 0, "Error saving dataset to database", fi.Name);
                throw new Exception("Error saving dataset to database", e);
                //return (int)ExitCodes.DatabaseError;
            }

        }

        private static List<SystemConfig> LoadSystemConfigFiles(string metaDir)
        {
            string configDir = metaDir + @"\ConfigFile\";

            List<SystemConfig> systemMetaFiles = new List<SystemConfig>();

            string[] metaFiles = System.IO.Directory.GetFiles(configDir, "*.meta.json");

            Logger.Debug($"Discovered {metaFiles.Count()} System Configuration files within {configDir}");

            foreach (string s in metaFiles)
            {
                DatasetLoader.Entities.DatasetMetadata metafile = null;
                string metacontents = null;
                System.IO.FileInfo fi = null;


                try
                {
                    fi = new System.IO.FileInfo(s);

                    metacontents = File.ReadAllText(fi.FullName);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    Logger.Error("Error Reading MetaFile.", e);
                    SendNotification(metafile, (int)ExitCodes.Failure, 0, "Error Reading MetaFile.", s);
                    //return (int)ExitCodes.Failure;
                }

                if (!(IsSystemConfigValid(metacontents)))
                {
                    Logger.Error($"Invalid metafile schema. Please correct metafile ({fi.FullName}) and resubmit");
                    SendNotification(metafile, (int)ExitCodes.InvalidJson, 0, $"Invalid metafile schema. Please correct metafile ({fi.Name}) and resubmit", s);
                    //return (int)ExitCodes.InvalidJson;
                };

                //SystemConfig a = null;
                //a = JsonConvert.DeserializeObject<SystemConfig>(metacontents);
                systemMetaFiles.Add(JsonConvert.DeserializeObject<SystemConfig>(metacontents));
            }

            return systemMetaFiles;
        }

        private static string FindMetaFile(string filepath)
        {
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string metafilename = Path.Combine(Path.GetDirectoryName(filepath).ToString() + (@"\" + filename + ".meta.json"));
            Logger.Debug("FileName: " + filename);
            Logger.Debug("MetaFileName: " + metafilename);

            return metafilename;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static Boolean IsJsonRequestValid(string json)
        {
            MetaFile result = null;

            try
            {
                result = JsonConvert.DeserializeObject<MetaFile>(json);
            }
            catch (Exception e)
            {
                Logger.Error("Error deserializing input json.", e);
                return false;
            }

            result = null;
            return true;
        }

        private static Boolean IsSystemConfigValid(string json)
        {
            SystemConfig result = null;

            try
            {
                result = JsonConvert.DeserializeObject<SystemConfig>(json);
            }
            catch (Exception e)
            {
                Logger.Error("Error deserializing input system configuration json.", e);
                return false;
            }

            result = null;
            return true;
        }

        private static string GenerateDatasetFilename(DatasetLoader.Entities.DatasetMetadata mf, FileInfo inputFileName)
        {
            //if (!(String.IsNullOrEmpty(mf.datasetName)))
            //{
            //    string result = mf.datasetName + Path.GetExtension(inputFileName.FullName);
            //    return result;
            //}
            //else
            //{
            //    return Path.GetFileName(inputFileName.FullName);
            //}

            string outFileName = inputFileName.Name;
            outFileName.Replace(inputFileName.Extension, "");

            StringBuilder outFile = new StringBuilder();



            if (!(String.IsNullOrEmpty(mf.datasetNamePrefix)))
            {
                outFile.Append($"{mf.datasetNamePrefix}_");
                //string px = mf.datasetNamePrefix;
                //if (px.Contains("$Date$"))
                //{
                //    outFileName = $"{DateTime.Now.ToString("yyyyMMdd")}_{outFileName}";
                //}
                //else if (px.Contains("$DateTime$"))
                //{
                //    outFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{outFileName}";
                //}
                //else
                //{
                //    outFileName = $"{px}_{outFileName}";
                //}
            }

            //Add Base File Name
            outFile.Append(outFileName.Replace(inputFileName.Extension, ""));

            if (!(String.IsNullOrEmpty(mf.datasetNameSufix)))
            {
                outFile.Append($"_{mf.datasetNameSufix}");
                //string sx = mf.datasetNamePrefix;
                //if (sx.Contains("$Date$"))
                //{
                //    outFileName = $"{outFileName}_{DateTime.Now.ToString("yyyyMMdd")}";
                //}
                //else if (sx.Contains("$DateTime$"))
                //{
                //    outFileName = $"{outFileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                //}
            }

            //Add File Extension
            outFile.Append(inputFileName.Extension);

            return outFile.ToString();

        }

        private static void SendNotification(DatasetLoader.Entities.DatasetMetadata metaFile, int exitCode, int datasetID, string errorMessage, string fileName)
        {


            try
            {
                SmtpClient mySmtpClient = new SmtpClient("mail.sentry.com");

                // set smtp-client with basicAuthentication
                //mySmtpClient.UseDefaultCredentials = false;
                //System.Net.NetworkCredential basicAuthenticationInfo = new System.Net.NetworkCredential("username", "password");
                //mySmtpClient.Credentials = basicAuthenticationInfo;

                // add from,to mailaddresses
                MailAddress from = new MailAddress(Configuration.Config.GetHostSetting("DatasetMgmtEmail"));
                MailMessage myMail = new System.Net.Mail.MailMessage();
                myMail.From = from;

                //if (exitCode == 0 && (metaFile.notificationOn == "Both" || metaFile.notificationOn == "Success"))
                //{

                myMail.Body += @"<p><b><font color=""red"">Do Not Reply To This Email, This Inbox Is Not Monitored</font></b></p>";

                // set subject and encoding
                if (exitCode == 0)
                {
                    myMail.Subject = $"Dataset Management Upload of {metaFile.datasetName} Completed Successfully";
                    if (Configuration.Config.GetHostSetting("EnvironmentName") == "Dev")
                    {
                        myMail.Body += $"The {metaFile.datasetName} was successfully registered with Dataset Management";
                        myMail.Body += $"</p><br><br>Click <a href=\"http://localhost:2457/Dataset/Detail/?id={datasetID} \">here</a> to view {metaFile.datasetName} within Dataset Management</p>";
                    }
                    else
                    {
                        myMail.Body += $"</p><br><br>Click <a href=\"http://data{Configuration.Config.GetHostSetting("EnvironmentName")}.sentry.com/Dataset/Detail/?id={datasetID} \">here</a> to view {metaFile.datasetName} within Dataset Management</p>";
                    }

                }
                else
                {
                    myMail.Subject = $"Dataset Management Upload Completed with Errors";

                    StringBuilder body = new StringBuilder();

                    body.Append($"<p>Attempted to load {fileName}.</p>");
                    body.Append($"<p>Error Message: <br> {errorMessage}</p><br><br>");
                    myMail.Body += body;
                }

                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                // set body-message and encoding
                //myMail.Body = "<b>Do Not Reply To This Email, This Inbox Is Not Monitored</b>";
                //myMail.Body += "<br>using <b>HTML</b>.";
                //myMail.BodyEncoding = System.Text.Encoding.UTF8;
                // text or html
                myMail.IsBodyHtml = true;

                if (exitCode == 0 && (metaFile.notificationOn == "Both" || metaFile.notificationOn == "Success"))
                {
                    if (metaFile.notificationEmail != null)
                    {
                        myMail.To.Add(metaFile.notificationEmail);
                        myMail.To.Add("BIPortalAdmin@Sentry.com");
                    }
                    else
                    {
                        myMail.To.Add("BIPortalAdmin@Sentry.com");
                    }

                    mySmtpClient.Send(myMail);
                }
                else
                {
                    if (metaFile != null && exitCode != 0 && (metaFile.notificationOn == "Both" || metaFile.notificationOn == "Failure"))
                    {
                        if (metaFile.notificationEmail != null)
                        {
                            myMail.To.Add(metaFile.notificationEmail);
                        }
                        else
                        {
                            myMail.To.Add("BIPortalAdmin@Sentry.com");
                        }

                        mySmtpClient.Send(myMail);
                    }
                    else
                    {
                        myMail.To.Add("BIPortalAdmin@Sentry.com");
                        mySmtpClient.Send(myMail);
                    }

                }




                //}

            }

            catch (SmtpException ex)
            {
                Logger.Error("SmtpException has occured", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Error Occurred", ex);
            }
        }
    }
}
