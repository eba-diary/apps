using System;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaFlowService : ISchemaFlowService
    {
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly ISecurityService _securityService;

        public SchemaFlowService(IConfigService configService, ISchemaService schemaService, IDatasetContext datasetContext, IUserService userService, ISecurityService securityService)
        {
            _configService = configService;
            _schemaService = schemaService;
            _datasetContext = datasetContext;
            _userService = userService;
            _securityService = securityService;
        }

        public async Task<SchemaResultDto> AddSchemaAsync(AddSchemaDto dto)
        {
            //get dataset
            Dataset dataset = _datasetContext.GetById(dto.DatasetId);

            //dataset not exists
            if (dataset != null)
            {
                //check permission to dataset
                IApplicationUser user = _userService.GetCurrentUser();
                UserSecurity security = _securityService.GetUserSecurity(dataset, user);

                if (security.CanManageSchema)
                {
                    //schema service - Create

                    //config service - Create

                    await _datasetContext.SaveChangesAsync();
                }
            }
            else
            {
                throw new ResourceNotFoundException();
            }

            throw new NotImplementedException();
        }

        public async Task<SchemaResultDto> UpdateSchemaAsync(UpdateSchemaDto dto)
        {


            throw new NotImplementedException();
        }
    }
}
