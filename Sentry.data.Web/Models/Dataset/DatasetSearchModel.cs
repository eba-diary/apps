using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class TileSearchModel : TileSearchEventModel
    {
        public List<TileModel> SearchableTiles { get; set; }
    }
}