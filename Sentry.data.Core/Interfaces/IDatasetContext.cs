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

        int GetMaxId();

        IEnumerable<String> GetCategoryList();

        IQueryable<Category> Categories { get; }

        IEnumerable<DatasetFrequency> GetDatasetFrequencies();
        
        void DeleteAllData();
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }

}
