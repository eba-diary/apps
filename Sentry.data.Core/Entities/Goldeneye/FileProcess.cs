using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class FileProcess
    {
        public FileProcess(string fileName)
        {
            this.fileName = fileName;
            this.started = false;
            this.fileCorrectlyDeleted = false;
        }

        public string fileName { get; set; }
        public Boolean started { get; set; }
        public Boolean fileCorrectlyDeleted { get; set; }
    }
}
