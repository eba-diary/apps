using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFileReplyMapping : ClassMapping<DatasetFileReply>
    {
        public DatasetFileReplyMapping()
        {
            this.Table("DatasetFileReply");
            this.Id(x => x.DatasetFileReplyId, (m) =>
            {
                m.Column("DatasetFileReply_Id");
                m.Generator(Generators.Identity);
            });
            this.Property((x) => x.DatasetFileId, (m) => m.Column("DatasetFile_Id"));
            this.Property((x) => x.SchemaID, (m) => m.Column("Schema_ID"));
            this.Property((x) => x.FileLocation, (m) => m.Column("FileLocation"));
            this.Property((x) => x.ReplayStatus, (m) => m.Column("ReplyStatus"));
            this.Property((x) => x.DatasetId, (m) => m.Column("Dataset_ID"));
        }
    }
}
