using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISchemaService
    {
        SchemaRevisionDto GetSchemaRevisionDto(int id);
        List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id);
        List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId);
        SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId);
        int CreateAndSaveSchema(FileSchemaDto schemaDto);
        bool UpdateAndSaveSchema(FileSchemaDto schemaDto);
        FileSchemaDto GetFileSchemaDto(int id);
        List<DatasetFile> GetDatasetFilesBySchema(int schemaId);
        DatasetFile GetLatestDatasetFileBySchema(int schemaId);
        FileSchema GetFileSchemaByStorageCode(string storageCode);
        bool RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent);
    }
}
