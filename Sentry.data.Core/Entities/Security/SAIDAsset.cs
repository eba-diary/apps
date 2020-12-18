using System.Collections.Generic;

namespace Sentry.data.Core.Entities
{
    public class SAIDAsset
    {
        public SAIDAsset()
        {
            Roles = new List<SAIDRole>();
        }
        public int? Id { get; set; }
        public string Name { get; set; }
        public string SaidKeyCode { get; set; }
        public int? SosTeamId { get; set; }
        public bool? AssetOfRisk { get; set; }
        public string DataClassification { get; set; }
        public string SystemIsolation { get; set; }
        public string ChargeableUnit { get; set; }
        public string AwsLocation { get; set; }
        public List<SAIDRole> Roles { get; set; }
    }
}
