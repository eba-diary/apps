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
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.IsCompressed), $"Must set to true to use {nameof(BaseSchemaModel.CompressionTypeCode)}");
            }
            else if (requestModel.IsCompressed && string.IsNullOrWhiteSpace(requestModel.CompressionTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.CompressionTypeCode), $"Must provide a valid value when {nameof(requestModel.IsCompressed)} is true - {string.Join(" | ", Enum.GetNames(typeof(CompressionTypes)))}");
            }

            //PreprocessingCode required when IsPreprocessingRequired is true
            if (!requestModel.IsPreprocessingRequired && !string.IsNullOrWhiteSpace(requestModel.PreprocessingTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.IsPreprocessingRequired), $"Must set to true to use {nameof(BaseSchemaModel.PreprocessingTypeCode)}");
            }
            else if (requestModel.IsPreprocessingRequired && string.IsNullOrWhiteSpace(requestModel.PreprocessingTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.PreprocessingTypeCode), $"Must provide a valid value when {nameof(BaseSchemaModel.IsPreprocessingRequired)} is true - {string.Join(" | ", Enum.GetNames(typeof(DataFlowPreProcessingTypes)))}");
            }

            //KafkaTopicName required when IngestionTypeCode is Topic
            if (!string.IsNullOrWhiteSpace(requestModel.IngestionTypeCode) && Enum.TryParse(requestModel.IngestionTypeCode, true, out IngestionType ingestionType))
            {
                if (ingestionType == IngestionType.Topic)
                {
                    if (string.IsNullOrWhiteSpace(requestModel.KafkaTopicName))
                    {
                        validationResponse.AddFieldValidation(nameof(BaseSchemaModel.KafkaTopicName), $"Required field when {nameof(BaseSchemaModel.IngestionTypeCode)} is {IngestionType.Topic}");
                    }
                    else if (_datasetContext.DataFlow.Any(x => x.TopicName == requestModel.KafkaTopicName))
                    {
                        validationResponse.AddFieldValidation(nameof(BaseSchemaModel.KafkaTopicName), "Kafka topic name already exists");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(requestModel.KafkaTopicName))
                {
                    validationResponse.AddFieldValidation(nameof(BaseSchemaModel.IngestionTypeCode), $"Value must be {IngestionType.Topic} to set {nameof(BaseSchemaModel.KafkaTopicName)}");
                }
            }

            return Task.FromResult(validationResponse);
        }

        public abstract Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel);

        protected void ValidateFileTypeCode(BaseSchemaModel requestModel, FileExtension fileType, ConcurrentValidationResponse validationResponse)
        {
            //invalid file type
            if (fileType == null)
            {
                AddFileTypeCodeValidationMessage(validationResponse);
            }
            //validate properties that depend on file type
            else
            {
                ValidateDelimiter(requestModel.Delimiter, fileType, validationResponse);
                ValidateHasHeader(requestModel.HasHeader, fileType, validationResponse);
                ValidateSchemaRootPath(requestModel.SchemaRootPath, fileType, validationResponse);
            }
        }

        protected void AddFileTypeCodeValidationMessage(ConcurrentValidationResponse validationResponse)
        {
            List<string> fileTypes = _datasetContext.FileExtensions.Select(x => x.Name).ToList();
            validationResponse.AddFieldValidation(nameof(BaseSchemaModel.FileTypeCode), $"Must provide a valid value - {string.Join(" | ", fileTypes)}");
        }

        protected void AddScopeTypeCodeValidationMessage(ConcurrentValidationResponse validationResponse)
        {
            List<string> scopeTypes = _datasetContext.DatasetScopeTypes.Select(x => x.Name).ToList();
            validationResponse.AddFieldValidation(nameof(BaseSchemaModel.ScopeTypeCode), $"Must provide a valid value - {string.Join(" | ", scopeTypes)}");
        }

        private void ValidateDelimiter(string delimiter, FileExtension fileType, ConcurrentValidationResponse validationResponse)
        {
            //delimited requires a delimiter
            if (fileType.Name == ExtensionNames.DELIMITED)
            {
                if (string.IsNullOrEmpty(delimiter))
                {
                    validationResponse.AddFieldValidation(nameof(BaseSchemaModel.Delimiter), $"Required field when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.DELIMITED}");
                }
                else if (delimiter == ",")
                {
                    validationResponse.AddFieldValidation(nameof(BaseSchemaModel.FileTypeCode), $"Value must be {ExtensionNames.CSV} if {nameof(BaseSchemaModel.Delimiter)} is a comma (,)");
                }
                else if (delimiter.Length > 1)
                {
                    validationResponse.AddFieldValidation(nameof(BaseSchemaModel.Delimiter), $"Must be a single character");
                }
            }
            //csv requires the delimiter to be a comma
            else if (fileType.Name == ExtensionNames.CSV)
            {
                if (delimiter != ",")
                {
                    validationResponse.AddFieldValidation(nameof(BaseSchemaModel.Delimiter), $"Value must be a comma (,) when {nameof(BaseSchemaModel.FileTypeCode)} is {ExtensionNames.CSV}");
                }
            }
            //delimiter should not be populated for any other file types
            else if (!string.IsNullOrEmpty(delimiter))
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.FileTypeCode), $"Value must be {ExtensionNames.CSV} or {ExtensionNames.DELIMITED} to set {nameof(BaseSchemaModel.Delimiter)}");
            }
        }

        private void ValidateHasHeader(bool hasHeader, FileExtension fileType, ConcurrentValidationResponse validationResponse)
        {
            if (hasHeader && fileType.Name != ExtensionNames.DELIMITED && fileType.Name != ExtensionNames.CSV)
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.HasHeader), $"Value must be false when {nameof(BaseSchemaModel.FileTypeCode)} is {fileType.Name}");
            }
        }

        private void ValidateSchemaRootPath(string schemaRootPath, FileExtension fileType, ConcurrentValidationResponse validationResponse)
        {
            if (!string.IsNullOrWhiteSpace(schemaRootPath) && fileType.Name != ExtensionNames.JSON && fileType.Name != ExtensionNames.XML)
            {
                validationResponse.AddFieldValidation(nameof(BaseSchemaModel.FileTypeCode), $"Value must be {ExtensionNames.JSON} or {ExtensionNames.XML} to set {nameof(BaseSchemaModel.SchemaRootPath)}");
            }
        }
    }
}