using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Factories.Fields
{
    public abstract class FieldDtoFactory
    {
        public abstract BaseFieldDto GetField();
    }
}
