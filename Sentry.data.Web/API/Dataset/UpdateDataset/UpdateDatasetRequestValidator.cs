using System;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : IRequestModelValidator<UpdateDatasetRequestModel>
    {
        public ConcurrentValidationResponse Validate(UpdateDatasetRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public ConcurrentValidationResponse Validate(IRequestModel requestModel)
        {
            return Validate((UpdateDatasetRequestModel)requestModel);
        }
    }
}