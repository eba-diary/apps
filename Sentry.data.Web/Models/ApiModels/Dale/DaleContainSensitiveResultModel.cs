using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleContainSensitiveResultModel
    {
        public bool DoesContainSensitiveResults { get; set; }
        public DaleEventDto DaleEvent { get; set; }
    }
}
