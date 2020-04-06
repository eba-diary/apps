using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessArea : ISecurable
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string AbbreviatedName { get; set; }

        //ISecurable Impl.
        public virtual string PrimaryOwnerId { get; set; }
        public virtual string PrimaryContactId { get; set; }
        public virtual bool IsSecured { get; set; }
        public virtual Security Security { get; set; }


    }
}