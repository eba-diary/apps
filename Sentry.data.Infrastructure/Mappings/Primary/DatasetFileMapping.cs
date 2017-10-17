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
    public class DatasetFileMapping : ClassMapping<DatasetFile>
    {
        public DatasetFileMapping()
        {
            Lazy(true);

            this.Table("DatasetFile");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.DatasetFileId, (m) =>
            {
                m.Column("DatasetFile_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.FileName, (m) => m.Column("File_NME"));
            //this.Property((x) => x.DatasetId, (m) => m.Column("Dataset_ID"));
            this.Property((x) => x.UploadUserName, (m) => m.Column("UploadUser_NME"));
            this.Property((x) => x.CreateDTM, (m) => m.Column("Create_DTM"));
            this.Property((x) => x.ModifiedDTM, (m) => m.Column("Modified_DTM"));
            this.Property((x) => x.FileLocation, (m) => m.Column("FileLocation"));
            this.Property((x) => x.ParentDatasetFileId, (m) => m.Column("ParentDatasetFile_ID"));
            this.Property((x) => x.VersionId, (m) => m.Column("Version_ID"));
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
            
        }

    }
}
