using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public class IndexGlobalDatasetsRequestValidator : IRequestModelValidator<IndexGlobalDatasetsRequestModel>
    {
        public Task<ConcurrentValidationResponse> ValidateAsync(IndexGlobalDatasetsRequestModel requestModel)
        {
            ConcurrentValidationResponse validationResponse = new ConcurrentValidationResponse();

            if (requestModel.IndexAll)
            {
                if (requestModel.GlobalDatasetIds?.Any() == true)
                {
                    validationResponse.AddFieldValidation(nameof(requestModel.GlobalDatasetIds), $"Value(s) not accepted when {nameof(requestModel.IndexAll)} set to true");
                }
            }
            else
            {
                validationResponse = requestModel.Validate(x => x.GlobalDatasetIds).Required().MaxLength(20).ValidationResponse;
            }

            return Task.FromResult(validationResponse);
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((IndexGlobalDatasetsRequestModel)requestModel);
        }
    }
}