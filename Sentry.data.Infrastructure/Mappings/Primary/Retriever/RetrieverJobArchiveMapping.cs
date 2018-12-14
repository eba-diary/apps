using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
     public class RetrieverJobArchiveMapping : ClassMapping<RetrieverJobArchive>
    {
        public RetrieverJobArchiveMapping()
        {
            Table("RetrieverJobArchive");

            Id(x => x.Archive_Id, m =>
            {
                m.Column("Archive_Id");
                m.Generator(Generators.Identity);
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

            Property(x => x.JobOptions, m =>
            {
                m.Column("JobOptions");
                m.Access(Accessor.Field);

                //http://geekswithblogs.net/lszk/archive/2011/07/11/nhibernatemapping-a-string-field-as-nvarcharmax-in-sql-server-using.aspx
                m.Type(NHibernateUtil.StringClob);
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

        }
    }
}
