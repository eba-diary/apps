using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class AddSchemaRequestValidator : IRequestModelValidator<AddSchemaRequestModel>
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISAIDService _saidService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public AddSchemaRequestValidator(IDatasetContext datasetContext, ISAIDService saidService, IQuartermasterService quartermasterService, IAssociateInfoProvider associateInfoProvider)
        {
            _datasetContext = datasetContext;
            _saidService = saidService;
            _quartermasterService = quartermasterService;
            _associateInfoProvider = associateInfoProvider;
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(AddSchemaRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel.Validate(x => x.SchemaName).Required().MaxLength(100)
                .Validate(x => x.SchemaDescription).Required()
                .Validate(x => x.FileTypeCode).Required()
                .Validate(x => x.IngestionTypeCode).Required().EnumValue(typeof(IngestionType), IngestionType.DSC_Pull.ToString())
                .Validate(x => x.CompressionTypeCode).EnumValue(typeof(CompressionTypes))
                .Validate(x => x.PreprocessingTypeCode).EnumValue(typeof(DataFlowPreProcessingTypes))
                .Validate(x => x.PrimaryContactId).Required()
                .Validate(x => x.ScopeTypeCode).Required()
                .ValidationResponse;

            Task[] asyncValidations = new Task[] 
            { 
                requestModel.ValidateSaidEnvironmentAsync(_saidService, _quartermasterService, validationResponse),
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse)
            };

            Dataset dataset = _datasetContext.GetById<Dataset>(requestModel.DatasetId);

            //check dataset exists for dataset id
            if (dataset == null)
            {
                validationResponse.AddFieldValidation(nameof(requestModel.DatasetId), "Dataset does not exist");
            }
            //check schema name does not already exist for dataset
            else if (!validationResponse.HasValidationsFor(nameof(requestModel.SchemaName)) && dataset.DatasetFileConfigs.Any(x => !x.DeleteInd && x.Name.ToLower() == requestModel.SchemaName.ToLower()))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.SchemaName), "Schema name already exists for the Dataset");
            }

            //check for valid file type
            FileExtension fileType = validationResponse.HasValidationsFor(nameof(requestModel.FileTypeCode)) ? null : _datasetContext.FileExtensions.FirstOrDefault(x => x.Name.ToLower() == requestModel.FileTypeCode.ToLower());
            if (fileType == null)
            {
                List<string> fileTypes = _datasetContext.FileExtensions.Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(requestModel.FileTypeCode), $"Must provide a valid value - {string.Join(" | ", fileTypes)}");
            }
            else
            {
                //delimited requires a delimiter
                if (fileType.Name == ExtensionNames.DELIMITED && string.IsNullOrWhiteSpace(requestModel.Delimiter))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.Delimiter), $"Required field when {nameof(requestModel.FileTypeCode)} is {fileType.Name}");
                }
                //csv requires the delimiter to be a comma
                else if (fileType.Name == ExtensionNames.CSV && (string.IsNullOrWhiteSpace(requestModel.Delimiter) || requestModel.Delimiter != ","))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.Delimiter), $"Value must be a comma (,) when {nameof(requestModel.FileTypeCode)} is {fileType.Name}");
                }
            }

            //don't require datasetscopetype and see what happens
            if (validationResponse.HasValidationsFor(nameof(requestModel.ScopeTypeCode)) || !_datasetContext.DatasetScopeTypes.Any(x => x.Name.ToLower() == requestModel.ScopeTypeCode.ToLower()))
            {
                List<string> scopeTypes = _datasetContext.DatasetScopeTypes.Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(requestModel.ScopeTypeCode), $"Must provide a valid value - {string.Join(" | ", scopeTypes)}");
            }

            //CompressionTypeCode required when IsCompressed is true
            if (!requestModel.IsCompressed && !string.IsNullOrWhiteSpace(requestModel.CompressionTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.IsCompressed), $"{requestModel.CompressionTypeCode} is not used when {requestModel.IsCompressed} is false");
            }
            else if (requestModel.IsCompressed && string.IsNullOrWhiteSpace(requestModel.CompressionTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.CompressionTypeCode), $"Must provide a valid value when {nameof(requestModel.IsCompressed)} is true - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}");
            }

            //PreprocessingCode required when IsPreprocessingRequired is true
            if (!requestModel.IsPreprocessingRequired && !string.IsNullOrWhiteSpace(requestModel.PreprocessingTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.IsPreprocessingRequired), $"{requestModel.PreprocessingTypeCode} is not used when {requestModel.IsPreprocessingRequired} is false");
            }
            else if (requestModel.IsPreprocessingRequired && string.IsNullOrWhiteSpace(requestModel.PreprocessingTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.PreprocessingTypeCode), $"Must provide a valid value when {nameof(requestModel.IsPreprocessingRequired)} is true - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}");
            }

            //KafkaTopicName required when IngestionTypeCode is Topic
            if (!validationResponse.HasValidationsFor(nameof(requestModel.IngestionTypeCode)) && Enum.TryParse(requestModel.IngestionTypeCode, true, out IngestionType ingestionType))
            {
                if (ingestionType == IngestionType.Topic)
                {
                    if (string.IsNullOrWhiteSpace(requestModel.KafkaTopicName))
                    {
                        validationResponse.AddFieldValidation(nameof(requestModel.KafkaTopicName), $"Required field when {nameof(requestModel.IngestionTypeCode)} is {IngestionType.Topic}");
                    }
                    else if (_datasetContext.DataFlow.Any(x => x.TopicName == requestModel.KafkaTopicName))
                    {
                        validationResponse.AddFieldValidation(nameof(requestModel.KafkaTopicName), "Kafka topic name already exists");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(requestModel.KafkaTopicName))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.IngestionTypeCode), $"Value must be {IngestionType.Topic} to use {nameof(requestModel.KafkaTopicName)}");
                }
            }

            //FileSchemaFlow is reserved name
            if (!string.IsNullOrWhiteSpace(requestModel.SchemaName) && requestModel.SchemaName.StartsWith("FileSchemaFlow"))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.SchemaName), $"FileSchemaFlow is a reserved name");
            }

            await Task.WhenAll(asyncValidations);

            return validationResponse;
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((AddSchemaRequestModel)requestModel);
        }
    }
}