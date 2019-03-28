using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class FavoritesDetailModel
    {
        public List<FavoriteItemDetailModel> Favorites { get; set; }
        public string MailToAllLink { get; set; }
    }
}