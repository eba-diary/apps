using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleAdvancedCriteriaDto
    {

        public string Asset { get; set; }
        public bool AssetIsValid { get; set; }

        public string Server { get; set; }
        public bool ServerIsValid { get; set; }

        public string Database { get; set; }
        public bool DatabaseIsValid { get; set; }

        public string Object { get; set; }
        public bool ObjectIsValid { get; set; }

        public string ObjectType { get; set; }
        public bool ObjectTypeIsValid { get; set; }

        public string Column { get; set; }
        public bool ColumnIsValid { get; set; }


        public string SourceType { get; set; }
        public bool SourceTypeIsValid { get; set; }

        public string ToEventString()
        {
            List<string> criterias = new List<string>();

            if (!string.IsNullOrWhiteSpace(Asset))
            {
                criterias.Add($"Asset:{Asset}");
            }

            if (!string.IsNullOrWhiteSpace(Server))
            {
                criterias.Add($"Server:{Server}");
            }

            if (!string.IsNullOrWhiteSpace(Database))
            {
                criterias.Add($"Database:{Database}");
            }

            if (!string.IsNullOrWhiteSpace(Object))
            {
                criterias.Add($"Object:{Object}");
            }

            if (!string.IsNullOrWhiteSpace(ObjectType))
            {
                criterias.Add($"ObjectType:{ObjectType}");
            }

            if (!string.IsNullOrWhiteSpace(Column))
            {
                criterias.Add($"Column:{Column}");
            }

            return string.Join(" AND ", criterias);
        }

    }
}
