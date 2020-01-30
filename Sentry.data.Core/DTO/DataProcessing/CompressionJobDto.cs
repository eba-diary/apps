using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class CompressionJobDto
    {
        public CompressionTypes CompressionType { get; set; }
        public List<string> FileNameExclusionList { get; set; }
    }
}
