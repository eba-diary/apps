using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    class SASProvider //: IConsumptionLayerProvider
    {
        //private ConsumptionLayerComponent sasComp;

        //public SASProvider() { }

        //public SASProvider(ConsumptionLayerComponent clc)
        //{
        //    sasComp = clc;
        //}

        //public ConsumptionLayerComponent GetComponent()
        //{
        //    ComponentElement innerE = new ComponentElement("Service 1", 1, "link");
        //    ComponentElement innerE2 = new ComponentElement("Service 2", 2, "link");
        //    ComponentElement innerE3 = new ComponentElement("Service 3", 1, "link");

        //    ComponentElement ce = new ComponentElement("SASAppOA", "link");
        //    ce.Elements.Add(innerE);

        //    ComponentElement ce2 = new ComponentElement("SASApp", "link");
        //    ce2.Elements.Add(innerE);
        //    ce2.Elements.Add(innerE2);
        //    ce2.Elements.Add(innerE3);

        //    ComponentElement ce3 = new ComponentElement("ExecSASApp", "link");
        //    ce3.Elements.Add(innerE2);
        //    ce3.Elements.Add(innerE3);

        //    sasComp.ComponentElements.Add(ce);
        //    sasComp.ComponentElements.Add(ce2);
        //    sasComp.ComponentElements.Add(ce3);

        //    int compStatusCount = 0;

        //    foreach (ComponentElement a in sasComp.ComponentElements)
        //    {
        //        int eStatusCount = 0;

        //        foreach (ComponentElement b in a.Elements)
        //        {
        //            eStatusCount += b.Status;
        //        }

        //        if (eStatusCount == a.Elements.Count)
        //        {
        //            a.Status = 1;
        //        }
        //        else if (eStatusCount == a.Elements.Count * 2)
        //        {
        //            a.Status = 2;
        //        }
        //        else
        //        {
        //            a.Status = 3;
        //        }

        //        compStatusCount += a.Status;
        //    }

        //    if (compStatusCount == sasComp.ComponentElements.Count)
        //    {
        //        sasComp.Status = 1;
        //    }
        //    else if (compStatusCount == sasComp.ComponentElements.Count * 2)
        //    {
        //        sasComp.Status = 2;
        //    }
        //    else
        //    {
        //        sasComp.Status = 3;
        //    }

        //    return sasComp;
        //}
    }
}
