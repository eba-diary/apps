namespace Sentry.data.Web
{
    public interface IViewModelValidator<in T> : IViewModelValidator where T : IRequestViewModel
    {
        void Validate(T viewModel);
    }

    public interface IViewModelValidator
    {
        void Validate(IRequestViewModel viewModel);
    }
}
