using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataSourceService
    {
        List<DataSourceTypeDto> GetDataSourceTypeDtosForDropdown();
        List<AuthenticationTypeDto> GetValidAuthenticationTypeDtosByType(string sourceType);
        List<AuthenticationTypeDto> GetAuthenticationTypeDtos();
        Task<bool> ExchangeAuthToken(DataSource dataSource, string authToken);
    }
}
