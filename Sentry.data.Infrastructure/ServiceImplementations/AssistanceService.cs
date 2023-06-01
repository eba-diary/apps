using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class AssistanceService : BaseDomainService<AssistanceService>, IAssistanceService
    {
        private readonly IJiraService _jiraService;

        public AssistanceService(IJiraService jiraService, DomainServiceCommonDependency<AssistanceService> commonDependency) : base(commonDependency)
        {
            _jiraService = jiraService;
        }

        public async Task<AddAssistanceResultDto> AddAssistanceAsync(AddAssistanceDto addAssistanceDto)
        {
            if (!_dataFeatures.CLA4870_DSCAssistance.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4870_DSCAssistance), "AddAssistance");
            }

            throw new NotImplementedException();
        }
    }
}
