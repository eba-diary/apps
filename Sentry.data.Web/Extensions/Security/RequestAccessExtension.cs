
namespace Sentry.data.Web
{
    public static class RequestAccessExtension
    {

        public static Core.RequestAccess ToCore(this RequestAccessModel model)
        {
            return new Core.RequestAccess()
            {
                DatasetId = model.DatasetId,
                AdGroupName = model.AdGroupName,
                BusinessReason = model.BusinessReason,
                RequestorsId = model.RequestorsId
            };
        }

    }
}