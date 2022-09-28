using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using NHibernate;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class RetrieverJobMapping : ClassMapping<RetrieverJob>
    {
        public RetrieverJobMapping()
        {
            Table("RetrieverJob");

            Id(x => x.Id, m =>
            {
                m.Column("Job_ID");
                m.Generator(Generators.Identity);
            });

            Property(x => x.JobGuid, m =>
            {
                m.Column("Job_Guid");
                m.Type(NHibernateUtil.Guid);
            });

            Property(x => x.RelativeUri, m =>
            {
                m.Column("RelativeUri_DSC");
                m.NotNullable(false);
            });

            Property(x => x.Schedule, m =>
            {
                m.Column("Schedule");
                m.NotNullable(false);
            });

            Property(x => x.Created, m =>
            {
                m.Column("Created_DTM");
                m.NotNullable(false);
            });

            Property(x => x.Modified, m =>
            {
                m.Column("Modified_DTM");
                m.NotNullable(false);
            });
            
            this.Property((x) => x.IsGeneric, (m) => m.Column("IsGeneric_IND"));
            this.Property((x) => x.IsEnabled, (m) => m.Column("IsEnabled"));
            this.Property((x) => x.ObjectStatus, (m) => m.Column("ObjectStatus"));
            this.Property((x) => x.DeleteIssueDTM, (m) => m.Column("DeleteIssueDTM"));
            this.Property((x) => x.DeleteIssuer, (m) => m.Column("DeleteIssuer"));

            Property(x => x.JobOptions, m =>
            {
                m.Column("JobOptions");
                m.Access(Accessor.Field);

                //http://geekswithblogs.net/lszk/archive/2011/07/11/nhibernatemapping-a-string-field-as-nvarcharmax-in-sql-server-using.aspx
                m.Type(NHibernateUtil.StringClob);
            });

            ManyToOne(x => x.FileSchema, m =>
            {
                m.Column("Schema_Id");
                m.Class(typeof(FileSchema));
            });

            ManyToOne(x => x.DataFlow, m =>
            {
                m.Column("DataFlow_ID");
                m.Class(typeof(DataFlow));
            });

            ManyToOne(x => x.DataSource, m =>
            {
                m.Column("DataSource_ID");
                m.ForeignKey("FK_RetrieverJob_DataSource");
                m.Class(typeof(DataSource));
            });

            ManyToOne(x => x.DatasetConfig, m =>
            {
                m.Column("Config_ID");
                m.ForeignKey("FK_RetrieverJob_DatasetFileConfigs");
                m.Class(typeof(DatasetFileConfig));
            });

            this.Bag(x => x.JobHistory, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("JobHistory");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Job_ID");
                    k.ForeignKey("FK_JobHistory_RetrieverJob");
                });
            }, map => map.OneToMany(a => a.Class(typeof(JobHistory))));

            this.Bag(x => x.Submissions, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("Submission");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Job_ID");
                    k.ForeignKey("FK_Submission_RetrieverJob");
                });
            }, map => map.OneToMany(a => a.Class(typeof(Submission))));

            Property(x => x.ExecutionParameters, m =>
            {
                m.Column("ExecutionParameters");
                m.Access(Accessor.Field);
                m.Type(NHibernateUtil.StringClob);
            });
        }
    }
}
