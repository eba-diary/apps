using System;
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
        private string _requestFilePath;
        private BundleRequest _request = null;
        private S3ServiceProvider _s3Service;
        private string _baseWorkingDir;
        private IDatasetContext _dscontext;
        //private static Amazon.S3.IAmazonS3 _s3client = null;

        public Bundle(string requestFilePath, IDatasetContext dscontext)
        {
            _requestFilePath = requestFilePath;
            _s3Service = new S3ServiceProvider();
            _baseWorkingDir = Configuration.Config.GetHostSetting("BundleBaseWorkDirectory");
            _dscontext = dscontext;
        }


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

                string incomingRequest = System.IO.File.ReadAllText(this._requestFilePath);
                string bundledFile = null;

                //Get request from DatasetBundler location
                _request = JsonConvert.DeserializeObject<BundleRequest>(incomingRequest);

                Console.WriteLine($"Loaded request Guid: {_request.RequestGuid}");
                Logger.Info($"Loaded request Guid: {_request.RequestGuid}");



                bundleStart = DateTime.Now;

                //Create Bundle Started Event
                Event e = new Event();
                e.EventType = _dscontext.GetEventType(3);
                e.Status = _dscontext.GetStatus(2);
                e.TimeCreated = bundleStart;
                e.TimeNotified = bundleStart;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = _request.RequestInitiatorId;
                e.Dataset = _request.DatasetID;
                e.DataConfig = _request.DatasetFileConfigId;
                e.Reason = $"{_request.RequestGuid} : Bundle Request Started";
                e.Parent_Event = _request.RequestGuid;
                await Utilities.CreateEventAsync(e);

                Logger.Info("Start Event Created.");

                List<BundlePart> parts_list = Collect_Parts(_request.SourceKeys);
                Logger.Debug($"Found {parts_list.Count()} keys to concatenate");
                List<List<BundlePart>> grouped_parts_list = Chunk_By_Size(parts_list);
                Sentry.Common.Logging.Logger.Debug($"Created {grouped_parts_list.Count()} concatenation groups");
                for (int i = 0; i < grouped_parts_list.Count(); i++)
                {
                    Sentry.Common.Logging.Logger.Debug($"Concatenating group {i}/{grouped_parts_list.Count()}");
                    bundledFile = RunSingleContatenation(grouped_parts_list[i], $"{_request.TargetFileName + _request.FileExtension}");
                }

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

                //Remove working directory for this request
                //Passing true to recursively delete all sub-directories\files within the working directory
                Logger.Info($"Removing processed work file: {Directory.GetParent(bundledFile).ToString()}");
                Directory.Delete(Directory.GetParent(bundledFile).ToString(), true);

                //Create BundleResponse
                BundleResponse resp = new BundleResponse();
                resp.RequestGuid = _request.RequestGuid;
                resp.DatasetID = _request.DatasetID;
                resp.DatasetFileConfigId = _request.DatasetFileConfigId;
                resp.TargetBucket = _request.Bucket;
                resp.TargetFileName = $"{_request.TargetFileName}{_request.FileExtension}";
                resp.TargetKey = (_request.TargetFileLocation + $"{resp.TargetFileName}");
                resp.TargetVersionId = versionId;
                resp.RequestInitiatorId = _request.RequestInitiatorId;
                resp.EventID = "";

                //Push BundleResponse to dataset bundle droploaction
                string jsonResponse = JsonConvert.SerializeObject(resp, Formatting.Indented);

                using (MemoryStream ms = new MemoryStream())
                {
                    StreamWriter writer = new StreamWriter(ms);

                    writer.WriteLine(jsonResponse);
                    writer.Flush();

                    //You have to rewind the MemoryStream before copying
                    ms.Seek(0, SeekOrigin.Begin);

                    using (FileStream fs = new FileStream($"{_request.DatasetDropLocation}{_request.RequestGuid}.json", FileMode.OpenOrCreate))
                    {
                        ms.CopyTo(fs);
                        fs.Flush();
                    }
                }

                Logger.Info($"Bundle Request Processed - Request:{_request.RequestGuid} Parts:{_request.SourceKeys.Count()} TotalTime(sec):{(DateTime.Now - bundleStart).TotalSeconds}");

                //remove reqeust file
                Logger.Info("Removing processed request file");
                System.IO.File.Delete(this._requestFilePath);

            }
            catch (Exception ex)
            {
                //Create Bundle Failed Event
                Event e = new Event();
                e.EventType = _dscontext.GetEventType(3);
                e.Status = _dscontext.GetStatus(4);
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = _request.RequestInitiatorId;
                e.Dataset = _request.DatasetID;
                e.DataConfig = _request.DatasetFileConfigId;
                e.Reason = $"{_request.RequestGuid} : Bundle Request Failed - See SEL Logs for further detail.";
                e.Parent_Event = _request.RequestGuid;
                await Utilities.CreateEventAsync(e);

                Logger.Info($"Failed Event Created: {e.ToString()}");


                Logger.Error($"Bundle Request Failed - Request:{_request.RequestGuid}", ex);
                throw new Exception($"Bundle Request Failed - Request:{_request.RequestGuid}", ex);
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
            firstHalf = parts_list.Take(partcnt/2).ToList();
            secondHalf = parts_list.Skip(partcnt / 2).ToList();
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
                Logger.Debug($"Filed to create {_baseWorkingDir + _request.RequestGuid}");
                Logger.Error($"Failed to create working dir - RequestGuid:{_request.RequestGuid}");
                throw;
            }
            
            
            //Merge first and second half
            Parallel.ForEach(firstHalf, new ParallelOptions { }, (part) =>
            {
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
                    //BundlePart secondPart = secondHalf.Where(x => x.Id == secondId).First();
                    //stream second half part into target file


                    Console.WriteLine($"Mering part {secondId} ({part.Key}:{part.VersionId}) into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                    Logger.Info($"Mering part {secondId} ({part.Key}:{part.VersionId}) into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
                    using (Stream resp = _s3Service.GetObject(secondHalf[secondId].Key, secondHalf[secondId].VersionId))
                    {
                        resp.CopyTo(fs);
                        fs.Flush();

                        resp.Dispose();
                    }


                    //If part count is odd, then merge last part of secondhalf into first part of firsthalf
                    //This should only hit once if part count is odd
                    if (part.Id == 0 && !remainerProcessed && partcnt % 2.0 != 0)
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

                    fs.Dispose();

                    Console.WriteLine($"Completed merge into {_baseWorkingDir + _request.RequestGuid}\\{part.Id}");
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
            List<BundlePart> firstHalf = list.Take(partcnt / 2).ToList();
            List<BundlePart> secondHalf = list.Skip(partcnt / 2).ToList();   
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
