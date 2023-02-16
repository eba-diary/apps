using System;

namespace Sentry.data.Web.API
{
    public class AddDatasetRequestValidator : IRequestModelValidator<AddDatasetRequestModel>
    {
        public ValidationResponseModel Validate(AddDatasetRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public ValidationResponseModel Validate(IRequestModel requestModel)
        {
            return Validate((AddDatasetRequestModel)requestModel);
        }
    }
}