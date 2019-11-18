using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces.DataProcessing
{
    public interface ISchemaLoadProvider : IBaseActionProvider
    {
        /// <summary>
        /// Return SchemaID associated with storage code within s3 key.  Will return empty string if not found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetStorageCodeFromKey(string key);
    }
}
