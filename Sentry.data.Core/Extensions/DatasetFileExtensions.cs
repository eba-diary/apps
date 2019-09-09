using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class DatasetFileExtensions
    {
        public static List<string> ToObjectKeyVersion(this List<DatasetFileParquet> parquetFileList)
        {
            List<string> resultList = new List<string>();
            foreach (DatasetFileParquet file in parquetFileList)
            {
                resultList.Add(
                   $"parquet/{file.FileLocation}"
                );
            }
            return resultList;
        }
    }
}
