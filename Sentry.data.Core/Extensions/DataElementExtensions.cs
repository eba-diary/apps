using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sentry.data.Core
{
    public static class DataElementExtensions
    {

        public static string GenerateSASLibary(this DataElementDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.ParentDatasetId));
        }        
    }
}
