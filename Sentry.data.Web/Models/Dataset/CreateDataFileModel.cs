using Sentry.data.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class CreateDataFileModel : BaseEntityModel
    {
        public CreateDataFileModel() { }

        public CreateDataFileModel(DatasetDetailDto dto) : base(dto)
        {
            this.dsID = dto.DatasetId;
            this.DatasetFileConfigNames = dto.DatasetFileConfigSchemas.ToDictionary(x => x.ConfigId.ToString(), y => y.SchemaName);
        }

        [DisplayName("Dataset")]
        public int dsID { get; set; }

        [DisplayName("Schema")]
        public Dictionary<string, string> DatasetFileConfigNames { get; set; }
    }
}