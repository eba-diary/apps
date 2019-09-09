using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFileParquetMapping : ClassMapping<DatasetFileParquet>
    {
        public DatasetFileParquetMapping()
        {
            this.Table("DatasetFileParquet");
            this.Id(x => x.DatsetFileParquetId, (m) =>
            {
                m.Column("DatasetFileParquet_Id");
                m.Generator(Generators.Identity);
            });
            this.Property((x) => x.DatasetFileId, (m) => m.Column("DatasetFile_Id"));
            this.Property((x) => x.SchemaId, (m) => m.Column("Schema_ID"));
            this.Property((x) => x.FileLocation, (m) => m.Column("FileLocation"));
        }
    }
}
