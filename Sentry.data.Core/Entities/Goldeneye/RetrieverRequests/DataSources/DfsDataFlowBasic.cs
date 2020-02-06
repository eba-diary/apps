using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DfsDataFlowBasic : DfsSource
    {
        public DfsDataFlowBasic()
        {
            IsUriEditable = false;
            BaseUri = new Uri($"{BaseUri.ToString()}DatasetLoader/Job/");
        }
    }
}
