using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.data.Core.Interfaces.DataProcessing;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.ServiceImplementations.DataProcessing
{
    public class DataStepService : IDataStepService
    {
        private IBaseActionProvider _provider;
        private readonly IDatasetContext _dsContext;

        //This approach was used for providers as temporary until code is refactor into java
        private readonly Lazy<IS3DropProvider> _s3DropProvider;
        private readonly Lazy<IRawStorageProvider> _rawStorageProvider;
        private readonly Lazy<IQueryStorageProvider> _queryStorageProvider;
        private readonly Lazy<ISchemaLoadProvider> _schemaLoadProvider;
        private readonly Lazy<IConvertToParquetProvider> _convertToParquetProvider;
        private readonly Lazy<IUncompressZipProvider> _uncompressZipProvider;
        private readonly Lazy<IUncompressGzipProvider> _uncompressGzipProvider;
        private readonly Lazy<ISchemaMapProvider> _schemaMapProvider;
        private readonly Lazy<IGoogleApiActionProvider> _googleApiActionProvider;
        private readonly Lazy<IClaimIQActionProvider> _claimIQActionProvider;
        private readonly Lazy<IFixedWidthProvider> _fixedWidthProvider;

        public DataStepService(IDatasetContext datasetContext, Lazy<IS3DropProvider> s3DropProvider, Lazy<IRawStorageProvider> rawStorageProvider,
            Lazy<IQueryStorageProvider> queryStorageProvider, Lazy<ISchemaLoadProvider> schemaLoadProvider, Lazy<IConvertToParquetProvider> convertToParquetProvider,
            Lazy<IUncompressZipProvider> uncompressZipProvider, Lazy<IUncompressGzipProvider> uncompressGzipProvider, Lazy<ISchemaMapProvider> schemaMapProvider,
            Lazy<IGoogleApiActionProvider> googleApiActionProvider, Lazy<IClaimIQActionProvider> claimIQActionProvider, Lazy<IFixedWidthProvider> fixedWidthProvider)
        {
            _dsContext = datasetContext;
            _s3DropProvider = s3DropProvider;
            _rawStorageProvider = rawStorageProvider;
            _queryStorageProvider = queryStorageProvider;
            _schemaLoadProvider = schemaLoadProvider;
            _convertToParquetProvider = convertToParquetProvider;
            _uncompressZipProvider = uncompressZipProvider;
            _uncompressGzipProvider = uncompressGzipProvider;
            _schemaMapProvider = schemaMapProvider;
            _googleApiActionProvider = googleApiActionProvider;
            _claimIQActionProvider = claimIQActionProvider;
            _fixedWidthProvider = fixedWidthProvider;
        }

        #region Provider Properties
        public IS3DropProvider S3DropProvider
        {
            get { return _s3DropProvider.Value; }
        }
        public IRawStorageProvider RawStorageProvider
        {
            get { return _rawStorageProvider.Value; }
        }

        public IQueryStorageProvider QueryStorageProvider
        {
            get { return _queryStorageProvider.Value; }
        }

        public ISchemaLoadProvider SchemaLoadProvider
        {
            get { return _schemaLoadProvider.Value; }
        }

        public IConvertToParquetProvider ConvertToParquetProvider
        {
            get { return _convertToParquetProvider.Value; }
        }

        public IUncompressZipProvider UncompressZipProvider
        {
            get { return _uncompressZipProvider.Value; }
        }

        public IUncompressGzipProvider UncompressGzipProvider
        {
            get { return _uncompressGzipProvider.Value; }
        }

        public ISchemaMapProvider SchemaMapProvider
        {
            get { return _schemaMapProvider.Value; }
        }

        public IGoogleApiActionProvider GoogleApiActionProvider
        {
            get { return _googleApiActionProvider.Value; }
        }

        public IClaimIQActionProvider ClaimIQActionProvider
        {
            get { return _claimIQActionProvider.Value; }
        }

        public IFixedWidthProvider FixedWidthProvider
        {
            get { return _fixedWidthProvider.Value; }
        }
        #endregion



        public void ExecuteStep(DataFlowStepEvent stepEvent)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteStepAsync(DataFlowStepEvent stepEvent)
        {
            DataFlowStep step = _dsContext.GetById<DataFlowStep>(stepEvent.StepId);

            SetStepProvider(step.DataAction_Type_Id);

            await _provider.ExecuteActionAsync(step, stepEvent).ConfigureAwait(false);

            _dsContext.SaveChanges();
        }

        public void PublishStartEvent(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            throw new NotImplementedException();
        }

        public async Task PublishStartEventAsync(DataFlowStep step, string flowExecutionGuid, string runInstanceGuid, S3ObjectEvent s3Event)
        {
            try
            {                
                SetStepProvider(step.DataAction_Type_Id);

                if (_provider != null)
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"start-method <datastepservice-publishstartevent>", Log_Level.Debug);
                    await _provider.PublishStartEventAsync(step, flowExecutionGuid, runInstanceGuid, s3Event).ConfigureAwait(false);
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                }
                else
                {
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"datastepserivce-notconfiguredforprovider provider:{step.DataAction_Type_Id.ToString()}", Log_Level.Warning);
                    step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                }

                _dsContext.SaveChanges();
            }
            catch (Exception ex)
            {
                step.LogExecution(flowExecutionGuid, runInstanceGuid, $"end-method <datastepservice-publishstartevent>", Log_Level.Debug);
                _dsContext.SaveChanges();
            }
        }

        private void SetStepProvider(DataActionType actionType)
        {
            switch (actionType)
            {
                case DataActionType.S3Drop:
                case DataActionType.ProducerS3Drop:
                    _provider = S3DropProvider;
                    break;
                case DataActionType.RawStorage:
                    _provider = RawStorageProvider;
                    break;
                case DataActionType.QueryStorage:
                    _provider = QueryStorageProvider;
                    break;
                case DataActionType.SchemaLoad:
                    _provider = SchemaLoadProvider;
                    break;
                case DataActionType.ConvertParquet:
                    _provider = ConvertToParquetProvider;
                    break;
                case DataActionType.UncompressZip:
                    _provider = UncompressZipProvider;
                    break;
                case DataActionType.UncompressGzip:
                    _provider = UncompressGzipProvider;
                    break;
                case DataActionType.SchemaMap:
                    _provider = SchemaMapProvider;
                    break;
                case DataActionType.GoogleApi:
                    _provider = GoogleApiActionProvider;
                    break;
                case DataActionType.ClaimIq:
                    _provider = ClaimIQActionProvider;
                    break;
                case DataActionType.FixedWidth:
                    _provider = FixedWidthProvider;
                    break;
                case DataActionType.None:
                default:
                    break;
            }
        }
    }
}
