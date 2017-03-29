using Sentry.Associates;

namespace Sentry.data.Infrastructure
{
    public interface IAssociateInfoProvider
    {
        Associate GetAssociateInfo(string associateId);
    }
}
