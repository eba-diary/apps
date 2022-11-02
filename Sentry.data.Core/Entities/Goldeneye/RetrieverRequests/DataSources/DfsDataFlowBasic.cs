using Sentry.Configuration;
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

        public override Uri CalcRelativeUri(RetrieverJob job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter)
        {
            string fullPath;

            if (string.IsNullOrEmpty(CLA4260_QuartermasterNamedEnvironmentTypeFilter))
            {
                string baseUri = datasetEnvironmentType == NamedEnvironmentType.Prod ? Config.GetHostSetting("DFSDropLocationProd") : Config.GetHostSetting("DFSDropLocationNonProd");
                fullPath = Path.Combine(baseUri, job.DataFlow.SaidKeyCode, job.DataFlow.NamedEnvironment, job.DataFlow.FlowStorageCode);                
            }
            else
            {
                var locbase = BaseUri.ToString();
                var storagecde = job.DataFlow.FlowStorageCode;
                fullPath = Path.Combine(locbase, storagecde);
            }

            return new Uri(fullPath);
        }
    }
}
