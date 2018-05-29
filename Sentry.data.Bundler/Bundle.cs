﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using System.Threading;
using System.IO;
using Sentry.data.Infrastructure;
using Sentry.data.Common;

namespace Sentry.data.Bundler
{
    class Bundle
    {
        private BundleRequest _request = null;
        private IDatasetService _s3Service;
        private string _baseWorkingDir;
        private IDatasetContext _dscontext;
        private Event e;

        public Bundle(IDatasetContext dscontext, IDatasetService dsService)
        {
            //_s3Service = new S3ServiceProvider();
            _baseWorkingDir = Configuration.Config.GetHostSetting("BundleBaseWorkDirectory");
            _dscontext = dscontext;
            _s3Service = dsService;
        }

        public string RequestFilePath { get; set; }

        private class BundlePart
        {
            public int Id { get; set; }
            public long ContentLenght { get; set; }
            public string VersionId { get; set; }
            public string Key { get; set; }
        }


        public async void KeyContatenation()
        {
            DateTime bundleStart = DateTime.MinValue;

            try
            {
                Console.WriteLine("Reading incoming bundle request file");
                Logger.Info("Reading incoming bundle request file");

                string incomingRequest = System.IO.File.ReadAllText(this.RequestFilePath);
                string bundledFile = null;

                //Get request from DatasetBundler location
                _request = JsonConvert.DeserializeObject<BundleRequest>(incomingRequest);

                Console.WriteLine($"Loaded request Guid: {_request.RequestGuid}");
                Logger.Info($"Loaded request Guid: {_request.RequestGuid}");



                bundleStart = DateTime.Now;

                //Create Bundle Started Event
                e = new Event();
                e.EventType = _dscontext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                e.Status = _dscontext.EventStatus.Where(w => w.Description == "In Progress").FirstOrDefault();
                e.TimeCreated = bundleStart;
                e.TimeNotified = bundleStart;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = _request.RequestInitiatorId;
                e.Dataset = _request.DatasetID;
                e.DataConfig = _request.DatasetFileConfigId;
                e.Reason = $"Started bundle request for {_request.TargetFileName}";
                e.Parent_Event = _request.RequestGuid;
                await Utilities.CreateEventAsync(e);

                Logger.Info("Start Event Created.");

                List<BundlePart> parts_list = Collect_Parts(_request.SourceKeys);
                Logger.Debug($"Found {parts_list.Count} keys to concatenate");
                List<List<BundlePart>> grouped_parts_list = Chunk_By_Size(parts_list);
                Sentry.Common.Logging.Logger.Debug($"Created {grouped_parts_list.Count} concatenation groups");
                Sentry.Common.Logging.Logger.Debug($"Starting Concatenation process... RequestGuid:{_request.RequestGuid}");
                for (int i = 0; i < grouped_parts_list.Count; i++)
                {
                    Sentry.Common.Logging.Logger.Debug($"Concatenating group {i}/{grouped_parts_list.Count}");
                    bundledFile = RunSingleContatenation(grouped_parts_list[i], $"{_request.TargetFileName + _request.FileExtension}");
                }
                Sentry.Common.Logging.Logger.Info($"Finished Concatenation process. RequestGuid:{_request.RequestGuid}");

                //Push bundled file to s3 location
                string versionId = null;
                try
                {
                    versionId = _s3Service.UploadDataFile(bundledFile, (_request.TargetFileLocation + $"{_request.TargetFileName}{_request.FileExtension}"));
                }
                catch (Exception ex)
                {
                    Logger.Error($"Bundle file upload failed - Request:{_request.RequestGuid} Source:{bundledFile}) Destination:{_request.TargetFileLocation}", ex);
                    throw;
                }

                try
                {
                    //Remove working directory for this request
                    //Passing true to recursively delete all sub-directories\files within the working directory
                    Logger.Info($"Removing processed work file: {Directory.GetParent(bundledFile).ToString()}");
                    Directory.Delete(Directory.GetParent(bundledFile).ToString(), true);
                    Logger.Info($"Successfully removed work file");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to remove work file: {Directory.GetParent(bundledFile).ToString()}", ex);
                }

                Logger.Debug("Creating LoaderRequest...");
                //Create BundleResponse
                //Create Dataset Loader Request to register Bundle with Data.Sentry.Com
                LoaderRequest req = new LoaderRequest(Guid.Parse(_request.RequestGuid));
                req.IsBundled = true;
                req.DatasetID = _request.DatasetID;
                req.DatasetFileConfigId = _request.DatasetFileConfigId;
                req.TargetBucket = _request.Bucket;
                req.TargetFileName = $"{_request.TargetFileName}{_request.FileExtension}";
                req.TargetKey = (_request.TargetFileLocation + $"{req.TargetFileName}");
                req.TargetVersionId = versionId;
                req.RequestInitiatorId = _request.RequestInitiatorId;
                req.EventID = "";

                Logger.Debug("Successfully Created LoaderRequest, continuing on....");

                Logger.Debug("Serializing LoaderRequest");
                //Push BundleResponse to dataset bundle droploaction
                string jsonResponse = JsonConvert.SerializeObject(req, Formatting.Indented);
                Logger.Debug("Successfully Serialized LoaderRequest, continuing on....");

                Logger.Debug($"BundleResponse: {jsonResponse}");

                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        StreamWriter writer = new StreamWriter(ms);

                        writer.WriteLine(jsonResponse);
                        writer.Flush();

                        //You have to rewind the MemoryStream before copying
                        ms.Seek(0, SeekOrigin.Begin);

                        Logger.Debug($"Streaming LoaderRequest to {Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}");

                        using (FileStream fs = new FileStream($"{Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}{_request.RequestGuid}.json", FileMode.OpenOrCreate))
                        {
                            ms.CopyTo(fs);
                            fs.Flush();
                        }

                        Logger.Debug("Successfully Streamed LoaderRequest, continuing on....");
                    }

                    Logger.Debug("Creating bundle file process event");
                    //Create Bundle Started Event
                    e = new Event();
                    e.EventType = _dscontext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                    e.Status = _dscontext.EventStatus.Where(w => w.Description == "In Progress").FirstOrDefault();
                    e.TimeCreated = DateTime.Now;
                    e.TimeNotified = DateTime.Now;
                    e.IsProcessed = false;
                    e.UserWhoStartedEvent = _request.RequestInitiatorId;
                    e.Dataset = _request.DatasetID;
                    e.DataConfig = _request.DatasetFileConfigId;
                    e.Reason = $"Submitted request to dataset loader to upload and register {_request.TargetFileName}";
                    e.Parent_Event = _request.RequestGuid;
                    Logger.Debug("Successfully created bundle file process event, continuing on...");

                    Logger.Debug("Sending bundle file process event");
                    await Utilities.CreateEventAsync(e);
                    Logger.Debug("Successfully sent bundle file process event, continuing on...");

                    Logger.Info($"Bundle Request Processed - Request:{_request.RequestGuid} Parts:{_request.SourceKeys.Count} TotalTime(sec):{(DateTime.Now - bundleStart).TotalSeconds}");
                    
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to write BundleResponse.  Manually write the following to {Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}{_request.RequestGuid}.json. BundleResponse:{jsonResponse}", ex);
                }
                finally
                {
                    //remove reqeust file
                    Logger.Info("Removing processed request file");
                    try
                    {
                        System.IO.File.Delete(this.RequestFilePath);
                        Logger.Info("Successfully removed processed request file");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed removing bundle request file.  Manually delete the following file: {this.RequestFilePath}", ex);
                    }                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Bundle Request Failed", ex);

                //Create Bundle Failed Event
                e = new Event();
                e.EventType = _dscontext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                e.Status = _dscontext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = _request.RequestInitiatorId;
                e.Dataset = _request.DatasetID;
                e.DataConfig = _request.DatasetFileConfigId;
                e.Reason = $"Bundle Request Failed for {_request.TargetFileName}";
                e.Parent_Event = _request.RequestGuid;
                await Utilities.CreateEventAsync(e);
                //Logger.Info($"Failed Event Created: {e.ToString()}");
            }
        }

