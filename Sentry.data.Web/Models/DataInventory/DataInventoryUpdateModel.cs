namespace Sentry.data.Web
{
    public class DataInventoryUpdateModel
    {
        public int BaseColumnId { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsOwnerVerified { get; set; }
    }
}