using Sentry.Associates;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
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
        private readonly ISAIDService _saidService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public AddDatasetRequestValidator(IDatasetContext datasetContext, ISAIDService saidService, IQuartermasterService quartermasterService, IAssociateInfoProvider associateInfoProvider)
        {
            _datasetContext = datasetContext;
            _saidService = saidService;
            _quartermasterService = quartermasterService;
            _associateInfoProvider = associateInfoProvider;
        }

        public ConcurrentValidationResponse Validate(AddDatasetRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel.Validate(x => x.DatasetName).Required().MaxLength(1024)
                .Validate(x => x.DatasetDescription).Required().MaxLength(4096)
                .Validate(x => x.ShortName).Required().MaxLength(12).RegularExpression("^[0-9a-zA-Z]*$", "Only alphanumeric characters are allowed")
                .Validate(x => x.SaidAssetCode).Required()
                .Validate(x => x.CategoryName).Required()
                .Validate(x => x.OriginationCode).EnumValue(typeof(DatasetOriginationCode))
                .Validate(x => x.DataClassificationTypeCode).EnumValue(typeof(DataClassificationType))
                .Validate(x => x.OriginalCreator).Required().MaxLength(128)
                .Validate(x => x.NamedEnvironment).Required().RegularExpression("^[A-Z0-9]{1,10}$", "Must be alphanumeric, all caps, and less than 10 characters")
                .Validate(x => x.NamedEnvironmentTypeCode).EnumValue(typeof(NamedEnvironmentType))
                .Validate(x => x.PrimaryContactId).Required()
                .Validate(x => x.UsageInformation).MaxLength(4096)
                .ValidationResponse;

            bool isValidNamedEnvironment = !validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment));

            List<Task> asyncValidations = new List<Task>
            {
                ValidatePrimaryContactIdAsync(requestModel.PrimaryContactId, validationResponse),
                ValidateSaidAssetCodeNamedEnvironmentAsync(requestModel, validationResponse)
            };

            //dataset name exists
            if (!validationResponse.HasValidationsFor(nameof(requestModel.DatasetName)) && isValidNamedEnvironment &&
                _datasetContext.Datasets.Any(w => w.DatasetName.ToLower() == requestModel.DatasetName.ToLower() && w.DatasetType == DataEntityCodes.DATASET && w.NamedEnvironment == requestModel.NamedEnvironment))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.DatasetName), "Dataset name already exists for the named environment");
            }

            if (!validationResponse.HasValidationsFor(nameof(requestModel.ShortName)))
            {
                //short name is 'Default'
                if (string.Equals(requestModel.ShortName, SecurityConstants.ASSET_LEVEL_GROUP_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), $"Short name cannot be '{SecurityConstants.ASSET_LEVEL_GROUP_NAME}'");
                }

                //short name exists
                if (isValidNamedEnvironment && _datasetContext.Datasets.Any(d => d.ShortName.ToLower() == requestModel.ShortName.ToLower() && d.DatasetType == DataEntityCodes.DATASET && d.NamedEnvironment == requestModel.NamedEnvironment))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), "Short name is already in use by another Dataset for the named environment");
                }
            }

            //validate alternate email is sentry email
            if (!ValidationHelper.IsDSCEmailValid(requestModel.AlternateContactEmail))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.AlternateContactEmail), "Must be valid sentry.com email address");
            }

            //category exists
            if (validationResponse.HasValidationsFor(nameof(requestModel.CategoryName)) || !_datasetContext.Categories.Any(x => x.Name.ToLower() == requestModel.CategoryName.ToLower() && x.ObjectType == DataEntityCodes.DATASET))
            {
                List<string> categoryNames = _datasetContext.Categories.Where(x => x.ObjectType == DataEntityCodes.DATASET).Select(x => x.Name).ToList();
                validationResponse.AddFieldValidation(nameof(requestModel.CategoryName), $"Must provide a valid value - {string.Join(" | ", categoryNames)}");
            }

            Task.WaitAll(asyncValidations.ToArray());

            return validationResponse;
        }

        public ConcurrentValidationResponse Validate(IRequestModel requestModel)
        {
            return Validate((AddDatasetRequestModel)requestModel);
        }

        #region Private
        private async Task ValidateSaidAssetCodeNamedEnvironmentAsync(AddDatasetRequestModel requestModel, ConcurrentValidationResponse validationResponse)
        {
            if (!validationResponse.HasValidationsFor(nameof(requestModel.SaidAssetCode)))
            {
                if (await _saidService.VerifyAssetExistsAsync(requestModel.SaidAssetCode).ConfigureAwait(false))
                {
                    if (!validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment)) && Enum.TryParse(requestModel.NamedEnvironmentTypeCode, true, out NamedEnvironmentType namedEnvironmentType))
                    {
                        ValidationResults validationResults = await _quartermasterService.VerifyNamedEnvironmentAsync(requestModel.SaidAssetCode, requestModel.NamedEnvironment, namedEnvironmentType);

                        if (!validationResults.IsValid())
                        {
                            //loop over results and align properties with the validation error returned
                            foreach (ValidationResult result in validationResults.GetAll())
                            {
                                switch (result.Id)
                                {
                                    case ValidationErrors.NAMED_ENVIRONMENT_INVALID:
                                        validationResponse.AddFieldValidation(nameof(requestModel.NamedEnvironment), result.Description);
                                        break;
                                    case ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID:
                                        validationResponse.AddFieldValidation(nameof(requestModel.NamedEnvironmentTypeCode), result.Description);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.SaidAssetCode), "Must be a valid SAID asset code");
                }                
            }
        }

        private async Task ValidatePrimaryContactIdAsync(string contactId, ConcurrentValidationResponse validationResponse)
        {
            if (!validationResponse.HasValidationsFor(nameof(AddDatasetRequestModel.PrimaryContactId)))
            {
                Associate associate = await _associateInfoProvider.GetActiveAssociateByIdAsync(contactId);

                if (associate == null)
                {
                    validationResponse.AddFieldValidation(nameof(AddDatasetRequestModel.PrimaryContactId), "Must be a valid active associate");
                }
            }
        }
	    #endregion
    }
}