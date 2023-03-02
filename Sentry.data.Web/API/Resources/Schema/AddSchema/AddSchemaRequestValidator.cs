using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class AddSchemaRequestValidator : BaseSchemaRequestValidator<AddSchemaRequestModel>
    {
        private readonly ISAIDService _saidService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public AddSchemaRequestValidator(IDatasetContext datasetContext, ISAIDService saidService, IQuartermasterService quartermasterService, IAssociateInfoProvider associateInfoProvider) : base(datasetContext)
        {
            _saidService = saidService;
            _quartermasterService = quartermasterService;
            _associateInfoProvider = associateInfoProvider;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(AddSchemaRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel.Validate(x => x.SchemaName).Required().MaxLength(100)
                .Validate(x => x.SchemaDescription).Required()
                .Validate(x => x.IngestionTypeCode).Required().EnumValue(typeof(IngestionType), IngestionType.DSC_Pull.ToString())
                .Validate(x => x.PrimaryContactId).Required()
                .ValidationResponse;

            Task[] asyncValidations = new Task[] 
            { 
                requestModel.ValidateSaidEnvironmentAsync(_saidService, _quartermasterService, validationResponse),
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse)
            };

            Task<ConcurrentValidationResponse> baseValidations = base.ValidateAsync(requestModel);

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

            //FileSchemaFlow is reserved name
            if (!string.IsNullOrWhiteSpace(requestModel.SchemaName) && requestModel.SchemaName.StartsWith("FileSchemaFlow"))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.SchemaName), $"FileSchemaFlow is a reserved name");
            }

            //file type exists
            FileExtension fileType = string.IsNullOrWhiteSpace(requestModel.FileTypeCode) ? null : _datasetContext.FileExtensions.FirstOrDefault(x => x.Name.ToLower() == requestModel.FileTypeCode.ToLower());
            if (fileType == null)
            {
                AddFileTypeCodeValidationMessage(validationResponse);
            }
            else
            {
                ValidateDelimiter(requestModel.Delimiter, fileType, validationResponse);
            }

            //scope type required and exists
            if (string.IsNullOrWhiteSpace(requestModel.ScopeTypeCode) || !_datasetContext.DatasetScopeTypes.Any(x => x.Name.ToLower() == requestModel.ScopeTypeCode.ToLower()))
            {
                AddScopeTypeCodeValidationMessage(validationResponse);
            }

            validationResponse.AddValidationsFrom(await baseValidations);
            await Task.WhenAll(asyncValidations);

            return validationResponse;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((AddSchemaRequestModel)requestModel);
        }
    }
}