namespace Sentry.data.Web.API
{
    public abstract class DatasetResponseModel : DatasetModel, IResponseModel
    {
        public int DatasetId { get; set; }
    }
}