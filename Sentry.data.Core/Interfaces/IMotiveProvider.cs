using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IMotiveProvider
    {
        Task MotiveOnboardingAsync(DataSource motiveSource, DataSourceToken token, int companiesDataflowId);
    }
}
