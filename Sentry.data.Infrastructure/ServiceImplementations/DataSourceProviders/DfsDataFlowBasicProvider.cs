using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using StructureMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
                //Set directory search
                var dirSearchCriteria = (String.IsNullOrEmpty(_filePath)) ? "*" : _filePath;

                DataFlowStep targetS3DropStep;

                using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
                {
                    IDatasetContext _dsContext = Container.GetInstance<IDatasetContext>();

                    DataFlow df = _dsContext.DataFlowStep.Where(w => w.SchemaMappings.Any(a => a.MappedSchema.SchemaId == job.FileSchema.SchemaId && a.DataFlowStepId.DataAction_Type_Id == DataActionType.SchemaLoad)).Select(s => s.DataFlow).FirstOrDefault();

                    targetS3DropStep = _dsContext.DataFlowStep.Where(w => w.DataFlow == df && w.DataAction_Type_Id == DataActionType.S3Drop).FirstOrDefault();

                    //find target s3 drop location
                    string targetPrefix = targetS3DropStep.TriggerKey;

                    // Only search top directory and source files not locked and does not start with two exclamaition points !!
                    string[] files = Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w)).ToArray();

                    try
                    {
                        //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                        ProcessFilesInParallel(files, targetPrefix);
                    }
                    catch (AggregateException ae)
                    {
                        var ignoredExceptions = new List<Exception>();

                        // This is where you can choose which exceptions to handle.
                        foreach (var ex in ae.Flatten().InnerExceptions)
                        {
                            if (ex is Exception)
                                Logger.Error("dfsdataflowbasicprovider-execute failed sending file(s)");
                            else
                                ignoredExceptions.Add(ex);
                        }
                        if (ignoredExceptions.Count > 0) throw new AggregateException(ignoredExceptions);
                    }                    
                }
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "dfsdataflowbasicprovider-execute failed", ex);
            }
        }

        private void ProcessFilesInParallel(string[] fileArray, string targetPrefix)
        {
            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(fileArray, item =>
                {
                    try
                    {
                        using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
                        {
                            IS3ServiceProvider _s3ServiceProvider = Container.GetInstance<IS3ServiceProvider>();

                            _s3ServiceProvider.UploadDataFile(item, GenerateTargetKey(targetPrefix, item));
                        }   
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }
            catch
            {

            }
        }

        private string GenerateTargetKey(string targetPrefix, string a)
        {
            return $"{targetPrefix}{Path.GetFileName(a)}";
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }
    }
}
