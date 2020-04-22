using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public static class DaleExtensions
    {
        public static DaleSearchDto ToDto(this DaleSearchModel model)
        {
            if (model == null)
            {
                return new DaleSearchDto();
            }

            return new DaleSearchDto()
            {
                Table = model.Table,
                Column = model.Column,
                Criteria = model.Criteria
            };
            
        }
    }
}