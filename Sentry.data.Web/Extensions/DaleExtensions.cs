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
                Destiny = model.Destiny,
                Sensitive = model.Sensitive
            };
        }

        public static DaleResultModel ToWeb(this DaleResultDto dto)
        {
            return new DaleResultModel()
            {
                Asset = dto.Asset,
                Server = dto.Server,
                Database = dto.Database,
                Object = dto.Object,
                ObjectType = dto.ObjectType,
                Column = dto.Column,
                
                IsSensitive = dto.IsSensitive,
                Alias = dto.Alias,
                ProdType = dto.ProdType,

                ColumnType = dto.ColumnType,
                MaxLength = dto.MaxLength,
                Precision = dto.Precision,
                Scale = dto.Scale,
                IsNullable = dto.IsNullable,
                EffectiveDate = dto.EffectiveDate.ToString("MM/dd/yyyy HH:mm:ss"),
                BaseColumnId = dto.BaseColumnId,

                IsUserVerified = dto.IsUserVerified

                
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
            else 
            {
                return DaleDestiny.Object;
            }
        }

        public static DaleSensitiveDto ToDto(this DaleSensitiveModel model)
        {
            if (model == null)
            {
                return new DaleSensitiveDto();
            }

            return new DaleSensitiveDto()
            {
                BaseColumnId = model.BaseColumnId,
                IsSensitive = model.IsSensitive
            };
        }

        public static List<DaleSensitiveDto> ToDto(this List<DaleSensitiveModel> models)
        {
            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>();

            models.ForEach(x => dtos.Add(x.ToDto()));

            return dtos;
        }


    }
}