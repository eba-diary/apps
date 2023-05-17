using Newtonsoft.Json.Linq;
using Sentry.Core;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="schemaId"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <returns></returns>
        SchemaRevisionFieldStructureDto GetLatestSchemaRevisionFieldStructureBySchemaId(int datasetId, int schemaId);

        /// <summary>
        /// Create new FileSchema entity and it to context. Context add is awaited.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<FileSchemaDto> AddSchemaAsync(FileSchemaDto dto);

        /// <summary>
        /// Create new FileSchema entity and add it to context.  Does not save changes.
        /// </summary>
        /// <param name="dto">FileSchemaDto</param>
        /// <returns></returns>
        int Create(FileSchemaDto dto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaDto"></param>
        /// <returns></returns>
        int CreateAndSaveSchema(FileSchemaDto schemaDto);

        /// <summary>
        /// Performs all necessary external dependency creation statements.
        /// </summary>
        /// <remarks>To be executed after creation of schema</remarks>
        /// <param name="schemaId"></param>
        Task CreateExternalDependenciesAsync(int schemaId);

        /// <summary>
        /// Performs all necessary external dependency creation statements.
        /// </summary>
        /// <remarks>To be executed after creation of schema</remarks>
        /// <param name="schemaDto"></param>
        Task CreateExternalDependenciesAsync(FileSchemaDto schemaDto);

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
        /// 
        [Obsolete("Use " + nameof(CreateSchemaRevision) + "method excepting " + nameof(SchemaRevisionFieldStructureDto), false)]
        int CreateAndSaveSchemaRevision(int schemaId, List<BaseFieldDto> schemaRows, string revisionname, string jsonSchema = null);

        /// <summary>
        /// Creates SchemaRevision and SchemaField entity objects.
        /// <para>Objects are not saved to domain context</para>
        /// </summary>
        /// <param name="schemaRevisionFieldsStructureDto"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to schema</exception>
        /// <exception cref="DatasetNotFoundException">Thrown when dataset does not exist</exception>"
        /// <returns></returns>
        int CreateSchemaRevision(SchemaRevisionFieldStructureDto schemaRevisionFieldsStructureDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="schemaRevisionId"></param>
        void CreateSchemaRevisionExternalDependencies(int schemaId, int schemaRevisionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaDto"></param>
        /// <exception cref="SchemaUnauthorizedAccessException">Thrown when user does not have access to manage schema</exception>
        /// <returns></returns>
        bool UpdateAndSaveSchema(FileSchemaDto schemaDto);
        Task<FileSchemaDto> UpdateSchemaAsync(FileSchemaDto dto, FileSchema schema);
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
        /// Create or Update consumption layer(s) for each schema.
        /// </summary>
        /// <remarks>
        /// It is callers responsibility to sync consumption layer if necessary (e.g. with Snowflake)
        /// </remarks>
        /// <param name="schemaIdList">List of schemas.</param>
        void CreateOrUpdateConsumptionLayersForSchema(int[] schemaIdList);
        /// <summary>
        /// Create or Update consumption layer(s) for a schema.
        /// </summary>
        /// <remarks>
        /// It is callers responsibility to sync consumption layer if necessary (e.g. with Snowflake)
        /// </remarks>
        /// <param name="schemaId">Schema ID</param>
        void CreateOrUpdateConsumptionLayersForSchema(FileSchema schema, FileSchemaDto dto, Dataset ds);

        (int schemaId, bool schemaExistsInTargetDataset) SchemaExistsInTargetDataset(int targetDatasetId, string schemaName);
        void GenerateConsumptionLayerEvents(FileSchema schema, JObject propertyDeltaList);

        /// <summary>
        /// Checks if we should create / update the consumption layers. 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="propertyDeltaList"></param>
        /// <param name="forceGenerate"></param>
        void TryGenerateSnowflakeConsumptionCreateEvent(FileSchema schema, JObject propertyDeltaList, bool forceGenerate);

        /// <summary>
        /// Publishes event to delete consumption layer for schema. 
        /// </summary>
        /// <param name="schema"></param>
        void PublishSnowflakeConsumptionDeleteRequest(Schema schema);
    }
}
