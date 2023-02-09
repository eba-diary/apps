namespace Sentry.data.Web
{
    public class UpdateSampleViewModelValidator : IRequestModelValidator<UpdateSampleViewModel>
    {
        public ValidationResponseModel Validate(UpdateSampleViewModel viewModel)
        {
            ValidationResponseModel validationResponse = new ValidationResponseModel();

            if (string.IsNullOrWhiteSpace(viewModel.Name) && string.IsNullOrWhiteSpace(viewModel.Description))
            {
                validationResponse.AddFieldValidation("Name", "Name AND/OR Description is required");
                validationResponse.AddFieldValidation("Description", "Name AND/OR Description is required");
            }

            return validationResponse;
        }

        public ValidationResponseModel Validate(IRequestModel viewModel)
        {
            return Validate((UpdateSampleViewModel)viewModel);
        }
    }
}