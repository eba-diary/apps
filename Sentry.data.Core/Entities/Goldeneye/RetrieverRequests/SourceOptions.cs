using System;

namespace Sentry.data.Core
{
    [Serializable]
    public class SourceOptions
    {
        public string JarFile { get; set; }
        public string ClassName { get; set; }
        public string[] JarDepenencies { get; set; }
    }
}