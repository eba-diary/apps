﻿using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DataSourceModel
    {
        public DataSourceModel()
        {
            Headers = new List<RequestHeader>();
            ContactIds = new List<string>();

            //this is needed for the associate picker js.
            this.HrempServiceUrl = Configuration.Config.GetHostSetting("HrApiUrl");
            this.HrempServiceEnv = Configuration.Config.GetHostSetting("HrApiEnvironment");
        }

        public DataSourceModel(DataSourceDto dto)
        {
            Id = dto.OriginatingId;
            Name = dto.Name;
            Description = dto.Description;
            AuthID = dto.AuthID;
            IsUserPassRequired = dto.IsUserPassRequired;
            BaseUri = dto.BaseUri;
            PortNumber = dto.PortNumber;
            Headers = new List<RequestHeader>();
            SourceType = dto.SourceType;
            Headers = dto.RequestHeaders ?? new List<RequestHeader>();
            TokenAuthHeader = dto.TokenAuthHeader;
            ClientId = dto.ClientId;
            TokenUrl = dto.TokenUrl;
            TokenExp = dto.TokenExp;
            PrimaryContactId = dto.PrimaryContactId;
            PrimaryContactName = dto.PrimaryContactName;
            PrimaryOwnerId = dto.PrimaryOwnerId;
            PrimaryOwnerName = dto.PrimaryOwnerName;
            IsSecured = dto.IsSecured;

            //We do not populate TokenAuthValue and ClientPrivateID.  On Post
            // if a value exists, then a new value is encrypted.  Otherwise, old value is unchanged.
            TokenAuthValue = null;
            ClientPrivateId = null;

            //this is needed for the associate picker js.
            this.HrempServiceUrl = Configuration.Config.GetHostSetting("HrApiUrl");
            this.HrempServiceEnv = Configuration.Config.GetHostSetting("HrApiEnvironment");
        }

        public virtual int Id { get; set; }

        public virtual string ReturnUrl { get; set; }

        [Required]
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
        // For certain types of Data Sources this value is required to be true.  Setting a checkboxfor to readonly still allows
        //   a user to change the value.  A workaround being used for this property is using jquery to disable the property
        //   for the specific data source types which require it to be true.  Then a $('form').on('submit') function
        //   enables the checkbox right before submitting the form.  This allows the value to be passed back to the controller.
        //https://stackoverflow.com/questions/40134337/set-mvc-checkbox-to-readonly-or-disabled-on-client-side
        public virtual bool IsUserPassRequired { get; set; }

        [DisplayName("Port Number")]
        public virtual int PortNumber { get; set; }

        [Required]
        [DisplayName("Base URL")]
        public virtual Uri BaseUri { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> AuthTypesDropdown { get; set; }

        #region TokenAuthSpecificProperties
        [DisplayName("Token Name")]
        public string TokenAuthHeader { get; set; }

        [DisplayName("Token Value")]
        public string TokenAuthValue { get; set; }
        #endregion

        #region OAuthSpecificProperties
        [DisplayName("Client Id")]
        public string ClientId { get; set; }

        [DisplayName("Client Private Id")]
        public string ClientPrivateId { get; set; }

        [DisplayName("Token Url")]
        public string TokenUrl { get; set; }

        [DisplayName("Token Expiration (in Seconds)")]
        public int TokenExp { get; set; }

        [DisplayName("Scope")]
        public string Scope { get; set; }
        #endregion

        [DisplayName("Request Headers")]
        public List<RequestHeader> Headers { get; set; }

        [DisplayName("Owner")]
        public string PrimaryOwnerName { get; set; }

        #region Security
        [DisplayName("Contact")]
        public string PrimaryContactName { get; set; }

        [DisplayName("Restrict Data Source")]
        public bool IsSecured { get; set; }
        public List<ContactInfoModel> ContactDetails { get; set; }
        #endregion


        //hidden properties
        public string PrimaryOwnerId { get; set; }
        public string PrimaryContactId { get; set; }
        public List<string> ContactIds { get; set; }


        //this is needed for the associate picker js.
        public string HrempServiceUrl { get; set; }
        public string HrempServiceEnv { get; set; }
    }
}