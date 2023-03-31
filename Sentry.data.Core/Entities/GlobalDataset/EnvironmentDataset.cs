using Nest;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class EnvironmentDataset
    {
        [PropertyName("datasetid")]
        public int DatasetId { get; set; }
        [PropertyName("datasetdescription")]
        public string DatasetDescription { get; set; }
        [PropertyName("category")]
        public string Category { get; set; }
        [PropertyName("environment")]
        public string Environment { get; set; }
        [PropertyName("environmenttype")]
        public string EnvironmentType { get; set; }
        [PropertyName("origin")]
        public string Origin { get; set; }
        [PropertyName("issecured")]
        public bool IsSecured { get; set; }
        [PropertyName("favoriteuserids")]
        public List<string> FavoriteUserIds { get; set; }
        [PropertyName("schemas")]
        public List<EnvironmentSchema> Schemas { get; set; }
    }
}