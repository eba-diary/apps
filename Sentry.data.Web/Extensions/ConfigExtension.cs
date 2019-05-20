using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class ConfigExtension
    {
        public static Core.DataElementDto ToDto(this EditSchemaModel model)
        {
            return new Core.DataElementDto()
            {
                SchemaName = model.Name,
                SchemaDescription = model.Description,
                SchemaIsForceMatch = model.IsForceMatch,
                SchemaIsPrimary = model.IsPrimary,
                Delimiter = model.Delimiter,
                DataElementChange_DTM = DateTime.Now,
                HasHeader = model.HasHeader,
                FileFormatId = model.FileTypeId
            };            
        }

        public static Core.DataElementDto DatasetModelToDto(this DatasetModel model)
        {
            return new Core.DataElementDto()
            {
                SchemaName = model.ConfigFileName,
                SchemaDescription = model.ConfigFileDesc,
                SchemaIsForceMatch = false,
                SchemaIsPrimary = true,
                Delimiter = model.Delimiter,
                DataElementChange_DTM = DateTime.Now,
                HasHeader = model.HasHeader,
                FileFormatId = model.FileExtensionId
            };
        }
    }
}