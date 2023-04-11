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
        [PropertyName("categorycode")]
        public string CategoryCode { get; set; }
        [PropertyName("namedenvironment")]
        public string NamedEnvironment { get; set; }
        [PropertyName("namedenvironmenttype")]
        public string NamedEnvironmentType { get; set; }
        [PropertyName("originationcode")]
        public string OriginationCode { get; set; }
        [PropertyName("issecured")]
        public bool IsSecured { get; set; }
        [PropertyName("favoriteuserids")]
        public List<string> FavoriteUserIds { get; set; }
        [PropertyName("environmentschemas")]
        public List<EnvironmentSchema> EnvironmentSchemas { get; set; }
    }
}