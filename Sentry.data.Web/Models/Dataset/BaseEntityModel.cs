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
        public BaseEntityModel() { }

        public BaseEntityModel(BaseEntityDto dto)
        {
            //view fields
            this.DatasetName = dto.DatasetName;
            this.DatasetDesc = dto.DatasetDesc;
            this.CreationUserName = dto.CreationUserName;
            this.PrimaryOwnerName = dto.PrimaryOwnerName;
            this.DatasetDtm = dto.DatasetDtm;
            this.ChangedDtm = dto.ChangedDtm;
            this.DatasetCategoryIds = dto.DatasetCategoryIds;
            TagIds = string.Join(",", dto.TagIds);

            //hidden properties
            this.DatasetId = dto.DatasetId;
            this.PrimaryOwnerId = dto.PrimaryOwnerId;
            this.UploadUserName = dto.UploadUserName;


            //details
            this.IsFavorite = dto.IsFavorite;
            this.IsSubscribed = dto.IsSubscribed;
            this.AmountOfSubscriptions = dto.AmountOfSubscriptions;
            this.Views = dto.Views;
            this.ObjectType = dto.ObjectType;
            this.CategoryColor = dto.CategoryColor;
            this.CategoryNames = dto.CategoryNames;
            this.Security =dto.Security.ToModel();
        }



        //view fields
        [Required()]
        [MaxLength(1024)]
        [DisplayName("Dataset Name")]
        public string DatasetName { get; set; }
        [Required()]
        [MaxLength(4096)]
        [DisplayName("Description")]
        public string DatasetDesc { get; set; }
        [Required]
        [MaxLength(128)]
        [DisplayName("Originating Creator")]
        public string CreationUserName { get; set; }
        [Required]
        [DisplayName("Primary Owner")]
        public string PrimaryOwnerName { get; set; }
        [Required]
        [DisplayName("Creation Date")]
        [DataType(DataType.Date)]
        public DateTime DatasetDtm { get; set; }
        [DisplayName("Last Modified")]
        public DateTime ChangedDtm { get; set; }
        [Required]
        [DisplayName("Category")]
        public List<int> DatasetCategoryIds { get; set; }
        public string TagIds { get; set; }



        //Dropdown Lists
        public IEnumerable<SelectListItem> AllCategories { get; set; }
        public IEnumerable<SelectListItem> AllFrequencies { get; set; }
        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }
        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataClassifications { get; set; }
        public IEnumerable<SelectListItem> AllExtensions { get; set; }



        //hidden properties
        public int DatasetId { get; set; }
        public string PrimaryOwnerId { get; set; }
        public string UploadUserName { get; set; }


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
