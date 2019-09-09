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
    public class DatasetFileConfigsMapping : ClassMapping<DatasetFileConfig>
    {
        public DatasetFileConfigsMapping()
        {
            this.Table("DatasetFileConfigs");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ConfigId, (m) =>
            {
                m.Column("Config_ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.FileTypeId, (m) => m.Column("FileType_ID"));
            this.Property((x) => x.Name, (m) => m.Column("Config_NME"));
            this.Property((x) => x.Description, (m) => m.Column("Config_DSC"));
            this.Property((x) => x.DeleteInd, (m) => m.Column("DeleteInd"));
            this.Property((x) => x.DeleteIssuer, (m) => m.Column("DeleteIssuer"));
            this.Property((x) => x.DeleteIssueDTM, (m) => m.Column("DeleteIssueDTM"));

            this.ManyToOne(x => x.ParentDataset, m =>
            {
                m.Column("Dataset_ID");
                m.ForeignKey("FK_DatasetFileConfigs_Dataset");
                //m.Cascade(Cascade.All);
                m.Class(typeof(Dataset));
            });

            this.ManyToOne(x => x.DatasetScopeType, m =>
            {
                m.Column("DatasetScopeType_ID");
                m.ForeignKey("FK_DatasetFileConfigs_DatasetScopeTypes");
                m.Class(typeof(DatasetScopeType));
            });

            this.ManyToOne(x => x.FileExtension, m =>
            {
                m.Column("FileExtension_CDE");
                m.ForeignKey("FK_DatasetFileConfigs_FileExtension");
                m.Class(typeof(FileExtension));
            });

            this.Bag(x => x.RetrieverJobs, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("RetrieverJob");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Config_ID");
                    k.ForeignKey("FK_RetrieverJob_DatasetFileConfigs");
                });
            }, map => map.OneToMany(a => a.Class(typeof(RetrieverJob))));

            this.Bag(x => x.DatasetFiles, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("DatasetFile");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("DatasetFileConfig_ID");
                    k.ForeignKey("FK_DatasetFile_DatasetFileConfigs");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DatasetFile))));

            this.Bag(x => x.Schema, (m) =>
            {
                m.Lazy(CollectionLazy.NoLazy);
                m.Inverse(true);
                m.Table("DataElement");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("Config_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataElement))));
        }
    }
}
