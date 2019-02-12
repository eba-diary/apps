using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class SchemaDetailModel : SchemaModel
    {
        public SchemaDetailModel(SchemaDetailDTO dto) : base(dto)
        {
            Rows = dto.Rows;
        }

        public List<SchemaRow> Rows { get; set; }
    }
}