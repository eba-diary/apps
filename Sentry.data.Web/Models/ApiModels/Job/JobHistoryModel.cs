namespace Sentry.data.Web.Models.ApiModels.Job
{
    public class JobHistoryModel
    {
        public int History_Id { get; set; }
        public int Job_Id { get; set; }
        public int Batch_Id { get; set; }
        public string State { get; set; }
        public string LivyAppId { get; set; }
        public string LivyDriverLogUrl { get; set; }
        public string LivySparkUiUrl { get; set; }
        public string LogInfo { get; set; }
        public string Created_DTM { get; set; }
        public string Modified_DTM { get; set; }
        public bool Active { get; set; }
    }
}