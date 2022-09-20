using Newtonsoft.Json.Linq;
using Sentry.Core;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <returns></returns>
        SchemaRevisionJsonStructureDto GetLatestSchemaRevisionJsonStructureBySchemaId(int datasetId, int schemaId);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaDto"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to manage schema</exception>
        /// <returns></returns>
        bool UpdateAndSaveSchema(FileSchemaDto schemaDto);
        void PublishSchemaEvent(int datasetId, int schemaId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extensionName"></param>
        /// <returns></returns>
        int GetFileExtensionIdByName(string extensionName);
        UserSecurity GetUserSecurityForSchema(int schemaId);
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
        /// <summary>
        /// Register file with dataset\schema based on dataflowstep event
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="objectKey"></param>
        /// <param name="versionId"></param>
        /// <param name="stepEvent"></param>
        /// <exception cref="ArgumentException"></exception>
        void RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldDtoList"></param>
        /// <param name="schemaId"></param>
        /// <exception cref="ValidationException">Thrown when metadata does not adhere to validations</exception>
        void ValidateCleanedFields(int schemaId, List<BaseFieldDto> fieldDtoList);
        IDictionary<int, string> GetSchemaList();
        /// <summary>
        /// Creates jobs to create new consumption layer(s) for a schema.
        /// </summary>
        /// <param name="schemaIdList">List of schemas to create a job for.</param>
        void EnqueueCreateConsumptionLayersForSchemaList(int[] schemaIdList);
        /// <summary>
        /// Creates new consumption layer(s) for a schema.
        /// </summary>
        /// <param name="schemaId">Schema ID</param>
        void CreateConsumptionLayersForSchema(FileSchema schema, FileSchemaDto dto, Dataset ds);
    }
}
