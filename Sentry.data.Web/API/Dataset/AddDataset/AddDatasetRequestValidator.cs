using Nest;
using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class AddDatasetRequestValidator : IRequestModelValidator<AddDatasetRequestModel>
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public AddDatasetRequestValidator(IDatasetContext datasetContext, IQuartermasterService quartermasterService, IAssociateInfoProvider associateInfoProvider)
        {
            _datasetContext = datasetContext;
            _quartermasterService = quartermasterService;
            _associateInfoProvider = associateInfoProvider;
        }

        public ValidationResponseModel Validate(AddDatasetRequestModel requestModel)
        {
            ValidationResponseModel validationResponse = new ValidationResponseModel();

            validationResponse.Validate(() => requestModel.DatasetName).Required()
                .Validate(() => requestModel.DatasetDescription).Required()
                .Validate(() => requestModel.ShortName).Required().MaxLength(12).RegularExpression("^[0-9a-zA-Z]*$", "Only alphanumeric characters are allowed")
                .Validate(() => requestModel.SaidAssetCode).Required()
                .Validate(() => requestModel.CategoryName).Required()
                .Validate(() => requestModel.OriginationCode).Required().EnumValue<DatasetOriginationCode>()
                .Validate(() => requestModel.DataClassificationTypeCode).Required().EnumValue<DataClassificationType>()
                .Validate(() => requestModel.OriginalCreator).Required()
                .Validate(() => requestModel.NamedEnvironment).Required().RegularExpression("^[A-Z0-9]{1,10}$", "Must be alphanumeric, all caps, and less than 10 characters")
                .Validate(() => requestModel.NamedEnvironmentTypeCode).Required().EnumValue<NamedEnvironmentType>()
                .Validate(() => requestModel.PrimaryContactId).Required();

            //dataset name exists
            if (!string.IsNullOrWhiteSpace(requestModel.DatasetName) && !string.IsNullOrWhiteSpace(requestModel.NamedEnvironment) &&
                _datasetContext.Datasets.Any(w => w.DatasetName.ToLower() == requestModel.DatasetName.ToLower() && w.DatasetType == DataEntityCodes.DATASET && w.NamedEnvironment == requestModel.NamedEnvironment))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.DatasetName), "Dataset name already exists for the named environment");
            }

            //short name is not 'Default'
            if (!string.IsNullOrWhiteSpace(requestModel.ShortName))
            {
                if (string.Equals(requestModel.ShortName, SecurityConstants.ASSET_LEVEL_GROUP_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), $"Short name cannot be '{SecurityConstants.ASSET_LEVEL_GROUP_NAME}'");
                }

                if (!string.IsNullOrWhiteSpace(requestModel.NamedEnvironment) &&
                    _datasetContext.Datasets.Any(d => d.ShortName.ToLower() == requestModel.ShortName.ToLower() && d.DatasetType == DataEntityCodes.DATASET && d.NamedEnvironment == requestModel.NamedEnvironment))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), "Short name is already in use by another Dataset for the named environment");
                }
            }

            if (!ValidationHelper.IsDSCEmailValid(requestModel.AlternateContactEmail))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.AlternateContactEmail), "Must be valid sentry.com email address");
            }

            //validate category exists
            if (!string.IsNullOrWhiteSpace(requestModel.CategoryName) && !_datasetContext.Categories.Any(x => x.Name.ToLower() == requestModel.CategoryName.ToLower()))
            {
                List<string> categoryNames = _datasetContext.Categories.Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(requestModel.CategoryName), $"Must provide a valid value - {string.Join(" | ", categoryNames)}");
            }

            //validate contact id exists (async if possible to also perform SAID validation async)
            Task<Associate> associateTask = null;
            if (!string.IsNullOrWhiteSpace(requestModel.PrimaryContactId))
            {
                associateTask = _associateInfoProvider.GetActiveAssociateByIdAsync(requestModel.PrimaryContactId);
            }

            //check SAID asset
            Task<ValidationResults> validationResultsTask = null;
            if (!string.IsNullOrWhiteSpace(requestModel.SaidAssetCode) && !string.IsNullOrWhiteSpace(requestModel.NamedEnvironment) && Enum.TryParse(requestModel.NamedEnvironmentTypeCode, out NamedEnvironmentType namedEnvironmentType))
            {
                validationResultsTask = _quartermasterService.VerifyNamedEnvironmentAsync(requestModel.SaidAssetCode, requestModel.NamedEnvironment, namedEnvironmentType);
            }

            if (associateTask != null)
            {
                Associate existingAssociate = associateTask.Result;
                if (existingAssociate == null)
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.PrimaryContactId), $"Not an active associate");
                }
            }

            if (validationResultsTask != null)
            {
                ValidationResults validationResults = validationResultsTask.Result;
                if (!validationResults.IsValid())
                {
                    //loop over results and align properties with the validation error returned
                }
            }

            return validationResponse;
        }

        public ValidationResponseModel Validate(IRequestModel requestModel)
        {
            return Validate((AddDatasetRequestModel)requestModel);
        }
    }
}