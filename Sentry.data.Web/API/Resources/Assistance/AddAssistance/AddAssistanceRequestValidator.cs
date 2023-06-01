using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public class AddAssistanceRequestValidator : IRequestModelValidator<AddAssistanceRequestModel>
    {
        public Task<ConcurrentValidationResponse> ValidateAsync(AddAssistanceRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = requestModel
                .Validate(x => x.Summary).Required()
                .Validate(x => x.Description).Required()
                .Validate(x => x.ReporterAssociateId).Required()
                .ValidationResponse;

            return Task.FromResult(validationResponse);
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((AddAssistanceRequestModel)requestModel);
        }
    }
}