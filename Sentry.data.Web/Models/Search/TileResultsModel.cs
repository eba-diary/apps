using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class TileResultsModel
    {
        public List<SelectListItem> PageSizeOptions { get; set; }
        public List<SelectListItem> SortByOptions { get; set; }
        public List<SelectListItem> LayoutOptions { get; set; }
        public List<TileModel> Tiles { get; set;}
        public List<PageItemModel> PageItems { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; }
        public int TotalResults { get; set; }
    }
}