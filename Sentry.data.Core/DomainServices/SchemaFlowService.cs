using System;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaFlowService : ISchemaFlowService
    {
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFlowService _dataFlowService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly ISecurityService _securityService;

        public SchemaFlowService(IConfigService configService, ISchemaService schemaService, IDataFlowService dataFlowService, IDatasetContext datasetContext, IUserService userService, ISecurityService securityService)
        {
            _configService = configService;
            _schemaService = schemaService;
            _dataFlowService = dataFlowService;
            _datasetContext = datasetContext;
            _userService = userService;
            _securityService = securityService;
        }

        public async Task<SchemaResultDto> AddSchemaAsync(AddSchemaDto dto)
        {
            //get dataset
            Dataset dataset = _datasetContext.GetById(dto.SchemaDto.ParentDatasetId);

            //check permission to dataset
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity security = _securityService.GetUserSecurity(dataset, user);

            if (security.CanManageSchema)
            {
                try
                {
                    //schema service - Create
                    FileSchemaDto addedSchemaDto = await _schemaService.AddSchemaAsync(dto.SchemaDto);

                    //config service - Create
                    dto.DatasetFileConfigDto.SchemaId = addedSchemaDto.SchemaId;
                    DatasetFileConfigDto addedConfigDto = await _configService.AddDatasetFileConfigAsync(dto.DatasetFileConfigDto);

                    //dataflow service - Create
                    dto.DataFlowDto.Name = $"{dataset.ShortName}_{addedSchemaDto.Name.Replace(" ", "")}";
                    DataFlowDto addedDataFlowDto = await _dataFlowService.AddDataFlowAsync(dto.DataFlowDto);

                    //map to result dto
                    //return result dto
                }
                catch (Exception ex)
                {
                    _datasetContext.Clear();
                    throw;
                }
            }

            throw new NotImplementedException();
        }

        public async Task<SchemaResultDto> UpdateSchemaAsync(UpdateSchemaDto dto)
        {


            throw new NotImplementedException();
        }
    }
}
