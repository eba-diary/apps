namespace Sentry.data.Core
{
    public class DaleAdvancedCriteriaDto
    {

        public string Asset { get; set; }
        public bool AssetIsEmpty { get; set; }

        public string Server { get; set; }
        public bool ServerIsEmpty { get; set; }

        public string Database { get; set; }
        public bool DatabaseIsEmpty { get; set; }

        public string Object { get; set; }
        public bool ObjectIsEmpty { get; set; }

        public string ObjectType { get; set; }
        public bool ObjectTypeIsEmpty { get; set; }

        public string Column { get; set; }
        public bool ColumnIsEmpty { get; set; }


        public string SourceType { get; set; }
        public bool SourceTypeIsEmpty { get; set; }

    }
}
