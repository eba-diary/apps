namespace Sentry.data.Web.API
{
    public class FilterCategoryOptionResponseModel : BaseFilterCategoryOptionModel
    {
        public long ResultCount { get; set; }
        public string ParentCategoryName { get; set; }
        public bool Selected { get; set; }
    }
}