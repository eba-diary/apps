using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class EditSourceModel
    {
        public EditSourceModel()
        {

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

            if (ds.Is<DfsBasic>()) { SourceType = "DFSBasic"; }
            else if (ds.Is<S3Basic>()) { SourceType = "S3Basic"; }
            else if (ds.Is<FtpSource>()) { SourceType = "FTP"; }
            else if (ds.Is<SFtpSource>()) { SourceType = "SFTP"; }
            else if (ds.Is<DfsCustom>()) { SourceType = "DFSCustom"; }
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

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> AuthTypesDropdown { get; set; }
    }
}