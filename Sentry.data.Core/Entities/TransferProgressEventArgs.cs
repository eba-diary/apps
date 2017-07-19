using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class TransferProgressEventArgs : EventArgs
    {
        public TransferProgressEventArgs(string filePath, int percentDone)
        {
            FilePath = filePath;
            PercentDone = percentDone;
        }
        public string FilePath { get; set; }
        public int PercentDone { get; set; } 
    }
}
