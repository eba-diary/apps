using System;
using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : IRequestModelValidator<UpdateDatasetRequestModel>
    {
        public async Task<ConcurrentValidationResponse> ValidateAsync(UpdateDatasetRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((UpdateDatasetRequestModel)requestModel);
        }
    }
}