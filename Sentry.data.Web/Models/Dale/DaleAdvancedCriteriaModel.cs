using System.Linq;

namespace Sentry.data.Web
{
    //NOTE!!!!  these READ ONLY properties determine if param is valid based on if empty or not
    //Its really the models responsiblity to determine if the param is valid or not, we could change this in future pending different changes and backend will not change
    public class DaleAdvancedCriteriaModel
    {
        public string Asset { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Object { get; set; }
        public string ObjectType { get; set; }
        public string Column { get; set; }
        public string SourceType { get; set; }

        public bool IsValid()
        {
            return GetType().GetProperties().Where(x => x.PropertyType == typeof(string)).All(x => !string.IsNullOrWhiteSpace(x.GetValue(this).ToString()));
        }
    }
}