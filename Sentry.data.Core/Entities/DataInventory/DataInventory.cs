using Nest;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataInventory
    {
        #region Properties
        [PropertyName("basecolumn_id")]
        public int Id { get; set; }

        [PropertyName("asset_cde")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.ASSET, true)]
        public string AssetCode { get; set; }

        [PropertyName("basedatabase_id")]
        public int BaseDatabaseId { get; set; }

        [PropertyName("base_nme")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.COLLECTIONNAME)]
        public string BaseName { get; set; }

        [PropertyName("change_cnt")]
        public int ChangeCount { get; set; }

        [PropertyName("columncontent_typ")]
        public string ColumnContentType { get; set; }

        [PropertyName("column_nme")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.COLUMN)]
        public string ColumnName { get; set; }

        [PropertyName("column_typ")]
        [FilterSearchField(FilterCategoryNames.DATATYPE)]
        public string ColumnType { get; set; }

        [PropertyName("content_id")]
        public int ContentId { get; set; }

        [PropertyName("current_cnt")]
        public int CurrentCount { get; set; }

        [PropertyName("database_nme")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.DATABASE, true)]
        public string DatabaseName { get; set; }

        [PropertyName("effectivechange_cnt")]
        public int EffectiveChangeCount { get; set; }

        [PropertyName("effective_dte")]
        public DateTime EffectiveDate { get; set; }

        [PropertyName("effective_dtm")]
        public DateTime EffectiveDateTime { get; set; }

        [PropertyName("expirationchange_cnt")]
        public int ExpirationChangeCount { get; set; }

        [PropertyName("expiration_dte")]
        public DateTime? ExpirationDate { get; set; }

        [PropertyName("expiration_dtm")]
        public DateTime? ExpirationDateTime { get; set; }

        [PropertyName("isnullable_flg")]
        [FilterSearchField(FilterCategoryNames.NULLABLE)]
        public bool? IsNullable { get; set; }

        [PropertyName("isownerverified_flg")]
        public bool IsOwnerVerified { get; set; }

        [PropertyName("issensitive_flg")]
        [FilterSearchField(FilterCategoryNames.SENSITIVE)]
        public bool IsSensitive { get; set; }

        [PropertyName("maxlength_len")]
        public int? MaxLength { get; set; }

        [PropertyName("precision_len")]
        public int? Precision { get; set; }

        [PropertyName("prod_typ")]
        [FilterSearchField(FilterCategoryNames.ENVIRONMENT)]
        public string ProdType { get; set; }

        [PropertyName("saidlist_nme")]
        public string SaidListName { get; set; }

        [PropertyName("scale_len")]
        public int? Scale { get; set; }

        [PropertyName("scanlist_nme")]
        public string ScanListName { get; set; }

        [PropertyName("scanrundate_dte")]
        public DateTime ScanRunDate { get; set; }

        [PropertyName("scanserver_nme")]
        public string ScanServerName { get; set; }

        [PropertyName("schema_nme")]
        public string SchemaName { get; set; }

        [PropertyName("server_nme")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.SERVER)]
        public string ServerName { get; set; }

        [PropertyName("source_nme")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.SOURCETYPE)]
        public string SourceName { get; set; }

        [PropertyName("@timestamp")]
        public DateTime Timestamp { get; set; }

        [PropertyName("total_cnt")]
        public int TotalCount { get; set; }

        [PropertyName("type_dsc")]
        [GlobalSearchField]
        [FilterSearchField(FilterCategoryNames.COLLECTIONTYPE)]
        public string TypeDescription { get; set; }

        [PropertyName("@version")]
        public string Version { get; set; }
        #endregion

        #region Mappings
        public DataInventorySearchResultRowDto ToDto()
        {
            return new DataInventorySearchResultRowDto()
            {
                Asset = AssetCode,
                Server = ServerName,
                Database = DatabaseName,
                Object = BaseName,
                ObjectType = TypeDescription,
                Column = ColumnName,
                IsSensitive = IsSensitive,
                ProdType = FilterCategoryOptionNormalizer.Normalize(FilterCategoryNames.ENVIRONMENT, ProdType),
                ColumnType = ColumnType,
                MaxLength = MaxLength ?? 0,
                Precision = Precision ?? 0,
                Scale = Scale ?? 0,
                IsNullable = IsNullable ?? false,
                EffectiveDate = EffectiveDateTime,
                BaseColumnId = Id,
                IsOwnerVerified = IsOwnerVerified,
                SourceType = SourceName,
                ScanCategory = ScanListName,
                ScanType = SaidListName
            };
        }
        #endregion
    }
}
