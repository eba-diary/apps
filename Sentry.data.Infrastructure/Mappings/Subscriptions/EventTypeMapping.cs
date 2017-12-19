﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class EventTypeMapping : ClassMapping<EventType>
    {

        public EventTypeMapping()
        {

            this.Table("EventType");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.Type_ID, (m) =>
            {
                m.Column("Type_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Severity, (m) => m.Column("Severity"));
            this.Property((x) => x.Description, (m) => m.Column("Description"));
        }
    }
}