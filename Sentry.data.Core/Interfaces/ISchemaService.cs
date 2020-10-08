﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Core;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Core
{
    public interface ISchemaService
    {
        SchemaRevisionDto GetSchemaRevisionDto(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="SchemaNotFoundException">Thrown when schema is not found</exception>
        /// <returns></returns>
        List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id);
        List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <returns></returns>
        SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId);
        int CreateAndSaveSchema(FileSchemaDto schemaDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <param name="schemaRows"></param>
        /// <param name="revisionname"></param>
        /// <param name="jsonSchema"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <returns></returns>
        int CreateAndSaveSchemaRevision(int schemaId, List<BaseFieldDto> schemaRows, string revisionname, string jsonSchema = null);
        bool UpdateAndSaveSchema(FileSchemaDto schemaDto);
        FileSchemaDto GetFileSchemaDto(int id);
        List<DatasetFile> GetDatasetFilesBySchema(int schemaId);
        DatasetFile GetLatestDatasetFileBySchema(int schemaId);
        FileSchema GetFileSchemaByStorageCode(string storageCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Config id</param>
        /// <param name="rows">Number of rows to return</param>
        /// <exception cref="SchemaNotFoundException">Thrown when schema is not found</exception>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <exception cref="HiveTableViewNotFoundException">Thrown when table or view not found</exception>
        /// <exception cref="HiveQueryException">Thrown when odbc driver throws an error</exception>
        /// <returns></returns>
        List<Dictionary<string, object>> GetTopNRowsByConfig(int id, int rows);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Schema Id</param>
        /// <param name="rows">Number of rows to return</param>
        /// <exception cref="SchemaNotFoundException">Thrown when schema is not found</exception>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <exception cref="HiveTableViewNotFoundException">Thrown when table or view not found</exception>
        /// <exception cref="HiveQueryException">Thrown when odbc driver throws an error</exception>
        /// <returns></returns>
        List<Dictionary<string, object>> GetTopNRowsBySchema(int id, int rows);
        bool RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent);
        bool SasUpdateNotification(int schemaId, int revisionId, string initiatorId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldDtoList"></param>
        /// <param name="schemaId"></param>
        /// <exception cref="ValidationException">Thrown when metadata does not adhere to validations</exception>
        void Validate(int schemaId, List<BaseFieldDto> fieldDtoList);
    }
}
