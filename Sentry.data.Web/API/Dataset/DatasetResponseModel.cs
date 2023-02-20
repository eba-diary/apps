using System;

namespace Sentry.data.Web.API
{
    public abstract class DatasetResponseModel : DatasetModel, IResponseModel
    {
        public int DatasetId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string ObjectStatusCode { get; set; }
    }
}