using Sentry.Core;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDatasetContext : IWritableDomainContext
    {
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        IQueryable<Dataset> Datasets { get; }
        //IQueryable<Category> DatasetMetadata { get; }

        Dataset GetById(int id);

        int GetDatasetCount();

        int GetCategoryDatasetCount(Category cat);

        int GetMaxId();

        Dataset GetByS3Key(string S3key);

        IEnumerable<String> GetCategoryList();

        IEnumerable<String> GetSentryOwnerList();

        IQueryable<Category> Categories { get; }

        IEnumerable<DatasetFrequency> GetDatasetFrequencies();
        
        void DeleteAllData();

        Boolean s3KeyDuplicate(string s3key);

        Boolean isDatasetNameDuplicate(string name);

        string GetPreviewKey(int id);
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

        Category GetCategoryByName(string name);
    }

}
