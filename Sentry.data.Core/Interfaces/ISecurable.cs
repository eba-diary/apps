
namespace Sentry.data.Core
{
    public interface ISecurable 
    {
        bool IsSecured { get; set; }
        Security Security { get; set; }
        string PrimaryContactId { get; set; }
        bool IsSensitive { get; }
    }
}
