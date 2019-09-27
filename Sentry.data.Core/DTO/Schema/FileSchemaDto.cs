using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class FileSchemaDto : SchemaDto
    {
        public FileSchemaDto()
        {

        }
        public override string SchemaEntity_NME { get; set; }
        public int FileExtensionId { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool CreateCurrentView { get; set; }
        public bool IsInSAS { get; set; }
        public string SasLibrary { get; set; }
    }
}
