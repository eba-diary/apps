using Newtonsoft.Json;
using Sentry.data.Core.DTO.Retriever;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.Config
{
    public class DataSourceTokenModel
    {
        public virtual int Id { get; set; }

        [DisplayName("Value")]
        public virtual string CurrentToken { get; set; }

        [DisplayName("Refresh Value")]
        public virtual string RefreshToken { get; set; }

        [DisplayName("Expiration Time")]
        public virtual DateTime? CurrentTokenExp { get; set; }

        [DisplayName("Name")]
        public virtual string TokenName { get; set; }

        [DisplayName("Url")]
        public virtual string TokenUrl { get; set; }

        [DisplayName("Expiration (in Seconds)")]
        public virtual int TokenExp { get; set; }

        [DisplayName("Scope")]
        public virtual string Scope { get; set; }

        public bool ToDelete { get; set; }

        public bool Enabled { get; set; }
    }
}