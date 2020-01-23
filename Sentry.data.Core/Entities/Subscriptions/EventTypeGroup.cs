using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Sentry.data.Core
{
    public enum EventTypeGroup
    {
        [Description("DATASET")]   
        DataSet = 1,
        
        [Description("BUSINESSAREA")] 
        BusinessArea = 2
    }
    
}
