using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.Migration
{
    public class MigrationResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool WasMigrated { get; set; }
        public string MigrationNotes { get; set; }
    }
}