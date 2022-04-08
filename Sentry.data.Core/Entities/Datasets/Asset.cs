namespace Sentry.data.Core
{
    public class Asset : ISecurable
    {

        public virtual int AssetId { get; set; }
        public virtual string SaidKeyCode { get; set; }

        #region Security

        /// <summary>
        /// Assets are always considered "secured"
        /// </summary>
        public virtual bool IsSecured { get; set; } = true;

        /// <summary>
        /// Assets don't have a Primary Contact Id - their owner is only pulled from SAID
        /// </summary>
        public virtual string PrimaryContactId { get; set; } = "000000";

        /// <summary>
        /// AdminDataPermissionsAreExplicit is decided at the dataset level - 
        /// This will always be false for this entity type
        /// </summary>
        public virtual bool AdminDataPermissionsAreExplicit { get; }
        public virtual Security Security { get; set; }
        public virtual ISecurable Parent { get; }

        #endregion
    }
}
