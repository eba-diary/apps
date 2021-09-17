using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataFlowService
    {
        List<DataFlowDto> ListDataFlows();
        DataFlowDetailDto GetDataFlowDetailDto(int id);
        List<DataFlowStepDto> GetDataFlowStepDtoByTrigger(string key);
        int CreateandSaveDataFlow(DataFlowDto dto);

        IQueryable<DataSourceType> GetDataSourceTypes();
        IQueryable<DataSource> GetDataSources();
        string GetDataFlowNameForFileSchema(FileSchema scm);


        void CreateDataFlowForSchema(FileSchema scm);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaFlowName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when schemaFlowName is not specified</exception>
        /// <exception cref="Exceptions.DataFlowNotFound">Thrown when dataflow is not found</exception>
        DataFlow GetDataFlowByName(string schemaFlowName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataFlowId"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.DataFlowStepNotFound">Thrown if dataf flow step is not found</exception>
        /// <exception cref="ArgumentNullException">Thrown if parameter is not specified</exception>
        DataFlowStep GetDataFlowStepForDataFlowByActionType(int dataFlowId, DataActionType actionType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">Dataflow Id</param>
        /// <returns></returns>
        string GetSchemaStorageCodeForDataFlow(int Id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scm"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.DataFlowStepNotFound"></exception>
        /// <exception cref="ArgumentNullException"
        DataFlowStepDto GetS3DropStepForFileSchema(FileSchema scm);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepId"></param>
        /// <exception cref="ArgumentNullException"
        /// <returns></returns>
        List<DataFlowStep> GetDependentDataFlowStepsForDataFlowStep(int stepId);
        Task<ValidationException> Validate(DataFlowDto dfDto);
        List<SchemaMapDetailDto> GetMappedSchemaByDataFlow(int dataflowId);
        void DeleteFlowsByFileSchema(FileSchema scm, bool logicalDelete = true);

        /// <summary>
        /// Retrieve retrieverjobdto associated with pull type dataflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RetrieverJobDto GetAssociatedRetrieverJobDto(int id);
        int UpdateandSaveDataFlow(DataFlowDto dfDto);

        /// <summary>
        /// Given a SAID asset key code, get all the named environments from Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The four-character key code for an asset</param>
        /// <returns>A list of NamedEnvironmentDto objects</returns>
        Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsAsync(string saidAssetKeyCode);
    }
}
