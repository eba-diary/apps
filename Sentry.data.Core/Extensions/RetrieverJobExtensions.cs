using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class RetrieverJobExtensions
    {
        public static void ToHszDropCreateModel(this RetrieverJob rj, HszDropLocationCreateModel model)
        {
            model.DropLocation = rj.GetUri().ToString();
        }
    }
}
