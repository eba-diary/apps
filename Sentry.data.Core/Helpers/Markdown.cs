using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public class Markdown
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public void AddLine(string text, bool escapeSpecialCharacters = true)
        {
            if (escapeSpecialCharacters)
            {
                text = Escape(text);
            }

            _builder.AppendLine(text);
        }

        public void Add(string text)
        {
            _builder.Append(Escape(text));
        }

        public void AddList<T>(List<T> items)
        {
            foreach (T item in items)
            {
                _builder.AppendLine($"- {Escape(item.ToString())}");
            }
        }

        public void AddBold(string text)
        {
            _builder.Append("*");
            Add(text);
            _builder.Append("*");
        }

        public string AddBoldInLine(string text)
        {
            return $"*{text}*";
        }

        public void AddBreak()
        {
            _builder.AppendLine();
        }

        public void AddBulletList(List<string> stringList)
        {
            stringList.ForEach(x => _builder.AppendLine($"* {x}"));
        }

        public void AddLink(string text, string url)
        {
            _builder.Append($"[{text}|{url}]");
        }

        public void AddJsonCodeBlock(string jsonstring)
        {            
            AddLine("{code:json}", false);
            AddLine(jsonstring, false);
            AddLine("{code}", false);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        private string Escape(string text)
        {
            return Regex.Replace(text, @"[\\`\*\-\{\}\[\]\<\>\(\)\#\+_\!\|]", x => $@"\{x.Value}");
        }
    }
}
