using System;
using System.Collections.Generic;
using System.IO;
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
            BaseUri = new Uri($"{BaseUri.ToString()}DatasetLoader/");
        }
        
        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION;
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            Uri u = new Uri(Path.Combine(new string[] { BaseUri.ToString(), Job.DataFlow.FlowStorageCode }).ToString());

            return u;
        }
    }
}
