﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDaleSearchProvider
    {
        List<DaleResultDto> GetSearchResults(DaleSearchDto dto);
        bool SaveSensitive(string sensitiveBlob);

    }
}
