﻿using System.ComponentModel;
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
            SAIDAssetKeyCode = dto.SAIDAssetKeyCode;
            NamedEnvironment = dto.NamedEnvironment;
            NamedEnvironmentType = dto.NamedEnvironmentType;
        }


        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

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

        public bool IncludeInSas { get; set; }

        [DisplayName("Create Current View")]
        public bool CreateCurrentView { get; set; }

        public ObjectStatusEnum ObjectStatus { get; set; }

        [Required]
        [DisplayName("SAID Asset")]
        public string SAIDAssetKeyCode { get; set; }

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
