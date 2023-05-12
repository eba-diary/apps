using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Sentry.data.Web
{
    public class BaseEntityModel
    {
        public BaseEntityModel()
        {
            this.HrempServiceUrl = Configuration.Config.GetHostSetting("HrApiUrl");
            this.HrempServiceEnv = Configuration.Config.GetHostSetting("HrApiEnvironment");
            this.ContactIds = new List<string>();
            this.Images = new List<ImageModel>();
        }

        public BaseEntityModel(BaseEntityDto dto)
        {
            //view fields
            this.DatasetName = dto.DatasetName;
            this.DatasetDesc = dto.DatasetDesc;
            this.CreationUserId = dto.CreationUserId;
            this.CreationUserName = dto.CreationUserName;
            this.PrimaryContactId = dto.PrimaryContactId;
            this.PrimaryContactName = dto.PrimaryContactName;
            this.AlternateContactEmail = dto.AlternateContactEmail;
            this.PrimaryContactEmail = dto.PrimaryContactEmail;
            this.IsSecured = dto.IsSecured;
            this.DatasetCategoryIds = dto.DatasetCategoryIds;
            this.TagIds = string.Join(",", dto.TagIds);

            this.DatasetId = dto.DatasetId;
            this.UploadUserId = dto.UploadUserId;
            this.UploadUserName = dto.UploadUserName;
            this.DatasetDtm = dto.DatasetDtm;
            this.ChangedDtm = dto.ChangedDtm;

            //this is needed for the associate picker js.
            this.HrempServiceUrl = Configuration.Config.GetHostSetting("HrApiUrl");
            this.HrempServiceEnv = Configuration.Config.GetHostSetting("HrApiEnvironment");

            //details
            this.IsFavorite = dto.IsFavorite;
            this.IsSubscribed = dto.IsSubscribed;
            this.AmountOfSubscriptions = dto.AmountOfSubscriptions;
            this.Views = dto.Views;
            this.ObjectType = dto.ObjectType;
            this.CategoryColor = dto.CategoryColor;
            this.CategoryNames = dto.CategoryNames;
            this.Security =dto.Security.ToModel();
            this.ContactIds = dto.ContactIds;
            this.ContactDetails = dto.ContactDetails.ToModel();
            this.MailtoLink = dto.MailtoLink;
            this.Images = dto.Images.ToModel();
        }



        //view fields
        [Required()]
        [MaxLength(1024)]
        [DisplayName("Dataset Name")]
        public virtual string DatasetName { get; set; }
        [Required()]
        [MaxLength(4096)]
        [DisplayName("Description")]
        public string DatasetDesc { get; set; }
        [Required]
        [MaxLength(128)]
        [DisplayName("Originating Creator")]
        public string CreationUserId { get; set; }
        [Required]
        [DisplayName("Creation Date")]
        [DataType(DataType.Date)]
        public DateTime DatasetDtm { get; set; }
        [DisplayName("Last Modified")]
        public DateTime ChangedDtm { get; set; }
        
        [DisplayName("Category")]
        public List<int> DatasetCategoryIds { get; set; }
        public string TagIds { get; set; }

        [DisplayName("Contact")]
        public string PrimaryContactName { get; set; }

        [DisplayName("Restrict Dataset")]
        public bool IsSecured { get; set; }

        [DisplayName("Selected Contacts")]
        public List<ContactInfoModel> ContactDetails { get; set; }

        [DisplayName("Alternate Contact Email")]
        public string AlternateContactEmail { get; set; }

        public bool CLA1130_SHOW_ALTERNATE_EMAIL { get; set; }

        public List<ImageModel> Images { get; set; }


        //Dropdown Lists
        public IEnumerable<SelectListItem> AllCategories { get; set; }
        public IEnumerable<SelectListItem> AllBusinessUnits { get; set; }
        public IEnumerable<SelectListItem> AllDatasetFunctions { get; set; }
        public IEnumerable<SelectListItem> AllFrequencies { get; set; }
        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }
        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataClassifications { get; set; }
        public IEnumerable<SelectListItem> AllExtensions { get; set; }



        //hidden properties
        public int DatasetId { get; set; }
        public string PrimaryContactId { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string UploadUserId { get; set; }
        public string UploadUserName { get; set; }
        public string CreationUserName { get; set; }
        public List<string> ContactIds { get; set; }

        //this is needed for the associate picker js.
        public string HrempServiceUrl { get; set; }
        public string HrempServiceEnv { get; set; }

        //shared details
        public int AmountOfSubscriptions { get; set; }
        public int Views { get; set; }
        public string ObjectType { get; set; }
        public bool IsSubscribed { get; set; }
        public bool IsFavorite { get; set; }
        public string MailtoLink { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryColor { get; set; }

        public UserSecurityModel Security { get; set; }
    }
}
