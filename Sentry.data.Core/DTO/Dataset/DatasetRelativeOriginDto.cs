
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetRelativeOriginDto
    {
        public int OriginDatasetId { get; set; }
        public string OriginDatasetName { get; set; }
        public List<DatasetRelativeDto> DatasetRelativesDto { get; set; }
    }
}
