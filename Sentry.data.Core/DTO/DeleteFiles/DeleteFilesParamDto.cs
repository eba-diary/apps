using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DeleteFilesParamDto
    {
        public string[] UserFileNameList { get; set; }
        public int[] UserFileIdList { get; set; }

    }
}
