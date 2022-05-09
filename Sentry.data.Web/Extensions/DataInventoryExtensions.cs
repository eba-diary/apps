using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class DataInventoryExtensions
    {
        public static DataInventorySearchResultRowModel ToWeb(this DataInventorySearchResultRowDto dto)
        {
            return new DataInventorySearchResultRowModel()
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

        public static List<DaleCategoryModel> ToWeb(this List<DataInventoryCategoryDto> dtos)
        {
            List<DaleCategoryModel> models = new List<DaleCategoryModel>();

            dtos.ForEach(x => models.Add(x.ToWeb()));

            return models;
        }

        public static DaleCategoryModel ToWeb(this DataInventoryCategoryDto dto)
        {
            return new DaleCategoryModel()
            {
                Category = dto.Category,
                IsSensitive = dto.IsSensitive
            };
        }

        public static List<DataInventorySearchResultRowModel> ToWeb(this List<DataInventorySearchResultRowDto> dtos)
        {
            List<DataInventorySearchResultRowModel> models = new List<DataInventorySearchResultRowModel>();

            dtos.ForEach(x => models.Add(x.ToWeb()));

            return models;
        }

        public static DataInventoryUpdateDto ToDto(this DataInventoryUpdateModel model)
        {
            if (model == null)
            {
                return new DataInventoryUpdateDto();
            }

            return new DataInventoryUpdateDto()
            {
                BaseColumnId = model.BaseColumnId,
                IsSensitive = model.IsSensitive,
                IsOwnerVerified = model.IsOwnerVerified
            };
        }       

        public static List<DataInventoryUpdateDto> ToDto(this List<DataInventoryUpdateModel> models)
        {
            List<DataInventoryUpdateDto> dtos = new List<DataInventoryUpdateDto>();
            models.ForEach(x => dtos.Add(x.ToDto()));
            return dtos;
        }

        //create a List of Assets that belong to the given column row. DataInventory view passes a comma delimited Asset list since one column can have many assets tied too it
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