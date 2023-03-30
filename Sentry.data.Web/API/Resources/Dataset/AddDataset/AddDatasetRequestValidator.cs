using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class AddDatasetRequestValidator : BaseDatasetRequestValidator<AddDatasetRequestModel>
    {
        private readonly ISAIDService _saidService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public AddDatasetRequestValidator(IDatasetContext datasetContext, ISAIDService saidService, IQuartermasterService quartermasterService, IAssociateInfoProvider associateInfoProvider) : base(datasetContext)
        {
            _saidService = saidService;
            _quartermasterService = quartermasterService;
            _associateInfoProvider = associateInfoProvider;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(AddDatasetRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel.Validate(x => x.DatasetName).Required().MaxLength(1024)
                .Validate(x => x.DatasetDescription).Required()
                .Validate(x => x.ShortName).Required().MaxLength(12).RegularExpression("^[0-9a-zA-Z]*$", "Only alphanumeric characters are allowed")
                .Validate(x => x.DataClassificationTypeCode).Required().EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString())
                .Validate(x => x.OriginalCreator).Required()
                .Validate(x => x.PrimaryContactId).Required()
                .ValidationResponse;

            Task[] asyncValidations = new Task[]
            {
                requestModel.ValidateSaidEnvironmentAsync(_saidService, _quartermasterService, validationResponse),
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse)
            };

            Task<ConcurrentValidationResponse> baseValidations = base.ValidateAsync(requestModel);

            //category required and exists
            if (string.IsNullOrWhiteSpace(requestModel.CategoryCode) || !_datasetContext.Categories.Any(x => x.Name.ToLower() == requestModel.CategoryCode.ToLower() && x.ObjectType == DataEntityCodes.DATASET))
            {
                AddCategoryCodeValidationMessage(validationResponse);
            }

            bool isValidNamedEnvironment = !validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment));

            //dataset name exists
            if (!validationResponse.HasValidationsFor(nameof(requestModel.DatasetName)) && _datasetContext.Datasets.Any(w => w.DatasetName.ToLower() == requestModel.DatasetName.ToLower() && w.DatasetType == DataEntityCodes.DATASET))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.DatasetName), "Dataset name already exists. If attempting to create a copy of an existing dataset in a different named environment, please use dataset migration.");
            }

            if (!validationResponse.HasValidationsFor(nameof(requestModel.ShortName)))
            {
                //short name is 'Default'
                if (string.Equals(requestModel.ShortName, SecurityConstants.ASSET_LEVEL_GROUP_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), $"Short name cannot be '{SecurityConstants.ASSET_LEVEL_GROUP_NAME}'");
                }
                //short name exists
                else if (isValidNamedEnvironment && _datasetContext.Datasets.Any(d => d.ShortName.ToLower() == requestModel.ShortName.ToLower() && d.DatasetType == DataEntityCodes.DATASET && d.NamedEnvironment == requestModel.NamedEnvironment))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), "Short name is already in use by another Dataset for the named environment");
                }
            }

            validationResponse.AddValidationsFrom(await baseValidations);
            await Task.WhenAll(asyncValidations);

            return validationResponse;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((AddDatasetRequestModel)requestModel);
        }
    }
}