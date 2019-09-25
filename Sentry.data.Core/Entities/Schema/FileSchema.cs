using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class FileSchema : Schema
    {
        public FileSchema() { }
        public FileSchema(DatasetFileConfig config,IApplicationUser user) : base(config, user)
        {
            Extension = config.FileExtension;
        }
        public virtual FileExtension Extension { get; set; }
    }
}
