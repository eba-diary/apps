using System;

namespace Sentry.data.Web
{
    //NOTE!!!!  these READ ONLY properties determine if param is valid based on if empty or not
    //Its really the models responsiblity to determine if the param is valid or not, we could change this in future pending different changes and backend will not change
    public class DaleAdvancedCriteriaModel
    {
        public string Asset { get; set; }
        public bool AssetIsValid                                    
        {
            get { return !String.IsNullOrWhiteSpace(Asset); }
        }

        public string Server { get; set; }
        public bool ServerIsValid
        {
            get { return !String.IsNullOrWhiteSpace(Server); }
        }


        public string Database { get; set; }
        public bool DatabaseIsValid
        {
            get { return !String.IsNullOrWhiteSpace(Database); }
        }

        public string Object { get; set; }
        public bool ObjectIsValid
        {
            get { return !String.IsNullOrWhiteSpace(Object); }
        }

        public string ObjectType { get; set; }
        public bool ObjectTypeIsValid
        {
            get { return !String.IsNullOrWhiteSpace(ObjectType); }
        }
        public string Column { get; set; }
        public bool ColumnIsValid
        {
            get { return !String.IsNullOrWhiteSpace(Column); }
        }


        public string SourceType { get; set; }
        public bool SourceTypeIsValid
        {
            get { return !String.IsNullOrWhiteSpace(SourceType); }
        }
    }
}