﻿using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IConfigService
    {
        SchemaApiDTO GetSchemaApiDTO(int id);
        SchemaDetaiApilDTO GetSchemaDetailDTO(int id);
        IList<ColumnDTO> GetColumnDTO(int id);
        void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows);
        List<string> Validate(DataElementDto dto);
        List<string> Validate(DataSourceDto dto);
        List<string> Validate(DatasetFileConfigDto dto);
        bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime);
        bool CreateAndSaveNewDataSource(DataSourceDto dto);
        bool UpdateAndSaveDataSource(DataSourceDto dto);
        bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        bool UpdateAndSaveDatasetFileConfig(DatasetFileConfigDto dto);
        List<DatasetFileConfig> GetSchemaMarkedDeleted();
        DataSourceDto GetDataSourceDto(int Id);
        UserSecurity GetUserSecurityForDataSource(int id);
        AccessRequest GetDataSourceAccessRequest(int dataSourceId);
        string RequestAccessToDataSource(AccessRequest request);
        DatasetFileConfigDto GetDatasetFileConfigDto(int configId);
        List<DatasetFileConfigDto> GetDatasetFileConfigDtoByDataset(int datasetId);
        bool Delete(int id, bool logicalDelete = true);
        UserSecurity GetUserSecurityForConfig(int id);
        SchemaDto GetSchemaDto(int id);
    }
}