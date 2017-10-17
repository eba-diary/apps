﻿using Sentry.Core;
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

        /// <summary>
        /// Checks for duplicate s3Key across all datasetfiles for specified dataset
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="s3key"></param>
        /// <returns>asdfasdf</returns>
        Boolean s3KeyDuplicate(int datasetId, string s3key);

        Boolean isDatasetNameDuplicate(string datasetName, string category);

        string GetPreviewKey(int id);

        IEnumerable<DatasetFile> GetDatasetFilesForDataset(int id);

        IEnumerable<DatasetFile> GetDatasetFilesVersions(int datasetId, int dataFileConfigId, string filename);

        int GetLatestDatasetFileIdForDataset(int id);

        IEnumerable<DatasetFile> GetAllDatasetFiles();

        DatasetFile GetDatasetFile(int id);

        IEnumerable<Dataset> GetDatasetByCategoryID(int id);

        Category GetCategoryById(int id);

        IEnumerable<DatasetScopeType> GetAllDatasetScopeTypes();

        DatasetScopeType GetDatasetScopeById(int id);

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

        Category GetCategoryByName(string name);

        IEnumerable<DatasetFileConfig> getAllDatasetFileConfigs();

        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId);

        int GetLatestDatasetFileIdForDatasetByDatasetFileConfig(int datasetId, int dataFileConfigId, string targetFileName);

        DatasetFileConfig getDatasetFileConfigs(int configId);
    }

}
