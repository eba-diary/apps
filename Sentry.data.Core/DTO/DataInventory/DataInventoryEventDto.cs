namespace Sentry.data.Core
{
    public class DataInventoryEventDto
    {
        public string SearchCriteria { get; set; }
        public bool QuerySuccess { get; set; }
        public string QueryErrorMessage { get; set; }

    }
}
