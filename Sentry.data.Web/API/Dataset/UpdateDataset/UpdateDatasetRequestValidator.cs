using System;

namespace Sentry.data.Web.API
{
    public class UpdateDatasetRequestValidator : IRequestModelValidator<UpdateDatasetRequestModel>
    {
        public ValidationResponseModel Validate(UpdateDatasetRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public ValidationResponseModel Validate(IRequestModel requestModel)
        {
            return Validate((UpdateDatasetRequestModel)requestModel);
        }
    }
}