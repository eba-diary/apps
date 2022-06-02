using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web.Models.ApiModels.Dataset
{
    public class DatasetInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool IsSecure { get; set; }
        public string PrimaryContactName { get; set; }
        public string PrimarContactEmail { get; set; }
        public string Category { get; set; }
        public string ObjectStatus { get; set; }
        public virtual string SAIDAssetKeyCode { get; set; }
        public virtual string NamedEnvironment { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual NamedEnvironmentType NamedEnvironmentType { get; set; }
    }
}