using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public class DatasetTileDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObjectStatusEnum Status { get; set; }
        public bool IsFavorite { get; set; }
        public string Category { get; set; }
        public bool IsSecured { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
