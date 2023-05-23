namespace Sentry.data.Core
{
    public class ConnectorCreateResponseDto
    {
        public bool SuccessStatusCode { get; set; }
        public string SuccessStatusCodeDescription { get; set; }
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
    }
}
