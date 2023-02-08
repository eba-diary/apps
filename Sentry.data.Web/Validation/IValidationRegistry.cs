namespace Sentry.data.Web
{
    public interface IValidationRegistry
    {
        bool TryGetValidatorFor<T>(out IViewModelValidator validator) where T : IRequestViewModel;
    }
}
