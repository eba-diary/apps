using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Linq;
using System.Threading.Tasks;

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
                if (fileType == null)
                {
                    AddFileTypeCodeValidationMessage(validationResponse);
                }
                else
                {
                    ValidateDelimiter(requestModel.Delimiter, fileType, validationResponse);
                }
            }

            if (!string.IsNullOrWhiteSpace(requestModel.ScopeTypeCode) && !_datasetContext.DatasetScopeTypes.Any(x => x.Name.ToLower() == requestModel.ScopeTypeCode.ToLower()))
            {
                AddScopeTypeCodeValidationMessage(validationResponse);
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