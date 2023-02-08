using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Sentry.data.Web
{
    public class AddSampleViewModelValidator : IViewModelValidator<AddSampleViewModel>
    {
        public void Validate(AddSampleViewModel viewModel)
        {
            List<ValidationResultViewModel> validationResults = new List<ValidationResultViewModel>();

            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                validationResults.Add(new ValidationResultViewModel() { InvalidField = "Name", ValidationMessages = new List<string> { "Name is required" } });
            }

            if (string.IsNullOrWhiteSpace(viewModel.Description))
            {
                validationResults.Add(new ValidationResultViewModel() { InvalidField = "Description", ValidationMessages = new List<string> { "Description is required" } });
            }

            if (string.IsNullOrWhiteSpace(viewModel.OriginalCreator))
            {
                validationResults.Add(new ValidationResultViewModel() { InvalidField = "Original Creator", ValidationMessages = new List<string> { "Original Creator is required" } });
            }

            if (validationResults.Any())
            {
                throw new ViewModelValidationException(validationResults);
            }
        }

        public void Validate(IRequestViewModel viewModel)
        {
            Validate((AddSampleViewModel)viewModel);
        }
    }
}