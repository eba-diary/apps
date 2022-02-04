using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

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
                Sensitive = model.Sensitive,
                AdvancedCriteria = model.DaleAdvancedCriteria.ToDto()
            };
        }

        public static DaleAdvancedCriteriaDto ToDto(this DaleAdvancedCriteriaModel model)
        {
            if (model == null)
            {
                return new DaleAdvancedCriteriaDto();
            }

            return new DaleAdvancedCriteriaDto()
            {
                Asset = model.Asset,                
                Server = model.Server,
                Database = model.Database,
                Object = model.Object,
                ObjectType = model.ObjectType,
                Column = model.Column,
                SourceType = model.SourceType
            };
        }

        public static DaleResultRowModel ToWeb(this DaleResultRowDto dto)
        {
            return new DaleResultRowModel()
            {
                Asset = dto.Asset,
                Server = dto.Server,
                Database = dto.Database,
                Object = dto.Object,
                ObjectType = dto.ObjectType,
                Column = dto.Column,

                IsSensitive = dto.IsSensitive,
                ProdType = dto.ProdType,

                ColumnType = dto.ColumnType,
                MaxLength = dto.MaxLength,
                Precision = dto.Precision,
                Scale = dto.Scale,
                IsNullable = dto.IsNullable,
                EffectiveDate = dto.EffectiveDate.ToString("MM/dd/yyyy HH:mm:ss"),
                BaseColumnId = dto.BaseColumnId,

                IsOwnerVerified = dto.IsOwnerVerified,
                AssetList = CreateAssetList(dto.Asset),
                SourceType = dto.SourceType,

                ScanCategory = dto.ScanCategory,
                ScanType = dto.ScanType
            };
        }




        public static DaleResultModel ToWeb(this DaleResultDto dto)
        {
            return new DaleResultModel()
            {
                DaleResults = dto.DaleResults.ToWeb(),
                DaleEvent = dto.DaleEvent
            };
        }

        public static DaleContainSensitiveResultModel ToWeb(this DaleContainSensitiveResultDto dto)
        {
            return new DaleContainSensitiveResultModel()
            {
                DoesContainSensitiveResults = dto.DoesContainSensitiveResults,
                DaleEvent = dto.DaleEvent
            };
        }

        public static DaleCategoryResultModel ToWeb(this DaleCategoryResultDto dto)
        {
            return new DaleCategoryResultModel()
            {
                DaleCategories = dto.DaleCategories.ToWeb(),
                DaleEvent = dto.DaleEvent
            };
        }

        public static List<DaleCategoryModel> ToWeb(this List<DaleCategoryDto> dtos)
        {
            List<DaleCategoryModel> models = new List<DaleCategoryModel>();

            dtos.ForEach(x => models.Add(x.ToWeb()));

            return models;
        }

        public static DaleCategoryModel ToWeb(this DaleCategoryDto dto)
        {
            return new DaleCategoryModel()
            {
                Category = dto.Category,
                IsSensitive = dto.IsSensitive
            };
        }

        public static List<DaleResultRowModel> ToWeb(this List<DaleResultRowDto> dtos)
        {
            List<DaleResultRowModel> models = new List<DaleResultRowModel>();

            dtos.ForEach(x => models.Add(x.ToWeb()));

            return models;
        }

        public static DaleDestiny ToDaleDestiny(this string destiny)
        {
            if (destiny.ToUpper() == DaleDestiny.Column.GetDescription().ToUpper())
            {
                return DaleDestiny.Column;
            }
            else if (destiny.ToUpper() == DaleDestiny.Object.GetDescription().ToUpper())
            {
                return DaleDestiny.Object;
            }
            else if(destiny.ToUpper() == DaleDestiny.SAID.GetDescription().ToUpper())
            {
                return DaleDestiny.SAID;
            }
            else if (destiny.ToUpper() == DaleDestiny.Server.GetDescription().ToUpper())
            {
                return DaleDestiny.Server;
            }
            else if (destiny.ToUpper() == DaleDestiny.Database.GetDescription().ToUpper())
            {
                return DaleDestiny.Database;
            }
            else
            {
                return DaleDestiny.Advanced;
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
                IsSensitive = model.IsSensitive,
                IsOwnerVerified = model.IsOwnerVerified
            };
        }

       

        public static List<DaleSensitiveDto> ToDto(this List<DaleSensitiveModel> models)
        {
            List<DaleSensitiveDto> dtos = new List<DaleSensitiveDto>();

            models.ForEach(x => dtos.Add(x.ToDto()));

            return dtos;
        }

        //create a List of Assets that belong to the given column row.  DataInventory view passes a comma delimited Asset list since one column can have many assets tied too it
        private static List<string> CreateAssetList(string asset)
        {
            if (!string.IsNullOrWhiteSpace(asset))
            {
                List<string> assets = asset.Split(',').Select(x => x != null ? x.TrimStart() : null).ToList();
                return assets;
            }
            return new List<string>();
        }

    }
}