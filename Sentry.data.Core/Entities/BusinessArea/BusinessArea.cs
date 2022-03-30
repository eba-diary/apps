namespace Sentry.data.Core
{
    public class BusinessArea : ISecurable
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string AbbreviatedName { get; set; }
        public virtual string PrimaryOwnerId { get; set; }


        #region Security

        public virtual string PrimaryContactId { get; set; }
        public virtual bool IsSecured { get; set; }
        public virtual Security Security { get; set; }

        /// <summary>
        /// AdminDataPermissionsAreExplicit is decided at the dataset level - 
        /// This will always be false for this entity type
        /// </summary>
        public virtual bool AdminDataPermissionsAreExplicit { get; }
        public virtual ISecurable Parent { get; }

        #endregion
    }
}