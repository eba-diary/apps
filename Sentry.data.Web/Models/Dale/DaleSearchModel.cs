using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public class DaleSearchModel
    {
        public string Criteria { get; set; }

        public DaleDestiny Destiny { get; set; }

        public DaleSensitive Sensitive { get; set; }

        public bool CanDaleSensitiveView { get; set; }

        public bool CanDaleSensitiveEdit { get; set; }

        public DaleResultModel DaleResultModel { get; set; }

        public DaleAdvancedCriteria DaleAdvancedCriteria { get; set; }

    }
}
