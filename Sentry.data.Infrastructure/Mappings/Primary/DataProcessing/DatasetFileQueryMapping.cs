using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFileQueryMapping : ClassMapping<DatasetFileQuery>
    {
        public DatasetFileQueryMapping()
        {
            this.Table("DatasetFileQuery");

            this.Id(x => x.DatasetFileQueryID);

            ///Properties from abstract class <see cref="Sentry.data.Core.Entities.DataProcessing.BaseDatasetFileV2"/>
            this.Property(x => x.FileNME);
            this.Property(x => x.FlowExecutionGUID);
            this.Property(x => x.ObjectBucket);
            this.Property(x => x.ObjectKey);
            this.Property(x => x.ObjectVersionID);
            this.Property(x => x.ObjectETag);
            this.Property(x => x.DatasetID);
            this.Property(x => x.SchemaId);
            this.Property(x => x.CreateDTM);

            ///Add properties specific to this class
            this.Property(x => x.DatasetFileDropID);
            this.Property(x => x.ObjectStatus);
            this.Property(x => x.RunInstanceGUID);
            this.Property(x => x.UpdateDTM);
        }
    }
}
