using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode.Impl;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataLineageMapping : ClassMapping<Lineage>
    {
        public DataLineageMapping()
        {

        }
    }
}
