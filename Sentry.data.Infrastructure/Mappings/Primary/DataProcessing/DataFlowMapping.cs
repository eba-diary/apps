﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowMapping : ClassMapping<DataFlow>
    {
        public DataFlowMapping()
        {
            this.Table("DataFlow");

            this.Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.FlowGuid, m => m.Column("FlowGuid"));
            this.Property(x => x.SaidKeyCode, m => m.Column("SaidKeyCode"));
            this.Property(x => x.CreatedDTM, m => m.Column("Create_DTM"));
            this.Property(x => x.CreatedBy, m => m.Column("CreatedBy"));
            this.Property(x => x.Questionnaire, m => m.Column("Questionnaire"));
            this.Property(x => x.FlowStorageCode, m => m.Column("FlowStorageCode"));
            this.Property(x => x.ObjectStatus, m => m.Column("ObjectStatus"));
            this.Property(x => x.DeleteIssuer, m => m.Column("DeleteIssuer"));
            this.Property(x => x.DeleteIssueDTM, m => m.Column("DeleteIssueDTM"));
            this.Property(x => x.IngestionType, m => m.Column("IngestionType"));

            this.Bag(x => x.Steps, (m) =>
            {
                m.Inverse(true);
                m.Table("DataFlowStep");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("DataFlow_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataFlowStep))));

            this.Bag(x => x.Logs, (m) =>
            {
                m.Inverse(true);
                m.Table("DataFlowLog");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("DataFlow_Id");
                });
            }, map => map.OneToMany(a => a.Class(typeof(EventMetric))));
        }
    }
}
