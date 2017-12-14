using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.DatasetRetriever
{
    class CompressedFileRule
    {
        public CompressedFileRule(string fileName, int dataConfigId, Boolean isRegexSearch)
        {
            FileSearch = fileName;
            DataConfigId = dataConfigId;
            IsRegexSearch = isRegexSearch;
        }

        public Boolean IsRegexSearch { get; set; }
        public string FileSearch { get; set; }
        public int DataConfigId { get; set; }
    }
}
