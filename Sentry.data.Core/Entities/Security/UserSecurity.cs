using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class UserSecurity
    {

        public UserSecurity() { }


        public bool CanPreviewDataset { get; set; }
        public bool CanViewFullDataset { get; set; }
        public bool CanQueryDataset { get; set; }
        public bool CanConnectToDataset { get; set; }
        public bool CanUploadToDataset { get; set; }




        public bool CanEditDataset { get; set; }
        public bool CanCreateDataset { get; set; }

        public bool CanEditReport { get; set; }
    }
}
