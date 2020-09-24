using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleCategoryResultModel
    {
        public List<DaleCategoryModel> DaleCategories { get; set; }
        public DaleEventDto DaleEvent { get; set; }
    }
}
