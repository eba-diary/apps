using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class JobHistoryMapping : ClassMapping<JobHistory>
    {
        public JobHistoryMapping()
        {
            Table("JobHistory");

            Id(x => x.HistoryId, m =>
            {
                m.Column("History_Id");
                m.Generator(Generators.Identity);
            });

            Property(x => x.State, m =>
            {
                m.Column("State");
                m.NotNullable(true);
            });

            Property(x => x.LivyAppId, m =>
            {
                m.Column("LivyAppId");
                m.NotNullable(false);
            });

            Property(x => x.LivyDriverLogUrl, m =>
            {
                m.Column("LivyDriverLogUrl");
                m.NotNullable(false);
            });

            Property(x => x.LivySparkUiUrl, m =>
            {
                m.Column("LivySparkUiUrl");
                m.NotNullable(false);
            });

            Property(x => x.LogInfo, m =>
            {
                m.Column("LogInfo");
                m.NotNullable(false);
            });

            Property(x => x.Created, m =>
            {
                m.Column("Created");
                m.NotNullable(false);
            });

            Property(x => x.Modified, m =>
            {
                m.Column("Modified");
                m.NotNullable(false);
            });

            Property(x => x.BatchId, m =>
            {
                m.Column("BatchId");
                m.NotNullable(true);
            });

            Property(x => x.Active, m =>
            {
                m.Column("ActiveInd");
                m.NotNullable(true);
            });

            Property(x => x.JobGuid, m =>
            {
                m.Column("Job_Guid");
                m.NotNullable(true);
            });

            Property(x => x.ClusterUrl);

            this.ManyToOne(x => x.JobId, m =>
            {
                m.Column("Job_ID");
                m.ForeignKey("FK_JobHistory_RetrieverJob");
                m.Class(typeof(RetrieverJob));
            });

            this.ManyToOne(x => x.Submission, m =>
            {
                m.Column("Submission");
                m.Class(typeof(Submission));
            });
        }
    }
}
