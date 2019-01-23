﻿
namespace Sentry.data.Core
{
    public class DatasetDto : BaseEntityDto
    {

        public int OriginationId { get; set; }
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public int FileExtensionId { get; set; }
        public string Delimiter { get; set; }
        public int DatasetScopeTypeId { get; set; }
        public string DatasetInformation { get; set; }
        public string CategoryName { get; set; }
    }
}
