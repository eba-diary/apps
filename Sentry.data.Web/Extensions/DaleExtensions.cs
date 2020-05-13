using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;

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
                Criteria = model.Criteria,
                Destiny = model.Destiny
            };
        }

        public static DaleResultModel ToWeb(this DaleResultDto dto)
        {
            return new DaleResultModel()
            {
                Server = dto.Server,
                Database = dto.Database,
                Table = dto.Table,
                Column = dto.Column,
                ColumnType = dto.ColumnType,
                PrecisionLength = dto.PrecisionLength,
                ScaleLength = dto.ScaleLength,
                EffectiveDate = dto.EffectiveDate,
                ExpirationDate = dto.ExpirationDate,
                LastScanDate = dto.LastScanDate
            };
        }

        public static List<DaleResultModel> ToWeb(this List<DaleResultDto> dtos)
        {
            List<DaleResultModel> models = new List<DaleResultModel>();

            dtos.ForEach(x => models.Add(x.ToWeb()));

            return models;
        }

        public static DaleDestiny ToDaleDestiny(this string destiny)
        {
            if(destiny == DaleDestiny.Column.GetDescription())
            {
                return DaleDestiny.Column;
            }
            else if (destiny == DaleDestiny.Table.GetDescription())
            {
                return DaleDestiny.Table;
            }
            else
            {
                return DaleDestiny.View;
            }
        }
    }
}