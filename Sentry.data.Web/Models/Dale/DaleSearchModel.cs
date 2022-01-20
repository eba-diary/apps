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

        public DaleAdvancedCriteriaModel DaleAdvancedCriteria { get; set; }

        public bool CLA3550_DATA_INVENTORY_NEW_COLUMNS { get; set; }
        public bool CLA3707_UsingSQLSource { get; set; }
    }
}
