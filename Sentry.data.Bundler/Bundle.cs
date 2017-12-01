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

namespace Sentry.data.Bundler
{
    class Bundle
    {
        private string _requestFilePath;
        private static BundleRequest _request = null;
        private S3ServiceProvider _s3Service;
        //private static Amazon.S3.IAmazonS3 _s3client = null;

        public Bundle(string requestFilePath)
        {
            _requestFilePath = requestFilePath;
            _s3Service = new S3ServiceProvider();
        }


        private class BundlePart
        {
            public int Id { get; set; }
            public long ContentLenght { get; set; }
            public string VersionId { get; set; }
            public string Key { get; set; }
        }


        public void KeyContatenation()
        {
            Console.WriteLine("Reading incoming bundle request file");
            Logger.Info("Reading incoming bundle request file");

            string incomingRequest = System.IO.File.ReadAllText(this._requestFilePath);

            //Get request from DatasetBundler location
            _request = JsonConvert.DeserializeObject<BundleRequest>(incomingRequest);

            Console.WriteLine($"Loaded request Guid: {_request.RequestGuid}");
            Logger.Info($"Loaded request Guid: {_request.RequestGuid}");

            DateTime bundleStart = DateTime.Now;
            List<BundlePart> parts_list = Collect_Parts(_request.SourceKeys);
            Logger.Debug($"Found {parts_list.Count()} keys to concatenate");
            List<List<BundlePart>> grouped_parts_list = Chunk_By_Size(parts_list);
            Sentry.Common.Logging.Logger.Debug($"Created {grouped_parts_list.Count()} concatenation groups");
            for (int i = 0; i < grouped_parts_list.Count(); i++)
            {
                Sentry.Common.Logging.Logger.Debug($"Concatenating group {i}/{grouped_parts_list.Count()}");
                RunSingleContatenation(grouped_parts_list[i], $"{_request.TargetFileName + _request.FileExtension}");
            }

            Logger.Info($"Bundle Request Processed - Request:{_request.RequestGuid} Parts:{_request.SourceKeys.Count()} TotalTime(sec):{(DateTime.Now - bundleStart).TotalSeconds}");

            //Logger.Info("Removing processed request file");
            //System.IO.File.Delete(this._requestFilePath);
        }

        private void RunSingleContatenation(List<BundlePart> parts_list, string result_filepath)
        {
            string targetfile = AssembleParts(result_filepath, parts_list);            
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

            bool remainerProcessed = false;

            //Split original part list in half
            //If odd number second half will have remainer
            firstHalf = parts_list.Take(partcnt/2).ToList();
            secondHalf = parts_list.Skip(partcnt / 2).ToList();
            object b = new Semaphore(1,10);
            
            //Merge first and second half
            Parallel.ForEach(firstHalf, new ParallelOptions { }, (part) =>
            {
                Console.WriteLine($"Started merge into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                Logger.Info($"Started merge into C:\\tmp\\DatasetBundlerWork\\{ part.Id}");
                //merge firstHalf with Id of (partcnt/2+i) within secondHalf
                //utilize firsthalf partID as target file, this will force unique file names
                using (FileStream fs = new FileStream($"C:\\tmp\\DatasetBundlerWork\\{part.Id}", FileMode.OpenOrCreate))
                {
                    Console.WriteLine($"Mering first part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                    Logger.Info($"Mering first part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
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


                    Console.WriteLine($"Mering second part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                    Logger.Info($"Mering {secondId} part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                    using (Stream resp = _s3Service.GetObject(secondHalf[secondId].Key, secondHalf[secondId].VersionId))
                    {
                        resp.CopyTo(fs);
                        fs.Flush();

                        resp.Dispose();
                    }


                    //If part count is odd, then merge last part of secondhalf into first part of firsthalf
                    //This should only hit once if part count is odd
                    if (!remainerProcessed && partcnt % 2.0 != 0)
                    {
                        Console.WriteLine($"Mering remainer part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                        Logger.Info($"Mering remainer part into into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                        using (Stream resp = _s3Service.GetObject(secondHalf.Last().Key, secondHalf.Last().VersionId))
                        {
                            resp.CopyTo(fs);
                            fs.Flush();

                            resp.Dispose();
                        }
                        remainerProcessed = true;
                    }

                    fs.Dispose();

                    Console.WriteLine($"Completed merge into C:\\tmp\\DatasetBundlerWork\\{part.Id}");
                }
                
            });

            //Rebuild 

            //Then merge file at first Half ID with object 

            //Loop each half

            BundlePart finalFile = AssembleLocalFiles(firstHalf);

            //firstHalf = firstHalf.Take(partcnt / 2).ToList();
            //secondHalf = firstHalf.Skip(partcnt / 2).ToList();
            

            return $"C:\\tmp\\DatasetBundlerWork\\{finalFile.Id}";
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
                using (FileStream fs = new FileStream($"C:\\tmp\\DatasetBundlerWork\\{part.Id}", FileMode.Append, FileAccess.Write))
                {
                    Logger.Info($"Merging {secondHalf[secondId].Id} into {part.Id}");

                    //stream firsthalf part
                    using (var secFile = File.OpenRead($"C:\\tmp\\DatasetBundlerWork\\{secondHalf[secondId].Id}"))
                    //new FileStream($"C:\\tmp\\DatasetBundlerWork\\{secondHalf[iter].Id}", FileMode.Open))
                    {
                        secFile.CopyTo(fs);
                        fs.Flush();

                        secFile.Dispose();
                    }

                    Logger.Info($"Deleting {secondHalf[secondId].Id} file");
                    System.IO.File.Delete($"C:\\tmp\\DatasetBundlerWork\\{secondHalf[secondId].Id}");


                    //If part count is odd, then merge last part of secondhalf into first part of firsthalf
                    //This should only hit once if part count is odd
                    if (!remainerProcessed && partcnt % 2.0 != 0)
                    {
                        Logger.Info($"Merging remainer {secondHalf.Last().Id} into {part.Id}");

                        using (var remainerFile = File.OpenRead($"C:\\tmp\\DatasetBundlerWork\\{secondHalf.Last().Id}"))
                        {
                            remainerFile.CopyTo(fs);
                            fs.Flush();

                            remainerFile.Dispose();
                        }

                        Logger.Info($"Deleteing {secondHalf.Last().Id}");
                        System.IO.File.Delete($"C:\\tmp\\DatasetBundlerWork\\{secondHalf.Last().Id}");

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
            int partid = 0;
            List<BundlePart> object_list = new List<BundlePart>();

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

            return object_list;
        }

        //private static string GetObject(string key, string versionId)
        //{
        //    GetObjectRequest req = new GetObjectRequest();
        //    string contents = null;

        //    req.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
        //    req.Key = key;
        //    req.VersionId = versionId;

        //    using (GetObjectResponse response = S3Client.GetObject(req))
        //    {
        //        using (StreamReader reader = new StreamReader(response.ResponseStream))
        //        {
        //            contents = reader.ReadToEnd();
        //        }
        //    }

        //    return contents;
        //}


    }
}
