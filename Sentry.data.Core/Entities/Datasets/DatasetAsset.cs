namespace Sentry.data.Core
{
    public class DatasetAsset : ISecurable
    {

        public virtual int DatasetAssetId { get; set; }
        public virtual string SaidKeyCode { get; set; }

        /// <summary>
        /// Assets are always considered "secured"
        /// </summary>
        public virtual bool IsSecured { get; set; } = true;

        /// <summary>
        /// Assets don't have a Primary Contact Id - their owner is only pulled from SAID
        /// </summary>
        public virtual string PrimaryContactId { get; set; } = "000000";
        
        /// <summary>
        /// IsSensitive is decided at the dataset level - 
        /// entire assets aren't ever considered sensitive
        /// </summary>
        public virtual bool IsSensitive { get; }

        public virtual Security Security { get; set; }
    }
}
