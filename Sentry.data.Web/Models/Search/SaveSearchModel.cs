namespace Sentry.data.Web
{
    public class SaveSearchModel : FilterSearchModel
    {
        public int Id { get; set; }
        public string SearchType { get; set; }
        public bool AddToFavorites { get; set; }
    }
}