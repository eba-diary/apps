namespace Sentry.data.Web.API
{
    public interface IRequestModelValidator<in T> : IRequestModelValidator where T : IRequestModel
    {
        ValidationResponseModel Validate(T requestModel);
    }

    public interface IRequestModelValidator
    {
        ValidationResponseModel Validate(IRequestModel requestModel);
    }
}
