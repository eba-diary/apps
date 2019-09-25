using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface ISchemaField
    {
        int FieldId { get; set; }
        string Name { get; set; }
        DateTime CreateDTM { get; set; }
        DateTime LastUpdateDTM { get; set; }
    }
}
