namespace Sentry.data.Core
{
    public class SavedSearchOptionDto
    {
        public int SavedSearchId { get; set; }
        public string SavedSearchName { get; set; }
        public string SavedSearchUrl { get; set; }
        public bool IsFavorite { get; set; }
    }
}
