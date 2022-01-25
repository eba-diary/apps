namespace Sentry.data.Web
{
    public class FilterSearchModel : FilterCategoriesSearchModel
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

    }
}