using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class FileExtensionMapping : ClassMapping<FileExtension>
    {
        public FileExtensionMapping()
        {
            this.Table("FileExtension");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.Id, (m) =>
            {
                m.Column("Extension_Id");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.Name, (m) => m.Column("Extension_NME"));
            this.Property((x) => x.Created, (m) => m.Column("Created_DTM"));
            this.Property((x) => x.CreatedUser, (m) => m.Column("CreateUser_ID"));
            
        }
    }
}
