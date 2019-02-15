using Sentry.data.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class CreateDataFileModel : BaseEntityModel
    {
        public CreateDataFileModel() { }

        public CreateDataFileModel(DatasetDto dto) : base(dto)
        {
            this.CategoryIDs = dto.DatasetCategoryIds.First();
            this.OriginationID = dto.OriginationId;
            this.dsID = dto.DatasetId;
            this.CategoryName = dto.CategoryName;
        }

        [DisplayName("File Upload")]
        public HttpPostedFile f { get; set; }
        public long ProgressConnectionId { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryIDs { get; set; }

        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [DisplayName("Dataset")]
        public int dsID { get; set; }

        [Required]
        [DisplayName("Category")]
        public string CategoryName { get; set; }
    }
}