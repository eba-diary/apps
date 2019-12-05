using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class BusinessAreaTileRowModel
    {
        public int ColumnSpan { get; set; }
        public int Sequence { get; set; }
        public List<BusinessAreaTileModel> Tiles { get; set; }
    }
}