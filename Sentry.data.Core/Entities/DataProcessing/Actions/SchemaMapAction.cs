using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaMapAction : BaseAction
    {
       public SchemaMapAction() { }

        public SchemaMapAction(ISchemaMapProvider schemaMapProvider) : base(schemaMapProvider) { }
    }
}