        private string RunSingleContatenation(List<BundlePart> parts_list, string result_filepath)
        {
            return AssembleParts(result_filepath, parts_list);            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result_filepath"></param>
        /// <param name="parts_list"></param>
        /// <returns></returns>
        private string AssembleParts(string result_filepath, List<BundlePart> parts_list)
        {
            int partcnt = parts_list.Count();

            List<BundlePart> firstHalf;
            List<BundlePart> secondHalf;
            BundlePart finalFile;

            bool remainerProcessed = false;

            //Split original part list in half
            //If odd number second half will have remainer
            firstHalf = parts_list.OrderBy(o => o.Id).Take(partcnt/2).ToList();
            secondHalf = parts_list.OrderBy(o => o.Id).Skip(partcnt / 2).ToList();
            //object b = new Semaphore(1,10)

            try
            {
                //Create working directory
                if (!Directory.Exists($"{_baseWorkingDir + _request.RequestGuid}\\"))
                {
                    Directory.CreateDirectory($"{_baseWorkingDir + _request.RequestGuid}\\");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create working dir - RequestGuid:{_request.RequestGuid} Dir:{_baseWorkingDir + _request.RequestGuid}");
                throw;
            }
            
            
            //Merge first and second half
            Parallel.ForEach(firstHalf, new ParallelOptions { }, (part) =>
            {
                try
                {
                    Logger.Debug($"PartId:{part.Id}");
                    Logger.Debug($"firstHalf count:{firstHalf.Count}");
                    Console.WriteLine($"Started merge into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                    Logger.Info($"Started merge into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                    //merge firstHalf with Id of (partcnt/2+i) within secondHalf
                    //utilize firsthalf partID as target file, this will force unique file names
                    using (FileStream fs = new FileStream($"{_baseWorkingDir + _request.RequestGuid}\\{part.Id}", FileMode.OpenOrCreate))
                    {
                        Console.WriteLine($"Creating base part {part.Id} from {part.Key}:{part.VersionId} into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                        Logger.Info($"Creating base part {part.Id} from {part.Key}:{part.VersionId} into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                        //stream firsthalf part
                        using (Stream resp = _s3Service.GetObject(part.Key, part.VersionId))
                        {
                            resp.CopyTo(fs);
                            fs.Flush();

                            resp.Dispose();
                        }

                        //determine secondhalf part index
                        //int secondId = ((partcnt / 2) - 2) + part.Id;
                        int secondId = part.Id;
                        Logger.Debug($"secondId:{secondId}");
                        Logger.Debug($"SecondHalf count:{secondHalf.Count}");
                        //BundlePart secondPart = secondHalf.Where(x => x.Id == secondId).First();
                        //stream second half part into target file


                        Console.WriteLine($"Mering part {secondId} ({secondHalf[secondId].Key}:{secondHalf[secondId].VersionId}) into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                        Logger.Info($"Mering part {secondId} ({secondHalf[secondId].Key}:{secondHalf[secondId].VersionId}) into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                        using (Stream resp = _s3Service.GetObject(secondHalf[secondId].Key, secondHalf[secondId].VersionId))
                        {
                            resp.CopyTo(fs);
                            fs.Flush();

                            resp.Dispose();
                        }

                        Logger.Debug("Checking for remainer part...");
                        //If part count is odd, then merge last part of secondhalf into first part of firsthalf
                        //This should only hit once if part count is odd
                        if (part.Id == 0 && !remainerProcessed && partcnt % 2.0 > 0)
                        {
                            Console.WriteLine($"Mering remainer part {secondHalf.Last().Id} into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                            Logger.Info("Detected odd number of parts, merging remainer into base part");
                            Logger.Info($"Mering remainer part {secondHalf.Last().Id} into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                            using (Stream resp = _s3Service.GetObject(secondHalf.Last().Key, secondHalf.Last().VersionId))
                            {
                                resp.CopyTo(fs);
                                fs.Flush();

                                resp.Dispose();
                            }
                            remainerProcessed = true;
                        }

                        Logger.Debug("Disposing File Stream");
                        fs.Dispose();

                        Console.WriteLine($"Completed merge into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed Concatenation process... RequestGuid:{_request.RequestGuid}", ex);
                    throw;
                }


            });

            //Rebuild 

            //Then merge file at first Half ID with object 

            //Loop each half
            if (firstHalf.Count() > 1)
            {
                finalFile = AssembleLocalFiles(firstHalf);
            }
            else
            {
                finalFile = firstHalf.First();
            }


            //firstHalf = firstHalf.Take(partcnt / 2).ToList();
            //secondHalf = firstHalf.Skip(partcnt / 2).ToList();


            return $"{_baseWorkingDir + _request.RequestGuid}\\{finalFile.Id}";
        }


        private BundlePart AssembleLocalFiles(List<BundlePart> list)
        {
            int partcnt = list.Count();
            bool remainerProcessed = false;
            List<BundlePart> firstHalf = list.OrderBy(o => o.Id).Take(partcnt / 2).ToList();
            List<BundlePart> secondHalf = list.OrderBy(o => o.Id).Skip(partcnt / 2).ToList();
            Parallel.ForEach(firstHalf, (part) =>
            {
                //int secondId = ((partcnt / 2) - 2) + part.Id;
                int secondId = part.Id;
                //merge firstHalf with Id of (partcnt/2+i) within secondHalf
                //utilize firsthalf partID as target file, this will force unique file names
                using (FileStream fs = new FileStream($"{_baseWorkingDir + _request.RequestGuid}\\{part.Id}", FileMode.Append, FileAccess.Write))
                {
                    Logger.Info($"Merging {secondHalf[secondId].Id} into {part.Id}");

                    //stream firsthalf part
                    using (var secFile = File.OpenRead($"{_baseWorkingDir + _request.RequestGuid}\\{secondHalf[secondId].Id}"))
                    //new FileStream($"C:\\tmp\\DatasetBundlerWork\\{secondHalf[iter].Id}", FileMode.Open))
                    {
                        secFile.CopyTo(fs);
                        fs.Flush();

                        secFile.Dispose();
                    }

                    Logger.Info($"Deleting {secondHalf[secondId].Id} file");
                    System.IO.File.Delete($"{_baseWorkingDir + _request.RequestGuid}\\{secondHalf[secondId].Id}");


                    //If part count is odd, then merge last part of secondhalf into first part of firsthalf
                    //This should only hit once if part count is odd
                    if (part.Id == 0 && !remainerProcessed && partcnt % 2.0 != 0)
                    {
                        Logger.Info($"Merging remainer {secondHalf.Last().Id} into {part.Id}");

                        using (var remainerFile = File.OpenRead($"{_baseWorkingDir + _request.RequestGuid}\\{secondHalf.Last().Id}"))
                        {
                            remainerFile.CopyTo(fs);
                            fs.Flush();

                            remainerFile.Dispose();
                        }

                        Logger.Info($"Deleteing {secondHalf.Last().Id}");
                        System.IO.File.Delete($"{_baseWorkingDir + _request.RequestGuid}\\{secondHalf.Last().Id}");

                        remainerProcessed = true;
                    }

                    fs.Dispose();
                }
            });

            if (firstHalf.Count > 1)
            {
                return AssembleLocalFiles(firstHalf);
            }
            else
            {
                return firstHalf.First();
            }
        }

        /// <summary>
        /// Groups parts based on max file size
        /// </summary>
        /// <param name="parts_list"></param>
        /// <param name="max_filesize"></param>
        /// <returns></returns>
        private List<List<BundlePart>> Chunk_By_Size(List<BundlePart> parts_list, long max_filesize)
        {
            //Can be used to split target file into multiple based on max_file size
            // Currently commented out logic so method will produce only one group of
            // all source keys regardless of target file size.
            List<List<BundlePart>> grouped_list = new List<List<BundlePart>>();
            List<BundlePart> current_list = new List<BundlePart>();
            long current_size = 0;

            foreach (BundlePart p in parts_list)
            {
                current_size += p.ContentLenght;
                current_list.Add(p);

                //Uncomment these lines if splitting logic is needed
                //if (current_size > max_filesize)
                //{
                //    grouped_list.Add(current_list);
                //    current_list = new List<BundlePart>();
                //    current_size = 0;
                //}
            }

            // remove this line if splitting logic is needed
            grouped_list.Add(current_list);

            //If max size is larger than all file combined
            if (grouped_list.Count() == 0 && current_list.Count() > 0)
            {
                grouped_list.Add(current_list);
            }

            return grouped_list;
        }

        /// <summary>
        /// Creates single group of all parts
        /// </summary>
        /// <param name="parts_list"></param>
        /// <returns></returns>
        private List<List<BundlePart>> Chunk_By_Size(List<BundlePart> parts_list)
        {
            // Will produce only one group of all source keys regardless of target file size.
            List<List<BundlePart>> grouped_list = new List<List<BundlePart>>();
            List<BundlePart> current_list = new List<BundlePart>();
            long current_size = 0;

            //Order by the largest first
            foreach (BundlePart p in parts_list.OrderByDescending(x => x.ContentLenght))
            {
                current_size += p.ContentLenght;
                current_list.Add(p);

            }

            grouped_list.Add(current_list);

            if (grouped_list.Count() == 0 && current_list.Count() > 0)
            {
                grouped_list.Add(current_list);
            }

            return grouped_list;
        }

        /// <summary>
        /// Returns list of s3 object keys with ContentLength metadata
        /// </summary>
        /// <param name="sourceKeys"></param>
        /// <returns></returns>
        private List<BundlePart> Collect_Parts(List<Tuple<string, string>> sourceKeys)
        {
            Logger.Debug($"Collecting Parts Metadata Started... RequestGuid:{_request.RequestGuid}");
            int partid = 0;
            List<BundlePart> object_list = new List<BundlePart>();

            try
            {
                foreach (var item in sourceKeys)
                {
                    BundlePart obj = new BundlePart();
                    obj.Id = partid;
                    obj.Key = item.Item1;
                    obj.VersionId = item.Item2;

                    //Get object metadata, without retrieving whole object, specifically for the size of the object
                    Dictionary<string, string> resp = _s3Service.GetObjectMetadata(obj.Key, obj.VersionId);

                    obj.ContentLenght = Convert.ToInt64(resp["ContentLength"]);

                    object_list.Add(obj);

                    partid++;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Failed to Collect parts", ex);
            }

            Logger.Debug($"Collecting Parts Metadata Finished... RequestGuid:{_request.RequestGuid}");

            return object_list;
        }
    }
}
