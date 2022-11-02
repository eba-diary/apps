using Polly.Registry;
using RestSharp;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;

namespace Sentry.data.Infrastructure
{
    public class GenericHttpsDataFlowProvider : GenericHttpsProvider
    {
        private readonly IDataFlowService _dataFlowService;

        public GenericHttpsDataFlowProvider(Lazy<IDatasetContext> datasetContext, Lazy<IConfigService> configService,
                Lazy<IEncryptionService> encryptionService, Lazy<IJobService> jobService, IDataFlowService dataFlowService,
                IRestClient restClient, IReadOnlyPolicyRegistry<string> policyRegistry, IDataFeatures dataFeatures) : base(datasetContext, configService, encryptionService, jobService, policyRegistry, restClient, dataFeatures)
        {
            _dataFlowService = dataFlowService;
        }

        protected override void FindTargetJob()
        {
            //Find the target prefix (s3) from S3DropAction on the DataFlow attached to RetrieverJob
            DataFlowStep _producerS3Drop = _dataFlowService.GetDataFlowStepForDataFlowByActionType(_job.DataFlow.Id, DataActionType.ProducerS3Drop);
            DataFlowStep _s3Drop = null;
            if (_producerS3Drop == null)
            {
                _s3Drop = _dataFlowService.GetDataFlowStepForDataFlowByActionType(_job.DataFlow.Id, DataActionType.S3Drop);
            }

            _targetStep = _producerS3Drop ?? _s3Drop;

            if (_targetStep == null)
            {
                _job.JobLoggerMessage("Error", "find_targetstep_failure");
                throw new Exception("Did not find target producers3drop data flow step");
            }

            _IsTargetS3 = true;
        }

        protected override void SetTargetPath(string extension)
        {
            try
            {
                _targetPath = $"{_jobService.Value.GetTargetPath(_targetStep, _job)}.{extension}";
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "targetstep_gettargetpath_failure", ex);
                throw;
            }
        }
    }
}
