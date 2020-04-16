﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Web
{
    public class DatasetModel : BaseEntityModel
    {
        public DatasetModel() { }

        public DatasetModel(DatasetDto dto) : base(dto)
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
        }


        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Dataset Scope")]
        [DisplayName("Dataset Scope")]
        public int DatasetScopeTypeId { get; set; }

        [MaxLength(16)]
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

        [MaxLength(4096)]
        [DisplayName("Usage Information")]
        public string DatasetInformation { get; set; }

        [Required]
        [DisplayName("Data Classification")]
        public DataClassificationType DataClassification { get; set; }

        [DisplayName("Add to SAS")]
        public bool IncludeInSas { get; set; }

        [DisplayName("Create Current View")]
        public bool CreateCurrentView { get; set; }
    }


}
