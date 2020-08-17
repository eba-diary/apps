namespace Sentry.data.Web
{
    public class DaleSensitiveModel
    {
        public int BaseColumnId { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsUserVerified { get; set; }
    }
}