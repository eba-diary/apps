using Sentry.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDatasetService : IReadableStatelessDomainContext
    {
        IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true);

        //Dataset GetDatasetDetails(string uniqueKey);

        string GetDatasetDownloadURL(string uniqueKey);

        void UploadDataset(string sourceFilePath, Dataset ds);

        void DeleteDataset(string uniqueKey);

        //DatasetFolder GetSubFolderStructure(DatasetFolder parentFolder = null, bool includeSubDirectories = true);

        //IQueryable<Dataset> GetDatasetsByFolderName(string folderName);

        //DatasetFolder GetFolderByUniqueKey(string uniqueKey);
    }

}
