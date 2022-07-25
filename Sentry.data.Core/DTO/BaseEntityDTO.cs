using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BaseEntityDto
    {

        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetDesc { get; set; }
        public string PrimaryContactId { get; set; }
        public string PrimaryContactName { get; set; }
        public string PrimaryContactEmail { get; set; }
        public bool IsSecured { get; set; }
        public DateTime DatasetDtm { get; set; }
        public DateTime ChangedDtm { get; set; }
        public string S3Key { get; set; }
        public List<int> DatasetCategoryIds { get; set; }
        public string DatasetType { get; set; }
        public string CreationUserId { get; set; }
        public string CreationUserName { get; set; }
        public string UploadUserId { get; set; }
        public string UploadUserName { get; set; }
        public List<string> TagIds { get; set; }
        public List<string> ContactIds { get; set; }
        public List<ContactInfoDto> ContactDetails { get; set; }
        public List<ImageDto> Images { get; set; }
        public string AlternateContactEmail { get; set; }


        //shared details
        public string ObjectType { get; set; } //probably not needed now that everything is split.
        public bool IsSubscribed { get; set; }
        public bool IsFavorite { get; set; }
        public int Views { get; set; }
        public int AmountOfSubscriptions { get; set; }
        public bool CanDisplay { get; set; }
        public string MailtoLink { get; set; }
        public string CategoryColor { get; set; }
        public List<string> CategoryNames { get; set; }
        public UserSecurity Security { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }

    }
}
