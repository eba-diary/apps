using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessUnit
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string AbbreviatedName { get; set; }
        public virtual int Sequence { get; set; }
    }
}