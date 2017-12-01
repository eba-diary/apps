using Newtonsoft.Json;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace Sentry.data.Infrastructure
{
    public class DatafileBundleProvider
    {
        ////private string pythonexe = @"C:\Users\072984\AppData\Local\Programs\Python36-32\python.exe";
        //private string _pythonexe = @"C:\TFS\Sentry.Data\Mainline\Sentry.data.Infrastructure\Service References\python-3.6.3-embed-amd64\python.exe";

        //private string _combinerApp = @"C:\Temp\PyCharm_Projects\dataset-management-lambda\CombineS3Files.py";
        //private string _keyBundlerApp = @"C:\Temp\PyCharm_Projects\dataset-management-lambda\KeyBundler.py";
        //private string _testfile = @"C:\Temp\PyCharm_Projects\dataset-management-lambda\Test.py";

        ////Parameters for script
        //private string _bucketName = @"sentry-dataset-management-np";
        //private string _folder = @"data-test/industry/fmcsa_census/mntly/";
        //private string _output = @"bundler/output/";
        //private string _bundleIntake = @"bundlework/intake/";
        //private string _bundleOutput = @"bundlework/output/";
        //private string _suffix = @".txt";
        //private string _fileSize = @"50000000000";

        //string targetKey = null;
        //string targetVersionId = null;
        //string targetETag = null;
        //private BundleRequest _request;

        //public DatafileBundleProvider()
        //{
        //    _request = new BundleRequest();
        //}

        //public void SubmitBundleRequest(List<DatasetFile> datasetFileList, string targetFileName, string email, IDatasetService s3Service)
        //{
        //    //Create 
        //    List<Tuple<string, string>> sourceKeys = new List<Tuple<string, string>>();

        //    string requestLocation = _bundleIntake + this._request.RequestGuid;
        //    string requestVersionId = null;


        //    this._request.DatasetID = datasetFileList.FirstOrDefault().Dataset.DatasetId;
        //    this._request.Bucket = Configuration.Config.GetHostSetting("AWSRootBucket");
        //    this._request.DatasetFileConfigId = datasetFileList.FirstOrDefault().DatasetFileConfig.ConfigId;
        //    this._request.TargetFileName = targetFileName;
        //    this._request.Email = email;

        //    //Due to the potental large number of files in list, upload SourceKeys file to bundler intake location on S3.
        //    //S3 location will be save as part of the request and bundler will getobject when processing request.
        //    foreach (DatasetFile df in datasetFileList)
        //    {
        //        _request.SourceKeys.Add(Tuple.Create(df.FileLocation, df.VersionId));
        //    }

        //    this._request.FileExtension = Path.GetExtension(sourceKeys.FirstOrDefault().Item1);

        //    string jsonRequest = JsonConvert.SerializeObject(_request);

        //    //    MemoryStream stream = new MemoryStream();
        //    //StreamWriter writer = new StreamWriter(stream);
        //    //writer.Write(s);
        //    //writer.Flush();
        //    //stream.Position = 0;
        //    //return stream;

        //    using (Stream s = GenerateStreamFromString(jsonRequest))
        //    {
        //        using (FileStream fs = new FileStream($"C:\\tmp\\DatasetBundler\\{this._request.RequestGuid}.json", FileMode.OpenOrCreate))
        //        {
        //            s.CopyTo(fs);
        //            fs.Flush();
        //        }                
        //    }

        //    //requestVersionId = s3Service.UploadDataFile(s, requestLocation);


        //    //Call Bulder Task on EC2 instance.  Pass requestLocation and requestVersionId variables as -key and -versionid parameters respectively

        //    //Bundler Bundle = new Bundler(s3Service);

        //    //Bundle.KeyContatenation(_request.SourceKeysFileLocation, _request.SourceKeysFileVersionId, 50000000, _request.TargetFileName);

        //}

        //public void DoWork()
        //{
        //    Sentry.Common.Logging.Logger.Debug("Starting Bundle Task...");
        //    ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(_pythonexe);

        //    myProcessStartInfo.UseShellExecute = false;
        //    myProcessStartInfo.RedirectStandardOutput = true;
        //    myProcessStartInfo.RedirectStandardError = true;

        //    StringBuilder arguments = new StringBuilder();
        //    arguments.Append(_keyBundlerApp);
        //    arguments.Append($@" --bucket ""{this._request.Bucket}""");
        //    arguments.Append($@" --keyfilelocation ""{this._request.SourceKeysFileLocation}""");
        //    arguments.Append($@" --keyfileversionid ""{this._request.SourceKeysFileVersionId}""");
        //    arguments.Append($@" --output ""{_bundleOutput}{this._request.RequestGuid}""");
        //    arguments.Append($@" --suffix ""{this._request.FileExtension}""");
        //    arguments.Append($@" --filesize ""{_fileSize}""");
        //    myProcessStartInfo.Arguments = arguments.ToString();

        //    Process myProcess = new Process();

        //    myProcess.StartInfo = myProcessStartInfo;
        //    myProcess.EnableRaisingEvents = true;

        //    //used eventhandlers based on the following STO answer
        //    //https://stackoverflow.com/questions/186822/capturing-console-output-from-a-net-application-c
        //    myProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
        //    myProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
        //    myProcess.Exited += new System.EventHandler(process_Exited);

        //    myProcess.Start();
        //    myProcess.BeginOutputReadLine();
        //    myProcess.BeginErrorReadLine();

        //    // wait exit signal from the app we called 
        //    myProcess.WaitForExit();

        //    // close the process 
        //    myProcess.Close();

        //    BundleResponse resp = new BundleResponse();

        //    resp.RequestGuid = this._request.RequestGuid;
        //    resp.TargetBucket = this._request.Bucket;
        //    resp.TargetKey = this.targetKey;
        //    resp.TargetVersionId = this.targetVersionId;
        //    resp.TargetETag = this.targetETag;
        //}


        //public BundleResponse StartBundleProcess(BundleRequest bunReq)
        //{
        //    ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(_pythonexe);

        //    myProcessStartInfo.UseShellExecute = false;
        //    myProcessStartInfo.RedirectStandardOutput = true;
        //    myProcessStartInfo.RedirectStandardError = true;

        //    StringBuilder arguments = new StringBuilder();
        //    arguments.Append(_keyBundlerApp);
        //    arguments.Append($@" --bucket ""{bunReq.Bucket}""");
        //    arguments.Append($@" --keyfilelocation ""{bunReq.SourceKeysFileLocation}""");
        //    arguments.Append($@" --keyfileversionid ""{bunReq.SourceKeysFileVersionId}""");
        //    arguments.Append($@" --output ""{_bundleOutput}{bunReq.RequestGuid}""");
        //    arguments.Append($@" --suffix ""{bunReq.FileExtension}""");
        //    arguments.Append($@" --filesize ""{_fileSize}""");
        //    myProcessStartInfo.Arguments = arguments.ToString();
            
        //    Process myProcess = new Process();

        //    myProcess.StartInfo = myProcessStartInfo;
        //    myProcess.EnableRaisingEvents = true;

        //    //used eventhandlers based on the following STO answer
        //    //https://stackoverflow.com/questions/186822/capturing-console-output-from-a-net-application-c
        //    myProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
        //    myProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
        //    myProcess.Exited += new System.EventHandler(process_Exited);

        //    myProcess.Start();
        //    myProcess.BeginOutputReadLine();
        //    myProcess.BeginErrorReadLine();

        //    // wait exit signal from the app we called 
        //    myProcess.WaitForExit();

        //    // close the process 
        //    myProcess.Close();

        //    BundleResponse resp = new BundleResponse();

        //    resp.RequestGuid = bunReq.RequestGuid;
        //    resp.TargetBucket = bunReq.Bucket;
        //    resp.TargetKey = this.targetKey;
        //    resp.TargetVersionId = this.targetVersionId;
        //    resp.TargetETag = this.targetETag;

        //    return resp;
        //}

        //private void process_Exited(object sender, EventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(e.ToString()))
        //    {
        //        //Sentry.Common.Logging.Logger.Info($"Bundle process exited with code {myProcess.ExitCode.ToString()}");
        //    }
            
        //}

        //private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(e.Data))
        //    {
        //        Sentry.Common.Logging.Logger.Error(e.Data);
        //    }
        //}

        //private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(e.Data))
        //    {
        //        if (e.Data.StartsWith("Target_File_Details"))
        //        {
        //            String[] details = e.Data.Split(new Char[] { '~' });

        //            targetKey = (details[1].Split(new Char[] { ':' }))[1];
        //            targetVersionId = (details[2].Split(new Char[] { ':' }))[1];
        //            targetETag = (details[3].Split(new Char[] { ':' }))[1];

        //        }
        //        Sentry.Common.Logging.Logger.Info(e.Data);
        //    }            
        //}

        //private static Stream GenerateStreamFromString(string s)
        //{
        //    MemoryStream stream = new MemoryStream();
        //    StreamWriter writer = new StreamWriter(stream);
        //    writer.Write(s);
        //    writer.Flush();
        //    stream.Position = 0;
        //    return stream;
        //}

        //private void TaskException(Task t)
        //{
        //    Sentry.Common.Logging.Logger.Fatal("Exception occurred on main Bundle Task. Stopping immediately.", t.Exception);
        //    //Environment.Exit(10001);
        //}

        //private class SourceObject
        //{
        //    private string _key;
        //    private string _versionid;
        //    private long _contentLength;

        //    public long ContentLenght
        //    {
        //        get { return _contentLength; }
        //        set { _contentLength = value; }
        //    }
        //    public string VersionId
        //    {
        //        get { return _versionid; }
        //        set { _versionid = value; }
        //    }
        //    public string Key
        //    {
        //        get { return _key; }
        //        set { _key = value; }
        //    }

        //}

        //private class Bundler
        //{
        //    private IDatasetService _s3Service;
        //    private long _minS3Size = 6000000;

        //    public Bundler(IDatasetService s3Service)
        //    {
        //        _s3Service = s3Service;
        //    }

        //    public void KeyContatenation(string keyfilelocation, string keyfileversionid, long max_filesize, string result_filepath)
        //    {

        //        //Get list of keys to concatenate
        //        string response = _s3Service.GetObject(keyfilelocation, keyfileversionid);

        //        List<Tuple<string, string>> SourceKeys = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(response);

        //        List<SourceObject> parts_list = Collect_Parts(SourceKeys);
        //        Sentry.Common.Logging.Logger.Debug($"Found {parts_list.Count()} keys to concatenate");
        //        List<List<SourceObject>> grouped_parts_list = Chunk_By_Size(parts_list, max_filesize);
        //        Sentry.Common.Logging.Logger.Debug($"Created {grouped_parts_list.Count()} concatenation groups");
        //        for (int i = 0; i < grouped_parts_list.Count(); i++)
        //        {
        //            Sentry.Common.Logging.Logger.Debug($"Concatenating group {i}/{grouped_parts_list.Count()}");
        //            RunSingleContatenation(grouped_parts_list[i], $"{result_filepath}-{i}");
        //        }

        //    }

        //    private void RunSingleContatenation(List<SourceObject> parts_list, string result_filepath)
        //    {
        //        String uploadId = null;
        //        if (parts_list.Count() > 1)
        //        {
        //            uploadId = _s3Service.StartUpload(result_filepath);

        //            List<CopyPartResponse> part_responses = AssembleParts(result_filepath, uploadId, parts_list);

        //        }
        //        else if(parts_list.Count() == 1)
        //        {

        //        }
        //        else
        //        {
        //            Sentry.Common.Logging.Logger.Debug($"No file to concatenate for {result_filepath}");
        //        }
        //    }

        //    private List<CopyPartResponse> AssembleParts(string result_filepath, string uploadId, List<SourceObject> parts_list)
        //    {
        //        List<CopyPartResponse> parts_mapping = new List<CopyPartResponse>();

        //        int partnum = 1;
        //        List<Tuple<string, string>> s3_parts = new List<Tuple<string, string>>();
        //        List<Tuple<string, string>> local_parts = new List<Tuple<string, string>>();

        //        // s3 CopyPart has a limitation on size of object
        //        foreach (SourceObject obj in parts_list)
        //        {
        //            if (obj.ContentLenght > _minS3Size)
        //            {
        //                s3_parts.Add(Tuple.Create(obj.Key, obj.VersionId));                        
        //            }
        //            else if(obj.ContentLenght <= _minS3Size)
        //            {
        //                local_parts.Add(Tuple.Create(obj.Key, obj.VersionId));
        //            }
        //        }

        //        foreach (Tuple<string, string> item in s3_parts)
        //        {
        //            CopyPartResponse resp = _s3Service.CopyPart(result_filepath, partnum, item.Item1, item.Item2, uploadId);
        //        }



        //    }

        //    /// <summary>
        //    /// Groups parts based on max file size
        //    /// </summary>
        //    /// <param name="parts_list"></param>
        //    /// <param name="max_filesize"></param>
        //    /// <returns></returns>
        //    private List<List<SourceObject>> Chunk_By_Size(List<SourceObject> parts_list, long max_filesize)
        //    {
        //        //Can be used to split target file into multiple based on max_file size
        //        // Currently commented out logic so method will produce only one group of
        //        // all source keys regardless of target file size.
        //        List<List<SourceObject>> grouped_list = new List<List<SourceObject>>();
        //        List<SourceObject> current_list = new List<SourceObject>();
        //        long current_size = 0;

        //        foreach (SourceObject p in parts_list)
        //        {
        //            current_size += p.ContentLenght;
        //            current_list.Add(p);

        //            //Uncomment these lines if splitting logic is needed
        //            //if (current_size > max_filesize)
        //            //{
        //            //    grouped_list.Add(current_list);
        //            //    current_list = new List<SourceObject>();
        //            //    current_size = 0;
        //            //}
        //        }

        //        // remove this line if splitting logic is needed
        //        grouped_list.Add(current_list);

        //        //If max size is larger than all file combined
        //        if (grouped_list.Count() == 0 && current_list.Count() > 0)
        //        {
        //            grouped_list.Add(current_list);
        //        }

        //        return grouped_list;
        //    }

        //    /// <summary>
        //    /// Returns list of s3 object keys with ContentLength metadata
        //    /// </summary>
        //    /// <param name="sourceKeys"></param>
        //    /// <returns></returns>
        //    private List<SourceObject> Collect_Parts(List<Tuple<string, string>> sourceKeys)
        //    {
        //        List<SourceObject> object_list = new List<SourceObject>();

        //        foreach (var item in sourceKeys)
        //        {
        //            SourceObject obj = new SourceObject();
        //            obj.Key = item.Item1;
        //            obj.VersionId = item.Item2;

        //            //Get object metadata, without retrieving whole object, specifically for the size of the object
        //            Dictionary<string, string> resp = _s3Service.GetObjectMetadata(obj.Key, obj.VersionId);

        //            obj.ContentLenght = Convert.ToInt64(resp["ContentLength"]);

        //            object_list.Add(obj);
        //        }

        //        return object_list;
        //    }
        //}

    }
}
