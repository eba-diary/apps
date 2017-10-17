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

            this.Property((x) => x.DataFileConfigId, (m) => m.Column("DataFileConfig_ID"));
            this.Property((x) => x.DatasetId, (m) => m.Column("Dataset_ID"));
            this.Property((x) => x.SearchCriteria, (m) => m.Column("SearchCriteria"));
            this.Property((x) => x.TargetFileName, (m) => m.Column("TargetFile_NME"));
            this.Property((x) => x.DropLocationType, (m) => m.Column("DropLocationType"));
            this.Property((x) => x.DropPath, (m) => m.Column("DropPath"));
            this.Property((x) => x.IsRegexSearch, (m) => m.Column("RegexSearch_IND"));
            this.Property((x) => x.OverwriteDatafile, (m) => m.Column("OverwriteDatafile_IND"));
            this.Property((x) => x.VersionsToKeep, (m) => m.Column("VersionsToKeep_NBR"));
            this.Property((x) => x.FileTypeId, (m) => m.Column("FileType_ID"));
            this.Property((x) => x.Name, (m) => m.Column("Config_NME"));
            this.Property((x) => x.Description, (m) => m.Column("Config_DSC"));
            this.Property((x) => x.IsGeneric, (m) => m.Column("IsGeneric"));
        }
    }
}
