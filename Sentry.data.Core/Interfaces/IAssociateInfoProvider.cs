using Sentry.Associates;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IAssociateInfoProvider
    {
        Associate GetAssociateInfo(string associateId);
        Associate GetAssociateInfoByName(string associateName);
        Task<Associate> GetActiveAssociateByIdAsync(string associateId);
    }
}
