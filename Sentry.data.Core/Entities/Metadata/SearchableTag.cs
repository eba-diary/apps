using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SearchableTag
    {
        public SearchableTag(MetadataTag tag)
        {
            id = tag.TagId;
            Name = tag.Name;
            Description = tag.Description;
            Count = tag.Datasets.Count();
        }
        public int id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
    }
}
