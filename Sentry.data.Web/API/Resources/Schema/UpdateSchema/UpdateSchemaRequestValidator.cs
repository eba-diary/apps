using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class UpdateSchemaRequestValidator : BaseSchemaRequestValidator<UpdateSchemaRequestModel>
    {
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public UpdateSchemaRequestValidator(IDatasetContext datasetContext, IAssociateInfoProvider associateInfoProvider) : base(datasetContext)
        {
            _associateInfoProvider = associateInfoProvider;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(UpdateSchemaRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.IngestionTypeCode).EnumValue(typeof(IngestionType), IngestionType.DSC_Pull.ToString())
                .ValidationResponse;

            Task primaryContactValidation = requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse);

            Task<ConcurrentValidationResponse> baseValidations = base.ValidateAsync(requestModel);

            if (!string.IsNullOrWhiteSpace(requestModel.FileTypeCode))
            {
                FileExtension fileType = _datasetContext.FileExtensions.FirstOrDefault(x => x.Name.ToLower() == requestModel.FileTypeCode.ToLower());
                ValidateFileTypeCode(requestModel, fileType, validationResponse);
            }
            else if (!string.IsNullOrEmpty(requestModel.Delimiter))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.FileTypeCode), $"Value must be {ExtensionNames.CSV} or {ExtensionNames.DELIMITED} to set {nameof(BaseSchemaModel.Delimiter)}");
            }

            if (!string.IsNullOrWhiteSpace(requestModel.ScopeTypeCode) && !_datasetContext.DatasetScopeTypes.Any(x => x.Name.ToLower() == requestModel.ScopeTypeCode.ToLower()))
            {
                AddScopeTypeCodeValidationMessage(validationResponse);
            }

            if (!string.IsNullOrWhiteSpace(requestModel.KafkaTopicName) && string.IsNullOrWhiteSpace(requestModel.IngestionTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.IngestionTypeCode), $"Value must be {IngestionType.Topic} to set {nameof(requestModel.KafkaTopicName)}");
            }

            if (!string.IsNullOrWhiteSpace(requestModel.SchemaRootPath) && string.IsNullOrWhiteSpace(requestModel.FileTypeCode))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.FileTypeCode), $"Value must be {ExtensionNames.JSON} or {ExtensionNames.XML} to set {nameof(BaseSchemaModel.SchemaRootPath)}");
            }

            validationResponse.AddValidationsFrom(await baseValidations);
            await primaryContactValidation;

            return validationResponse;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((UpdateSchemaRequestModel)requestModel);
        }
    }
}