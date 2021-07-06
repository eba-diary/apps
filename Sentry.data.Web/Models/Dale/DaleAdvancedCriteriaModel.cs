using System;

namespace Sentry.data.Web
{
    public class DaleAdvancedCriteriaModel
    {
        public string Asset { get; set; }
        public bool AssetIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Asset); }
        }

        public string Server { get; set; }
        public bool ServerIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Server); }
        }


        public string Database { get; set; }
        public bool DatabaseIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Database); }
        }

        public string Object { get; set; }
        public bool ObjectIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Object); }
        }

        public string ObjectType { get; set; }
        public bool ObjectTypeIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(ObjectType); }
        }
        public string Column { get; set; }
        public bool ColumnIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(Column); }
        }


        public string SourceType { get; set; }
        public bool SourceTypeIsEmpty
        {
            get { return String.IsNullOrWhiteSpace(SourceType); }
        }
    }
}