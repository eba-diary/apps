using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : BaseDatasetRequestValidator<UpdateDatasetRequestModel>
    {
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public UpdateDatasetRequestValidator(IDatasetContext datasetContext, IAssociateInfoProvider associateInfoProvider) : base(datasetContext)
        {
            _associateInfoProvider = associateInfoProvider;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(UpdateDatasetRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.DataClassificationTypeCode).EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString())
                .ValidationResponse;

            List<Task> asyncValidations = new List<Task>
            {
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse)
            };

            Task<ConcurrentValidationResponse> baseValidations = base.ValidateAsync(requestModel);

            //category exists
            if (!string.IsNullOrEmpty(requestModel.CategoryCode) && !_datasetContext.Categories.Any(x => x.Name.ToLower() == requestModel.CategoryCode.ToLower() && x.ObjectType == DataEntityCodes.DATASET))
            {
                AddCategoryCodeValidationMessage(validationResponse);
            }

            validationResponse.AddValidationsFrom(await baseValidations);
            await Task.WhenAll(asyncValidations.ToArray());

            return validationResponse;
        }

        public override async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((UpdateDatasetRequestModel)requestModel);
        }
    }
}