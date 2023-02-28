using System;

namespace Sentry.data.Web.API
{
    public abstract class BaseDatasetResponseModel : BaseImmutableDatasetModel, IResponseModel
    {
        public int DatasetId { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public string ObjectStatusCode { get; set; }
    }
}