using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDataSourceService
    {
        List<DataSourceTypeDto> GetDataSourceTypeDtosForDropdown();
        List<AuthenticationTypeDto> GetValidAuthenticationTypeDtosByType(string sourceType);
        List<AuthenticationTypeDto> GetAuthenticationTypeDtos();
    }
}
