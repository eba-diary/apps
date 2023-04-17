using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sentry.data.Web.API
{
    public class SearchGlobalDatasetsRequestValidator : IRequestModelValidator<SearchGlobalDatasetsRequestModel>
    {
        public Task<ConcurrentValidationResponse> ValidateAsync(SearchGlobalDatasetsRequestModel requestModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel)
        {
            return await ValidateAsync((SearchGlobalDatasetsRequestModel)requestModel);
        }
    }
}