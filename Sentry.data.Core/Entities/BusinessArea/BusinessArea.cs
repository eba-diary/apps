using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessArea
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string AbbreviatedName { get; set; }
    }
}