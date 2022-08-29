namespace Sentry.data.Web
{
    public class SavedSearchOptionModel
    {
        public int SavedSearchId { get; set; }
        public string SavedSearchName { get; set; }
        public string SavedSearchUrl { get; set; }
        public bool IsFavorite { get; set; }
    }
}