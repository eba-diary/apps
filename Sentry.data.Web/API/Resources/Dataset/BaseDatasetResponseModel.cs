using System;

namespace Sentry.data.Web.API
{
    public abstract class BaseDatasetResponseModel : BaseImmutableDatasetModel, IResponseModel
    {
        public int DatasetId { get; set; }
        public DateTimeOffset CreateDateTime { get; set; }
        public DateTimeOffset UpdateDateTime { get; set; }
        public string ObjectStatusCode { get; set; }
    }
}