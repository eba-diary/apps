using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <exception cref="SchemaNotFound">Thrown when schema is not found</exception>
        /// <returns></returns>
        List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id);
        List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <exception cref="SchemaUnauthorizedAccess">Thrown when user does not have access to schema</exception>
        /// <returns></returns>
        SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId);
        int CreateAndSaveSchema(FileSchemaDto schemaDto);
        bool UpdateAndSaveSchema(FileSchemaDto schemaDto);
        FileSchemaDto GetFileSchemaDto(int id);
        List<DatasetFile> GetDatasetFilesBySchema(int schemaId);
        DatasetFile GetLatestDatasetFileBySchema(int schemaId);
        FileSchema GetFileSchemaByStorageCode(string storageCode);
        bool RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent);
        bool SasUpdateNotification(int schemaId, int revisionId, string initiatorId);
    }
}
