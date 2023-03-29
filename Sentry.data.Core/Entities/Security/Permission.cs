
namespace Sentry.data.Core
{
    public class Permission
    {

        public Permission() { }

        public virtual int PermissionId { get; set; }
        public virtual string PermissionCode { get; set; }
        public virtual string PermissionName { get; set; }
        public virtual string PermissionDescription { get; set; }
        public virtual string SecurableObject { get; set; }

        public override string ToString()
        {
            return $"{PermissionName} - {PermissionDescription}";
        }
    }
}
