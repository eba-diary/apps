namespace Sentry.data.Web
{
    public interface IValidationRegistry
    {
        bool TryGetValidatorFor<T>(out IRequestModelValidator validator) where T : IRequestModel;
    }
}
