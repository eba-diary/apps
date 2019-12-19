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
        bool CreateDataFlow(int schemaId);
        void PublishMessage(string key, string message);
        //bool GenerateJobRequest(int dataFlowStepId, string sourceBucket, string sourceKey, string executionGuid);
        IQueryable<DataSourceType> GetDataSourceTypes();
        IQueryable<DataSource> GetDataSources();
    }
}
