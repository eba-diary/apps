using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public abstract class BaseSchemaRequestValidator<T> : IRequestModelValidator<T> where T : BaseSchemaModel, IRequestModel
    {
        protected readonly IDatasetContext _datasetContext;

        protected BaseSchemaRequestValidator(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public virtual Task<ConcurrentValidationResponse> ValidateAsync(T requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.CompressionTypeCode).EnumValue(typeof(CompressionTypes))
                .Validate(x => x.PreprocessingTypeCode).EnumValue(typeof(DataFlowPreProcessingTypes))
                .ValidationResponse;        

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
            if (!string.IsNullOrWhiteSpace(requestModel.IngestionTypeCode) && Enum.TryParse(requestModel.IngestionTypeCode, true, out IngestionType ingestionType))
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

            return Task.FromResult(validationResponse);
        }

        public abstract Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel);

        protected void AddFileTypeCodeValidationMessage(ConcurrentValidationResponse validationResponse)
        {
            List<string> fileTypes = _datasetContext.FileExtensions.Select(x => x.Name).ToList();
            validationResponse.AddFieldValidation(nameof(BaseSchemaModel.FileTypeCode), $"Must provide a valid value - {string.Join(" | ", fileTypes)}");
        }

        protected void ValidateDelimiter(string delimiter, FileExtension fileType, ConcurrentValidationResponse validationResponse)
        {
            //delimited requires a delimiter
            if (fileType.Name == ExtensionNames.DELIMITED && string.IsNullOrWhiteSpace(delimiter))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.Delimiter), $"Required field when {nameof(BaseSchemaModel.FileTypeCode)} is {fileType.Name}");
            }
            //csv requires the delimiter to be a comma
            else if (fileType.Name == ExtensionNames.CSV && (string.IsNullOrWhiteSpace(delimiter) || delimiter != ","))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.Delimiter), $"Value must be a comma (,) when {nameof(BaseSchemaModel.FileTypeCode)} is {fileType.Name}");
            }
        }

        protected void AddScopeTypeCodeValidationMessage(ConcurrentValidationResponse validationResponse)
        {
            List<string> scopeTypes = _datasetContext.DatasetScopeTypes.Select(x => x.Name).ToList();
            validationResponse.AddFieldValidation(nameof(BaseSchemaModel.ScopeTypeCode), $"Must provide a valid value - {string.Join(" | ", scopeTypes)}");
        }
    }
}