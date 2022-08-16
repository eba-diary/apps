using Sentry.data.Core;

namespace Sentry.data.Web.Models.ApiModels.Dataflow
{
    public class DataFlowStepModel
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }
        public int ExecutionOrder { get; set; }
        public string TriggetKey { get; set; }
        public string TargetPrefix { get; set; }
        public string RootAwsUrl { get; set; }
    }
}