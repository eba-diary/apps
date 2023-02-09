namespace Sentry.data.Web
{
    public interface IRequestModelValidator<in T> : IRequestModelValidator where T : IRequestModel
    {
        ValidationResponseModel Validate(T viewModel);
    }

    public interface IRequestModelValidator
    {
        ValidationResponseModel Validate(IRequestModel viewModel);
    }
}
