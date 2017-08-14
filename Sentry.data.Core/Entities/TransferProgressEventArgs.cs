using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class TransferProgressEventArgs : EventArgs
    {
        public TransferProgressEventArgs(string filePath, int percentDone, string type)
        {
            FilePath = filePath;
            PercentDone = percentDone;
            Type = type;
        }
        public string FilePath { get; set; }
        public int PercentDone { get; set; } 
        public string Type { get; set; }
    }
}
