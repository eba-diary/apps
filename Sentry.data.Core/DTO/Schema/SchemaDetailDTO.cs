﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaDetailDTO : SchemaDTO
    {
        public List<SchemaRow> Rows { get; set; }
    }
}
