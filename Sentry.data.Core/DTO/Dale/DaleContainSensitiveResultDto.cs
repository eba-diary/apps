using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleContainSensitiveResultDto
    {
        public bool DoesContainSensitiveResults { get; set; }
        public DaleEventDto DaleEvent { get; set; }
    }
}
