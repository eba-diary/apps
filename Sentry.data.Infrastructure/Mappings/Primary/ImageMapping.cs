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
    public class ImageMapping : ClassMapping<Image>
    {
        public ImageMapping()
        {
            this.Table("Image");

            this.Id((x) => x.ImageId, (m) =>
            {
                m.Column("ImageID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.ContentType, (m) => m.Column("ContentType"));
            this.Property((x) => x.FileExtension, (m) => m.Column("FileExtension"));
            this.Property((x) => x.FileName, (m) => m.Column("FileName"));
            this.Property((x) => x.StorageBucketName, (m) => m.Column("StorageBucketName"));
            this.Property((x) => x.StoragePrefix, (m) => m.Column("StoragePrefix"));
            this.Property((x) => x.StorageKey, (m) => m.Column("StorageKey"));
            this.Property((x) => x.UploadDate, (m) => m.Column("UploadDate"));
            this.Property((x) => x.Sort, (m) => m.Column("Sort"));

            this.ManyToOne(x => x.ParentDataset, m =>
            {
                m.Column("ParentDataset");
                m.ForeignKey("FK_DatasetFileConfigs_Dataset");
                m.Cascade(Cascade.All);
                m.Class(typeof(Dataset));
            });
        }
    }
}
