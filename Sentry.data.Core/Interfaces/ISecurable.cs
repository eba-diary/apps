
namespace Sentry.data.Core
{
    public interface ISecurable 
    {
        bool IsSecured { get; set; }
        Security Security { get; set; }
        string PrimaryContactId { get; set; }
        /// <summary>
        /// Do admins need to explicitly request permissions to an object or are they implicitly granted.
        /// </summary>
        bool AdminDataPermissionsAreExplicit { get; }
        ISecurable Parent { get; }
    }
}
