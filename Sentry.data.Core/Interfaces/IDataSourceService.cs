using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataSourceService
    {
        void ExchangeAuthToken(DataSource dataSource, string authToken);
    }
}
