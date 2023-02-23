using System.Threading.Tasks;

namespace Sentry.data.Web.API
{
    public interface IRequestModelValidator<in T> : IRequestModelValidator where T : IRequestModel
    {
        Task<ConcurrentValidationResponse> ValidateAsync(T requestModel);
    }

    public interface IRequestModelValidator
    {
        Task<ConcurrentValidationResponse> ValidateAsync(IRequestModel requestModel);
    }
}
