using System.Text;

namespace Sentry.data.Core.Helpers
{
    public static class JiraHelper
    {
        public static string Format_Bold(string text)
        {
            return $"*{text}*";
        }

        public static string Format_PreFormatted(string text)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{noformat}");
            sb.AppendLine(text);
            sb.Append("{noformat}");
            return sb.ToString();
        }

        public static string Format_JsonCodeBlock(string jsonstring)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{code:json}");
            sb.AppendLine(jsonstring);
            sb.Append("{code}");
            return sb.ToString();
        }

        public static string Format_BulletListItem(string text)
        {
            return $"* {text}";
        }
    }
}
