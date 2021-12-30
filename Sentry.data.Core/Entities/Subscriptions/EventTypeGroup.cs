﻿using System;
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
        
        [Description("BUSINESSAREA")]       //CURRENTLY PL IS ONLY BUSINESS AREA, LEAVE GENERIC IN CASE FUTURE BUSINESSAREA IS ADDED THAT SHARES EVENTTYPES
        BusinessArea = 2,

        [Description("BUSINESSAREA_DSC")]   //DSC IS A BUSINESSAREA AS FAR AS NOTIFICATIONS GO BUT HAVE SEPERATE EVENTTYPES
        BusinessAreaDSC = 3
    }
    
}
