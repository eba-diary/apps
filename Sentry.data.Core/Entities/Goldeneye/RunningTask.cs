using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RunningTask
    {
        public RunningTask(Task task, String name)
        {
            this.Task = task;
            this.Name = name;
            this.TimeStarted = DateTime.Now;
        }

        public Task Task { get; set; }
        public string Name { get; set; }
        public DateTime TimeStarted { get; set; }
    }
}
