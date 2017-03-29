using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class SearchModel
    {
        //input properties
        public string SearchText { get; set; }
        public AssetState SearchState { get; set; } = AssetState.Up;
        public int? SearchCategory { get; set; }
        //public int? SearchAsset { get; set; }
        public int SearchPage { get; set; } = 1;

        //properties set by the result of a search
        public FullCategoryModel Category { get; set; }
        public IEnumerable<BaseAssetModel> Assets { get; set; }
        public int AssetCount { get; set; }
        public int PageSize { get; set; }
        public int StartRecordNumber { get; set; }
        public int EndRecordNumber { get; set; }
    }
}
