using Sentry.data.Core;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class AddDatasetRequestValidator : IRequestModelValidator<AddDatasetRequestModel>
    {
        private readonly IDatasetContext _datasetContext;

        public AddDatasetRequestValidator(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public ValidationResponseModel Validate(AddDatasetRequestModel requestModel)
        {
            ValidationResponseModel validationResponse = new ValidationResponseModel();

            validationResponse.Requires(() => requestModel.DatasetName)
                .Requires(() => requestModel.DatasetDescription)
                .Requires(() => requestModel.ShortName)
                .MaxLength(() => requestModel.ShortName, 12)
                .RegularExpression(() => requestModel.ShortName, "^[0-9a-zA-Z]*$", "Only alphanumeric characters are allowed")
                .Requires(() => requestModel.SaidAssetCode)
                .Requires(() => requestModel.CategoryName)
                .Requires(() => requestModel.OriginationCode)
                .Requires(() => requestModel.DataClassificationTypeCode)
                .Requires(() => requestModel.OriginalCreator)
                .Requires(() => requestModel.NamedEnvironment)
                .RegularExpression(() => requestModel.NamedEnvironment, "^[A-Z0-9]{1,10}$", "Must be alphanumeric, all caps, and less than 10 characters")
                .Requires(() => requestModel.NamedEnvironmentTypeCode)
                .MaxLength(() => requestModel.UsageInformation, 4096);

            //dataset name exists
            if (!string.IsNullOrWhiteSpace(requestModel.DatasetName) && !string.IsNullOrWhiteSpace(requestModel.NamedEnvironment) &&
                _datasetContext.Datasets.Any(w => w.DatasetName.ToLower() == requestModel.DatasetName.ToLower() && w.DatasetType == DataEntityCodes.DATASET && w.NamedEnvironment == requestModel.NamedEnvironment))
            {
                validationResponse.AddFieldValidation(nameof(requestModel.DatasetName), "Dataset name already exists for the named environment");
            }

            //check code values
            //data classification valid value
            //dataset origination code valid value
            //named environment type valid value

            //check SAID asset
            //look up asset
            //verify the environment is valid for the asset and environment type matches

            return validationResponse;
        }

        public ValidationResponseModel Validate(IRequestModel requestModel)
        {
            return Validate((AddDatasetRequestModel)requestModel);
        }
    }
}