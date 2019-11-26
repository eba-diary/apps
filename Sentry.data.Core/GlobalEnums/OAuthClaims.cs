using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    //
    //https://www.iana.org/assignments/jwt/jwt.xhtml
    public enum OAuthClaims
    {
        [Description("Issuer")]
        iss = 0,
        [Description("Audience")]
        aud = 1,
        [Description("Expiration Time")]
        exp = 2,
        [Description("Permission Requesting")]
        scope = 3
    }
}