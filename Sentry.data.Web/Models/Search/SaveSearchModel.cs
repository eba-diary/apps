namespace Sentry.data.Web
{
    public class SaveSearchModel : FilterSearchModel
    {
        public string SearchType { get; set; }
        public string SearchName { get; set; }
        public bool AddToFavorites { get; set; }
    }
}