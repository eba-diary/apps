using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class UpdateSampleViewModelValidator : IViewModelValidator<UpdateSampleViewModel>
    {
        public void Validate(UpdateSampleViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Name) && string.IsNullOrWhiteSpace(viewModel.Description))
            {
                throw new ViewModelValidationException(new List<ValidationResultViewModel>() 
                { 
                    new ValidationResultViewModel
                    {
                        InvalidField = "Name/Description", ValidationMessages = new List<string> { "Name AND/OR Description is required" }
                    } 
                });
            }
        }

        public void Validate(IRequestViewModel viewModel)
        {
            Validate((UpdateSampleViewModel)viewModel);
        }
    }
}