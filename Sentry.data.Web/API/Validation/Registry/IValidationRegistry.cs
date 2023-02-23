namespace Sentry.data.Web.API
{
    public interface IValidationRegistry
    {
        bool TryGetValidatorFor<T>(out IRequestModelValidator validator) where T : IRequestModel;
    }
}
