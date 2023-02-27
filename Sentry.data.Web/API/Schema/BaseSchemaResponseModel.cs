using System;

namespace Sentry.data.Web.API
{
    public abstract class BaseSchemaResponseModel : BaseImmutableSchemaModel, IResponseModel
    {
        public int SchemaId { get; set; }
        public string StorageCode { get; set; }
        public string DropLocation { get; set; }
        public string ControlMTriggerName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string ObjectStatusCode { get; set; }
    }
}