using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.API
{
    public abstract class BaseDatasetRequestValidator<T> : IRequestModelValidator<T> where T : BaseDatasetModel, IRequestModel
    {
        protected readonly IDatasetContext _datasetContext;

        protected BaseDatasetRequestValidator(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public virtual async Task<ConcurrentValidationResponse> ValidateAsync(T requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.DatasetDescription).MaxLength(4096)
                .Validate(x => x.OriginationCode).Required().EnumValue(typeof(DatasetOriginationCode))
                .Validate(x => x.OriginalCreator).MaxLength(128)
                .Validate(x => x.UsageInformation).MaxLength(4096)
                .ValidationResponse;

            Task[] asyncValidations = new Task[]
            {
                ValidateAlternateContactEmailAsync(requestModel.AlternateContactEmail, validationResponse)
            };

            await Task.WhenAll(asyncValidations);

            return validationResponse;
        }

        public abstract Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel);

        protected void AddCategoryCodeValidationMessage(ConcurrentValidationResponse validationResponse)
        {
            List<string> categoryNames = _datasetContext.Categories.Where(x => x.ObjectType == DataEntityCodes.DATASET).Select(x => x.Name).ToList();
            validationResponse.AddFieldValidation(nameof(BaseDatasetModel.CategoryCode), $"Must provide a valid value - {string.Join(" | ", categoryNames)}");
        }

        private async Task ValidateAlternateContactEmailAsync(string alternateContactEmail, ConcurrentValidationResponse validationResponse)
        {
            await Task.Run(() =>
            {
                //validate alternate email is sentry email
                if (!ValidationHelper.IsDSCEmailValid(alternateContactEmail))
                {
                    validationResponse.AddFieldValidation(nameof(BaseDatasetModel.AlternateContactEmail), "Must be valid sentry.com email address");
                }
            });
        }
    }
}