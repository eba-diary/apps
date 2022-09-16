using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode;
using NHibernate;

namespace Sentry.data.Infrastructure.Mappings.Primary
{ 
    public class SubmissionMapping : ClassMapping<Submission>
    {
        public SubmissionMapping()
        {
            Table("Submission");

            Id(x => x.SubmissionId, m =>
            {
                m.Column("Submission_ID");
                m.Generator(Generators.Identity);
            });

            Property(x => x.JobGuid, m =>
            {
                m.Column("Job_Guid");
                m.NotNullable(true);
            });

            Property(x => x.Serialized_Job_Options, m =>
            {
                m.Column("Serialized_Job_Options");
                m.Type(NHibernateUtil.StringClob);
                m.NotNullable(true);
            });

            Property(x => x.Created, m =>
            {
                m.Column("Created");
                m.NotNullable(true);
            });

            Property(x => x.FlowExecutionGuid);
            Property(x => x.RunInstanceGuid);
            Property(x => x.ClusterUrl);

            this.ManyToOne(x => x.JobId, m =>
            {
                m.Column("Job_ID");
                m.ForeignKey("FK_JobHistory_RetrieverJob");
                m.Class(typeof(RetrieverJob));
            });
        }
    }
}