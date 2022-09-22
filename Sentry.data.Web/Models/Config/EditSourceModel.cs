using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web
{
    public class EditSourceModel
    {
        public EditSourceModel()
        {
            Headers = new List<RequestHeader>();
        }

        public EditSourceModel(DataSource ds)
        {
            Id = ds.Id;
            Name = ds.Name;
            Description = ds.Description;
            AuthID = ds.SourceAuthType.AuthID.ToString();
            IsUserPassRequired = ds.IsUserPassRequired;
            BaseUri = ds.BaseUri;
            PortNumber = ds.PortNumber;
            Headers = new List<RequestHeader>();


            if (ds.Is<DfsBasic>()) { SourceType = DataSoureDiscriminator.DEFAULT_DROP_LOCATION; }
            else if (ds.Is<S3Basic>()) { SourceType = DataSoureDiscriminator.DEFAULT_S3_DROP_LOCATION; }
            else if (ds.Is<FtpSource>()) { SourceType = DataSoureDiscriminator.FTP_SOURCE; }
            else if (ds.Is<SFtpSource>()) { SourceType = DataSoureDiscriminator.SFTP_SOURCE; }
            else if (ds.Is<DfsCustom>()) { SourceType = DataSoureDiscriminator.DFS_CUSTOM; }
            else if (ds.Is<HTTPSSource>())
            {
                SourceType = DataSoureDiscriminator.HTTPS_SOURCE;
                Headers = ((HTTPSSource)ds).RequestHeaders ?? new List<RequestHeader>();
                if (ds.SourceAuthType.Is<TokenAuthentication>())
                {
                    TokenAuthHeader = ((HTTPSSource)ds).AuthenticationHeaderName;
                    //We do not populate the AuthenticationHeaderValue.  On Post
                    // if a value exists, then a new value is encrypted.  Otherwise, old value is unchanged.
                }
            }
        }

        public virtual int Id { get; set; }

        public virtual string ReturnUrl { get; set; }

        [DisplayName("Data Source Name")]
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        [Required]
        [DisplayName("Source Type")]
        public virtual string SourceType { get; set; }

        [Required]
        [DisplayName("Authentication Type")]
        public virtual string AuthID { get; set; }

        [Required]
        [DisplayName("Are Username and Password Required?")]
        public virtual bool IsUserPassRequired { get; set; }

        [DisplayName("Port Number")]
        public virtual int PortNumber { get; set; }

        [Required]
        [DisplayName("Base URL")]
        public virtual Uri BaseUri { get; set; }

        [DisplayName("Token Name")]
        public string TokenAuthHeader { get; set; }

        [DisplayName("Token Value")]
        public string TokenAuthValue { get; set; }

        [DisplayName("Request Headers")]
        public List<RequestHeader> Headers { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> AuthTypesDropdown { get; set; }
    }
}