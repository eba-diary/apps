﻿using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataElementDetailMapping : ClassMapping<DataElementDetail>
    {
        public DataElementDetailMapping()
        {
            this.Table("DataElementDetail");

            this.Id(x => x.DataElementDetail_ID, m => m.Generator(Generators.Identity));

            this.Property(x => x.DataElementDetailCreate_DTM);
            this.Property(x => x.DataElementDetailChange_DTM);
            this.Property(x => x.DataElementDetailType_CDE);
            this.Property(x => x.DataElementDetailType_VAL);
            this.Property(x => x.LastUpdt_DTM);
            this.Property(x => x.BusElementKey);

        }
    }
}