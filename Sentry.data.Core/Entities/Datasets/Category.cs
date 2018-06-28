using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class Category
    {
        public Category() { }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Color { get; set; }

    }
}
