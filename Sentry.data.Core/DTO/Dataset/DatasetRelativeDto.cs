
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetRelativeDto
    {
        public DatasetRelativeDto(int datasetId, string namedEnvironment, string url)
        {
            DatasetId = datasetId;
            NamedEnvironment = namedEnvironment;
            Url = url;
        }

        public int DatasetId { get; set; }
        public string NamedEnvironment { get; set; }
        public string Url { get; set; }
    }
}
