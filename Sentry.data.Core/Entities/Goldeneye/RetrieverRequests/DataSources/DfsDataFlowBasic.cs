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
                return GlobalConstants.DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION;
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            var locbase = BaseUri.ToString();
            var storagecde = Job.DataFlow.FlowStorageCode;
            Uri u = new Uri(
                Path.Combine(
                    new string[] 
                    {
                        locbase,
                        storagecde
                    }
                ));

            return u;
        }
    }
}
