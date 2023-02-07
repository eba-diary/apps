namespace Sentry.data.Core
{
    public interface IIdentifiableDto : IValidatableDto
    {
        void SetId(int id);
    }
}
