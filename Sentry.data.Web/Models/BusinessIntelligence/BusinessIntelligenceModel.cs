using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceModel : BaseDatasetModel
    {
        public BusinessIntelligenceModel()
        {
            this.CanDisplay = true;
            this.TagIds = new List<int>();
        }

        public BusinessIntelligenceModel(BusinessIntelligenceDto dto) : base(dto, null)
        {
            Location = dto.Location;
            FileTypeId = dto.FileTypeId;
            FrequencyId = dto.FrequencyId;
            TagIds = dto.TagIds;
        }



        [Required]
        [DisplayName("Report Location")]
        public string Location { get; set; }

        [DisplayName("Exhibit Type")]
        public int FileTypeId { get; set; }

        [DisplayName("Frequency")]
        public int? FrequencyId { get; set; }

        public List<int> TagIds { get; set; } //selected values
        public string TagString { get; set; } //how the tags are displayed.
    }
}