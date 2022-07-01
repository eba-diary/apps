using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDataFlowService : IEntityService
    {
        List<DataFlowDto> ListDataFlows();
        /// <summary>
        /// Returns dataflow detail dto by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Sentry.data.Core.Exceptions.DataFlowNotFound"></exception>
        DataFlowDetailDto GetDataFlowDetailDto(int id);
        List<DataFlowDetailDto> GetDataFlowDetailDtoByDatasetId(int datasetId);
        List<DataFlowDetailDto> GetDataFlowDetailDtoBySchemaId(int schemaId);
        List<DataFlowDetailDto> GetDataFlowDetailDtoByStorageCode(string storageCode);

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
        /// Will create an upgraded dataflow (single dataflow configuration) from existing
        ///   producer dataflow metadata
        /// </summary>
        /// <param name="producerDataFlowIds"></param>
        void UpgradeDataFlows(int[] producerDataFlowIds);

        /// <summary>
        /// For the list of dataflow ids provided, this will set ObjectStatus appropriately based on logicDelete flag.
        /// In addition,
        ///   will find any retrieverjobs, associated with specified dataflow, and 
        ///   set its ObjectStatus = Deleted.
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="user"></param>
        /// <param name="logicalDelete"></param>
        /// <remarks>logicalDelete = true sets objectstatus to Pending_Delete. 
        /// logicalDelete = false sets objectstatus to Deleted.</remarks>
        bool Delete(List<int> idList, IApplicationUser user, bool logicalDelete);

        /// <summary>
        /// Will enqueue a hangfire job, for each id in idList,
        ///   that will run on hangfire background server and peform
        ///   the dataflow delete.
        /// </summary>
        /// <param name="idList"></param>
        /// <remarks> This will serves an Admin only funtionlaity within DataFlow API </remarks>
        bool Delete_Queue(List<int> idList, string userId, bool logicalDelete);

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

        /// <summary>
        /// Retrieve retrieverjobdto associated with pull type dataflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RetrieverJobDto GetAssociatedRetrieverJobDto(int id);
        int UpdateandSaveDataFlow(DataFlowDto dfDto, bool deleteOriginal = true);


        bool ValidateStepIdAndDatasetFileIds(int stepId, List<int> datasetFileIds);

        DataFlowDto GetDataFlowDtoByStepId(int stepId);

        int GetSchemaIdFromDatasetFileId(int datasetFileId);


    }
}
