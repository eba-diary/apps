using Sentry.data.Core.GlobalEnums;
using System;
using System.IO;

namespace Sentry.data.Core
{
    public class DfsDataFlowBasic : DfsSource
    {
        public DfsDataFlowBasic()
        
        {
            IsUriEditable = false;
            BaseUri = new Uri($"{BaseUri}DatasetLoader/");
        }

        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION;
            }
        }

        public override Uri CalcRelativeUri(RetrieverJob Job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
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
