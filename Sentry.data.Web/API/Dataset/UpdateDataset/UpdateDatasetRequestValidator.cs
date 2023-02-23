using System;
using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : IRequestModelValidator<UpdateDatasetRequestModel>
    {
        public async Task<ConcurrentValidationResponse> Validate(UpdateDatasetRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ConcurrentValidationResponse> Validate(IRequestModel requestModel)
        {
            return await Validate((UpdateDatasetRequestModel)requestModel);
        }
    }
}