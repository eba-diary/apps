namespace Sentry.data.Web.API
{
    public interface IRequestModelValidator<in T> : IRequestModelValidator where T : IRequestModel
    {
        ConcurrentValidationResponse Validate(T requestModel);
    }

    public interface IRequestModelValidator
    {
        ConcurrentValidationResponse Validate(IRequestModel requestModel);
    }
}
