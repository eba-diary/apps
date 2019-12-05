using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaTileDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TileColor { get; set; }
        public string ImageName { get; set; }
        public string LinkText { get; set; }
        public string Hyperlink { get; set; }
        public int Sequence { get; set; }
    }
}