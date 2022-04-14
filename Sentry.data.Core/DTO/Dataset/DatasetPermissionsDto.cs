using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// A DTO used to return results from <see cref="DatasetService.GetDatasetPermissions(int)"/>
    /// </summary>
    public class DatasetPermissionsDto
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; }    
        public string DatasetSaidKeyCode { get; set; }
        public IList<SecurablePermission> Permissions { get; set; }
    }
}
