using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class TagDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public int TagGroupId { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
}
