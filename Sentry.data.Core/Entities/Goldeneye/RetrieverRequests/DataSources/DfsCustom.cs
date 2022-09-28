using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DfsCustom : DfsSource
    {
        public DfsCustom()
        {
            IsUriEditable = true;
        }

        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.DFS_CUSTOM;
            }
        }
    }
}
