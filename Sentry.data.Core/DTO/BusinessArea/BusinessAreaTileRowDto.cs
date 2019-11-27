using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaTileRowDto
    {
        public int Id { get; set; }
        public int ColumnSpan { get; set; }
        public int Sequence { get; set; }

        public List<BusinessAreaTileDto> Tiles { get; set; }
    }
}