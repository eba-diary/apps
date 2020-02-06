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
        private ISchemaMapProvider _schemaMapProvider;

        public SchemaMapAction() { }

        public SchemaMapAction(ISchemaMapProvider schemaMapProvider) : base(schemaMapProvider)
        {
            _schemaMapProvider = schemaMapProvider;
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.SCHEMA_MAP_PREFIX;
        }
    }
}
