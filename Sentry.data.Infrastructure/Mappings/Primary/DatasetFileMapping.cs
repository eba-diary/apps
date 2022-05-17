using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFileMapping : ClassMapping<DatasetFile>
    {
        public DatasetFileMapping()
        {
            Lazy(true);

            this.Table("DatasetFile");

            this.Id((x) => x.DatasetFileId, (m) =>
            {
                m.Column("DatasetFile_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.FileName, (m) => m.Column("File_NME"));
            this.Property((x) => x.UploadUserName, (m) => m.Column("UploadUser_NME"));
            this.Property((x) => x.CreatedDTM, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.ModifiedDTM, (m) => m.Column("Modified_DTM"));
            this.Property((x) => x.FileLocation, (m) => m.Column("FileLocation"));
            this.Property((x) => x.ParentDatasetFileId, (m) => m.Column("ParentDatasetFile_ID"));
            this.Property((x) => x.VersionId, (m) => m.Column("Version_ID"));
            this.Property((x) => x.IsBundled, (m) => m.Column("isBundled_IND"));
            this.Property((x) => x.Information, (m) => m.Column("Information_DSC"));
            this.Property((x) => x.Size, (m) => m.Column("Size_AMT"));
            this.Property((x) => x.FlowExecutionGuid, (m) => m.Column("FlowExecutionGuid"));
            this.Property((x) => x.RunInstanceGuid, (m) => m.Column("RunInstanceGuid"));
            this.Property((x) => x.FileBucket);
            this.Property((x) => x.FileKey);
            this.Property((x) => x.ETag);
            this.Property((x) => x.ObjectStatus);
            this.Property((x) => x.OriginalFileName);




            this.ManyToOne(x => x.Dataset, m =>
            {
                m.Column("Dataset_ID");
                m.ForeignKey("FK_DatasetFile_Dataset");
                m.Class(typeof(Dataset));
            });
            this.ManyToOne(x => x.DatasetFileConfig, m =>
            {
                m.Column("DatasetFileConfig_ID");
                m.ForeignKey("FK_DatasetFile_DatasetFileConfigs");
                m.Class(typeof(DatasetFileConfig));
            });
            this.ManyToOne(x => x.Schema, m =>
            {
                m.Column("Schema_ID");
                m.Class(typeof(FileSchema));
            });
            this.ManyToOne(x => x.SchemaRevision, m =>
            {
                m.Column("SchemaRevision_ID");
                m.Class(typeof(SchemaRevision));
            });
        }

    }
}
