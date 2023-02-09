using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Sentry.data.Web
{
    public class AddSampleViewModelValidator : IRequestModelValidator<AddSampleViewModel>
    {
        public ValidationResponseModel Validate(AddSampleViewModel viewModel)
        {
            ValidationResponseModel validationResponse = new ValidationResponseModel();

            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                validationResponse.AddFieldValidation("Name" ,"Name is required");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Description))
            {
                validationResponse.AddFieldValidation("Description", "Description is required");
            }

            if (string.IsNullOrWhiteSpace(viewModel.OriginalCreator))
            {
                validationResponse.AddFieldValidation("Original Creator", "Original Creator is required");
            }

            return validationResponse;
        }

        public ValidationResponseModel Validate(IRequestModel viewModel)
        {
            return Validate((AddSampleViewModel)viewModel);
        }
    }
}