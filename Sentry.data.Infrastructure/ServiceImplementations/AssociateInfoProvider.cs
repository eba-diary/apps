using Sentry.Associates;
using Sentry.data.Core;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class AssociateInfoProvider : IAssociateInfoProvider
    {
        private readonly IAssociatesServiceClient _associateService;

        public AssociateInfoProvider(IAssociatesServiceClient associateService)
        {
            _associateService = associateService;
        }

        public async Task<Associate> GetActiveAssociateByIdAsync(string associateId)
        {
            return await _associateService.GetAssociateByIdAsync(associateId, false);
        }

        public Associate GetAssociateInfo(string associateId)
        {
            return _associateService.GetAssociateById(associateId, true);
        }

        public Associate GetAssociateInfoByName(string associateName)
        {
            return _associateService.GetAssociatesByName(associateName, true).First();
        }
    }
}
