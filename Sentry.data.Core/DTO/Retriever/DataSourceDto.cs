using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DataSourceDto
    {
        public int OriginatingId { get; set; }
        public string RetrunUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string KeyCode { get; set; }
        public string IVKey { get; set; }
        public string SourceType { get; set; }
        public string AuthID { get; set; }
        public bool IsUserPassRequired { get; set; }
        public int PortNumber { get; set; }
        public HttpMethods RequestMethod { get; set; }
        public bool UrlBasedRequest { get; set; }
        public bool ObjectBasedRequest { get; set; }
        public Uri BaseUri { get; set; }

        #region TokenAuthSpecific
        public string TokenAuthHeader { get; set; }
        public string TokenAuthValue { get; set; }
        #endregion

        #region OAuthSpecific
        public string ClientId { get; set; }
        public string ClientPrivateId { get; set; }
        public string TokenUrl { get; set; }
        public int TokenExp { get; set; }
        public string Scope { get; set; }
        #endregion

        public List<RequestHeader> RequestHeaders { get; set; }

    }
}
