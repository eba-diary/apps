using System.Collections.Generic;

namespace Sentry.data.Core.Entities.S3.S3AccessPointPolicy
{
    public class S3AccessPointPolicy
    {
        public string Version { get; set; } = "2012-10-17";
        public List<Statement> Statement { get; set; } = new List<Statement>();
    }

    public class Statement
    {
        public string Sid { get; set; }
        public string Effect { get; set; }
        public Principal Principal { get; set; }
        public List<string> Action { get; set; } = new List<string>();
        public object Resource { get; set; }
    }

    public class Principal
    {
        public List<string> AWS { get; set; } = new List<string>();
    }
}
