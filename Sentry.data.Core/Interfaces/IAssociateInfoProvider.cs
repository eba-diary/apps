using Sentry.Associates;

namespace Sentry.data.Core
{
    public interface IAssociateInfoProvider
    {
        Associate GetAssociateInfo(string associateId);

        Associate GetAssociateInfoByName(string associateName);
    }
}
