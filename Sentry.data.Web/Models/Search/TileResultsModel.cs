using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class TileResultsModel
    {
        public List<SelectListItem> PageSizeOptions { get; set; }
        public List<SelectListItem> SortByOptions { get; set; }
        public List<TileModel> Tiles { get; set;}
    }
}