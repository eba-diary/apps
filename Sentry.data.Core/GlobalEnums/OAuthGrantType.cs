﻿using System.ComponentModel;

namespace Sentry.data.Core.GlobalEnums
{
    public enum OAuthGrantType
    {
        //https://oauth.net/2/grant-types/

        //https://tools.ietf.org/html/rfc7523
        [Description("JWT Bearer")]
        jwtbearer = 0,
    }
}