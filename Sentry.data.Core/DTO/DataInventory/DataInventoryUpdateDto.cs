namespace Sentry.data.Core
{
    public class DataInventoryUpdateDto
    {
        public int BaseColumnId { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsOwnerVerified { get; set; }
    }
}

