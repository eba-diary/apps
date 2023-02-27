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

        public async Task<ConcurrentValidationResponse> ValidateAsync(AddDatasetRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel.Validate(x => x.DatasetName).Required().MaxLength(1024)
                .Validate(x => x.DatasetDescription).Required().MaxLength(4096)
                .Validate(x => x.ShortName).Required().MaxLength(12).RegularExpression("^[0-9a-zA-Z]*$", "Only alphanumeric characters are allowed")
                .Validate(x => x.SaidAssetCode).Required()
                .Validate(x => x.CategoryCode).Required()
                .Validate(x => x.OriginationCode).Required().EnumValue(typeof(DatasetOriginationCode))
                .Validate(x => x.DataClassificationTypeCode).Required().EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString())
                .Validate(x => x.OriginalCreator).Required().MaxLength(128)
                .Validate(x => x.NamedEnvironment).Required().RegularExpression("^[A-Z0-9]{1,10}$", "Must be alphanumeric, all caps, and less than 10 characters")
                .Validate(x => x.NamedEnvironmentTypeCode).Required().EnumValue(typeof(NamedEnvironmentType))
                .Validate(x => x.PrimaryContactId).Required()
                .Validate(x => x.UsageInformation).MaxLength(4096)
                .ValidationResponse;

            bool isValidNamedEnvironment = !validationResponse.HasValidationsFor(nameof(requestModel.NamedEnvironment));

            Task[] asyncValidations = new Task[]
            {
                requestModel.ValidateSaidEnvironmentAsync(_saidService, _quartermasterService, validationResponse),
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse),
                requestModel.ValidateAlternateContactEmailAsync(validationResponse)
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
                else if (isValidNamedEnvironment && _datasetContext.Datasets.Any(d => d.ShortName.ToLower() == requestModel.ShortName.ToLower() && d.DatasetType == DataEntityCodes.DATASET && d.NamedEnvironment == requestModel.NamedEnvironment))
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.ShortName), "Short name is already in use by another Dataset for the named environment");
                }
            }

            //category exists
            requestModel.ValidateCategoryCode(_datasetContext, validationResponse);

            await Task.WhenAll(asyncValidations);

            return validationResponse;
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((AddDatasetRequestModel)requestModel);
        }
    }
}