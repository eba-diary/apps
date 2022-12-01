using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetModel : BaseEntityModel
    {
        public DatasetModel() { }

        public DatasetModel(DatasetSchemaDto dto) : base(dto)
        {
            OriginationID = dto.OriginationId;
            DatasetScopeTypeId = dto.DatasetScopeTypeId;
            ConfigFileName = dto.ConfigFileName;
            ConfigFileDesc = dto.ConfigFileDesc;
            Delimiter = dto.Delimiter;
            HasHeader = dto.HasHeader;
            FileExtensionId = dto.FileExtensionId;
            DatasetInformation = dto.DatasetInformation;
            DataClassification = dto.DataClassification;
            SAIDAssetKeyCode = dto.SAIDAssetKeyCode;
            NamedEnvironment = dto.NamedEnvironment;
            NamedEnvironmentType = dto.NamedEnvironmentType;
            ShortName = dto.ShortName;
        }


        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [DisplayName("Dataset Scope")]
        public int DatasetScopeTypeId { get; set; }

        [MaxLength(100)]
        [DisplayName("Configuration Name")]
        public string ConfigFileName { get; set; }

        [DisplayName("Description")]
        public string ConfigFileDesc { get; set; }

        [DisplayName("Delimiter")]
        public string Delimiter { get; set; }

        [DisplayName("Has Header")]
        public bool HasHeader { get; set; }

        [DisplayName("File Extension")]
        public int FileExtensionId { get; set; }

        [DisplayName("Schema Root Path")]
        public string SchemaRootPath { get; set; }

        [MaxLength(4096)]
        [DisplayName("Usage Information")]
        public string DatasetInformation { get; set; }

        [Required]
        [DisplayName("Data Classification")]
        public DataClassificationType DataClassification { get; set; }

        [DisplayName("Create Current View")]
        public bool CreateCurrentView { get; set; }

        public ObjectStatusEnum ObjectStatus { get; set; }

        [Required]
        [DisplayName("SAID Asset")]
        public string SAIDAssetKeyCode { get; set; }

        [Required]
        [DisplayName("Short Name")]
        [Description("A unique alphanumeric code name for this dataset that is 12 characters or less")]
        [MaxLength(12, ErrorMessage = "Short Name must be 12 characters or less")]
        [RegularExpression("^[0-9a-zA-Z]*$", ErrorMessage = "Only alphanumeric characters are allowed in the Dataset Short Name")]
        public string ShortName { get; set; }

        /// <summary>
        /// Named Environment naming conventions from https://confluence.sentry.com/x/eQNvAQ
        /// </summary>
        [DisplayName("Named Environment")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression("^[A-Z0-9]{1,10}$", ErrorMessage = "Named environment must be alphanumeric, all caps, and less than 10 characters")]
        public string NamedEnvironment { get; set; }


        [DisplayName("Named Environment Type")]
        [System.ComponentModel.DataAnnotations.Required]
        public NamedEnvironmentType NamedEnvironmentType { get; set; }

        public IEnumerable<SelectListItem> SAIDAssetDropDown { get; set; }
        public IEnumerable<SelectListItem> NamedEnvironmentDropDown { get; set; }
        public IEnumerable<SelectListItem> NamedEnvironmentTypeDropDown { get; set; }
    }


}
