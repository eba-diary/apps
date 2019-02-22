using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaTileRow
    {
        public virtual int Id { get; set; }
        public virtual int ColumnSpan { get; set; }
        public virtual BusinessAreaType BusinessAreaType { get; set; }
        public virtual int Sequence { get; set; }

        public virtual IList<BusinessAreaTile> Tiles { get; set; }
    }
}