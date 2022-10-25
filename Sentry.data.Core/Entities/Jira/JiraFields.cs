using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Jira
{
    /// <summary>
    /// Class to hold information about jira issue fields
    /// </summary>
    [JsonConverter(typeof(JiraFieldConverter))]
    public class JiraFields : DynamicObject
    {

        public Dictionary<string, object> fields = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return fields.TryGetValue(binder.Name.ToLower(), out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            fields[binder.Name.ToLower()] = value;
            return true;
        }

    }
    /// <summary>
    /// Generic class to hold information about a jira field
    /// </summary>
    public class GenericJiraField
    {
        /// <summary>
        /// Generic ID field
        /// </summary>
        [JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        /// <summary>
        /// Generic Name field
        /// </summary>
        [JsonProperty("name", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    /// <summary>
    /// Converter for JiraFields to convert dynamic field list to JSON
    /// </summary>
    public class JiraFieldConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(JiraFields))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            JiraFields fields = (JiraFields)value;
            writer.WriteStartObject();
            foreach (var field in fields.fields)
            {
                writer.WritePropertyName(field.Key);
                serializer.Serialize(writer, field.Value);
            }
            writer.WriteEndObject();
        }
    }
}
