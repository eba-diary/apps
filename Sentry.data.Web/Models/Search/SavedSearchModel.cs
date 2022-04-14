namespace Sentry.data.Web
{
    public class SavedSearchModel : FilterSearchModel
    {
        public string SearchName { get; set; }
        public bool AddToFavorites { get; set; }
    }
}