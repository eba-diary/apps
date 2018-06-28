using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ConsumptionLayerType
    {
        private int id;
        private string name;
        private string childDescription;
        private string code;
        private string tool;
        private string toolDescription;
        private string color;

        public ConsumptionLayerType() { }

        public virtual int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public virtual string ChildDescription
        {
            get
            {
                return childDescription;
            }

            set
            {
                childDescription = value;
            }
        }

        public virtual string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public virtual string Tool
        {
            get
            {
                return tool;
            }

            set
            {
                tool = value;
            }
        }

        public virtual string ToolDescription
        {
            get
            {
                return toolDescription;
            }

            set
            {
                toolDescription = value;
            }
        }

        public virtual string Color
        {
            get
            {
                return color;
            }

            set
            {
                color = value;
            }
        }
    }
}
