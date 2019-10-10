using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ActionExecution
    {
        public virtual int Id { get; set; }
        public virtual DataFlow DataFlow { get; set; }
        public virtual BaseAction DataAction { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
    }
}
