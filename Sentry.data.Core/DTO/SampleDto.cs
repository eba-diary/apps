using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SampleDto : IIdentifiableDto
    {
        public int SampleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalCreator { get; set; }

        public void SetId(int id)
        {
            SampleId = id;
        }
    }
}
