using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaTile
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string TileColor { get; set; }
        public virtual string ImageName { get; set; }
        public virtual string LinkText { get; set; }
        public virtual string Hyperlink { get; set; }
        public virtual int Sequence { get; set; }
    }
}