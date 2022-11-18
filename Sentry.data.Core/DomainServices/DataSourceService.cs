using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataSourceService : IDataSourceService
    {
        #region Fields
        private readonly IDatasetContext _datasetContext;
        #endregion

        #region Constructor
        public DataSourceService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }
        #endregion

        #region IDataSourceService Implementations
        public List<DataSourceTypeDto> GetDataSourceTypeDtosForDropdown()
        {
            List<string> dropdownTypes = new List<string>
            {
                DataSourceDiscriminator.FTP_SOURCE,
                DataSourceDiscriminator.SFTP_SOURCE,
                DataSourceDiscriminator.DFS_CUSTOM,
                DataSourceDiscriminator.HTTPS_SOURCE,
                DataSourceDiscriminator.GOOGLE_API_SOURCE,
                DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE
            };

            List<DataSourceTypeDto> dataSourceDtos = _datasetContext.DataSourceTypes.Where(x => dropdownTypes.Contains(x.DiscrimatorValue))
                .Select(x => new DataSourceTypeDto { Name = x.Name, Description = x.Description, DiscrimatorValue = x.DiscrimatorValue})
                .ToList();

            return dataSourceDtos;
        }

        public List<AuthenticationTypeDto> GetValidAuthenticationTypeDtosByType(string sourceType)
        {
            List<AuthenticationType> validAuthTypes;

            switch (sourceType)
            {
                case DataSourceDiscriminator.FTP_SOURCE:
                    validAuthTypes = new FtpSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.SFTP_SOURCE:
                    validAuthTypes = new SFtpSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.DFS_CUSTOM:
                    validAuthTypes = new DfsCustom().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    validAuthTypes = new HTTPSSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    validAuthTypes = new GoogleApiSource().ValidAuthTypes;
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    validAuthTypes = new GoogleBigQueryApiSource().ValidAuthTypes;
                    break;
                default:
                    throw new NotImplementedException();
            }

            List<string> validAuthTypeCodes = validAuthTypes.Select(x => x.AuthType).ToList();

            List<AuthenticationType> allAuthTypes = _datasetContext.AuthTypes.ToList();
            List<AuthenticationType> fullValidAuthTypes = allAuthTypes.Where(x => validAuthTypeCodes.Contains(x.AuthType)).ToList();

            List<AuthenticationTypeDto> authenticationTypeDtos = fullValidAuthTypes.Select(x => x.ToDto()).ToList();

            return authenticationTypeDtos;
        }

        public List<AuthenticationTypeDto> GetAuthenticationTypeDtos()
        {
            List<AuthenticationType> allAuthTypes = _datasetContext.AuthTypes.ToList();
            List<AuthenticationTypeDto> authenticationTypeDtos = allAuthTypes.Select(x => x.ToDto()).ToList();

            return authenticationTypeDtos;
        }
        #endregion
    }
}
