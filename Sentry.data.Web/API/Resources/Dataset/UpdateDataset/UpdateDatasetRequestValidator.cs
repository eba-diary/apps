using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : IRequestModelValidator<UpdateDatasetRequestModel>
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IAssociateInfoProvider _associateInfoProvider;

        public UpdateDatasetRequestValidator(IDatasetContext datasetContext, IAssociateInfoProvider associateInfoProvider) 
        {
            _datasetContext = datasetContext;
            _associateInfoProvider = associateInfoProvider;
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(UpdateDatasetRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.DatasetDescription).MaxLength(4096)
                .Validate(x => x.OriginationCode).Required().EnumValue(typeof(DatasetOriginationCode))
                .Validate(x => x.DataClassificationTypeCode).EnumValue(typeof(DataClassificationType), DataClassificationType.None.ToString())
                .Validate(x => x.OriginalCreator).MaxLength(128)
                .Validate(x => x.UsageInformation).MaxLength(4096)
                .ValidationResponse;

            List<Task> asyncValidations = new List<Task>
            {
                requestModel.ValidatePrimaryContactIdAsync(_associateInfoProvider, validationResponse),
                requestModel.ValidateAlternateContactEmailAsync(validationResponse)
            };

            requestModel.ValidateCategoryCode(_datasetContext, validationResponse);

            await Task.WhenAll(asyncValidations.ToArray());

            return validationResponse;
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((UpdateDatasetRequestModel)requestModel);
        }
    }
}