using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using StructureMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class DfsDataFlowBasicProvider : BaseJobProvider
    {
        private string _filePath;

        public override void ConfigureProvider(RetrieverJob job)
        {
            throw new NotImplementedException();
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            _filePath = filePath;
            Execute(job);
        }

        public override void Execute(RetrieverJob job)
        {
            //Set Job
            _job = job;

            try
            {
                _job.JobLoggerMessage("Debug", $"start method {System.Reflection.MethodBase.GetCurrentMethod().Name}");

                //Set directory search
                var dirSearchCriteria = (String.IsNullOrEmpty(_filePath)) ? "*" : _filePath;

                DataFlowStep targetS3DropStep;

                using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext _dsContext = Container.GetInstance<IDatasetContext>();

                    targetS3DropStep = _dsContext.DataFlowStep.Where(w => w.DataFlow == job.DataFlow && (w.DataAction_Type_Id == DataActionType.S3Drop || w.DataAction_Type_Id == DataActionType.ProducerS3Drop)).FirstOrDefault();

                    //find target s3 drop location
                    string targetPrefix = targetS3DropStep.TriggerKey;

                    _job.JobLoggerMessage("Debug", $"directory:{_job.GetUri().LocalPath} searchcriteria:{dirSearchCriteria}");

                    // Only search top directory and source files not locked and does not start with two exclamaition points !!
                    string[] files = Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w)).ToArray();

                    _job.JobLoggerMessage("Debug", $"found {files.Length.ToString()}");

                    try
                    {
                        //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                        /******************************************************************************
                        * Utilizing Trigger bucket since we want to trigger the targetStep identified
                        ******************************************************************************/
                        ProcessFilesInParallel(files, targetS3DropStep.TriggerBucket, targetPrefix);
                    }
                    catch (AggregateException ae)
                    {
                        var ignoredExceptions = new List<Exception>();

                        // This is where you can choose which exceptions to handle.
                        foreach (var ex in ae.Flatten().InnerExceptions)
                        {
                            if (ex is Exception)
                                _job.JobLoggerMessage("Error","dfsdataflowbasicprovider-execute failed sending file(s)", ex);
                            else
                                ignoredExceptions.Add(ex);
                        }

                        if (ignoredExceptions.Count > 0)
                        {
                            throw new AggregateException(ignoredExceptions);
                        }
                    }
                    catch(Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "dfsdataflowbasicprovider-execute processfilesinparallel failed", ex);
                    }
                }

                _job.JobLoggerMessage("Debug", $"end method {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "dfsdataflowbasicprovider-execute failed", ex);
            }
        }

        private void ProcessFilesInParallel(string[] fileArray, string targetBucket, string targetPrefix)
        {
            _job.JobLoggerMessage("Debug", $"start method {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();
            
            Parallel.ForEach(fileArray, item =>
            {
                try
                {
                    using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
                    {
                        IS3ServiceProvider _s3ServiceProvider = Container.GetInstance<IS3ServiceProvider>();

                        //_s3ServiceProvider.UploadDataFile(item, GenerateTargetKey(targetPrefix, item));
                        _s3ServiceProvider.UploadDataFile(item, targetBucket, GenerateTargetKey(targetPrefix, item));

                        //Delete file within drop location
                        try
                        {
                            File.Delete(item);
                        }
                        catch (Exception ex)
                        {
                            _job.JobLoggerMessage("Error", $"Failed Deleting File from drop location : ({item})", ex);
                        }
                    }   
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            _job.JobLoggerMessage("Debug", $"end method {System.Reflection.MethodBase.GetCurrentMethod().Name}");

        }

        private string GenerateTargetKey(string targetPrefix, string a)
        {
            return $"{targetPrefix}{Path.GetFileName(a)}";
        }

        private bool IsFileLocked(string filePath)
        {
            _job.JobLoggerMessage("Debug", $"start method {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            try
            {
                using (File.Open(filePath, FileMode.Open))
                {
                    //just determining if file can be opened, no actions are to be performed
                }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            _job.JobLoggerMessage("Debug", $"end method {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            return false;
        }
    }
}
