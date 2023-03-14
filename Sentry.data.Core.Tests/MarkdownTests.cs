using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class MarkdownTests
    {
        [TestMethod]
        public void AddLine_Basic()
        {
            Markdown markdown = new Markdown();

            markdown.AddLine("Line 1");
            markdown.AddLine("Line 2");

            string result = markdown.ToString();

            Assert.AreEqual("Line 1\r\nLine 2\r\n", result);
        }

        [TestMethod]
        public void Add_Basic()
        {
            Markdown markdown = new Markdown();

            markdown.Add("This ");
            markdown.Add("is a single");
            markdown.Add(" line");

            string result = markdown.ToString();

            Assert.AreEqual("This is a single line", result);
        }

        [TestMethod]
        public void AddList_Basic()
        {
            Markdown markdown = new Markdown();

            List<string> items = new List<string> { "Item 1", "Item 2", "Item 3" };

            markdown.AddList(items);

            string result = markdown.ToString();

            Assert.AreEqual("- Item 1\r\n- Item 2\r\n- Item 3\r\n", result);
        }

        [TestMethod]
        public void AddBreak_Basic()
        {
            Markdown markdown = new Markdown();

            markdown.Add("This has a");
            markdown.AddBreak();
            markdown.Add("break");

            string result = markdown.ToString();

            Assert.AreEqual("This has a\r\nbreak", result);
        }

        [TestMethod]
        public void AddLink_Basic()
        {
            Markdown markdown = new Markdown();

            markdown.AddLink("Google", "https://www.google.com");

            string result = markdown.ToString();

            Assert.AreEqual("[Google|https://www.google.com]", result);
        }

        [TestMethod]
        public void Add_EscapeCharacters()
        {
            Markdown markdown = new Markdown();

            markdown.Add(@"Text - has special characters (-{}*`#\!|[]<>)");

            string result = markdown.ToString();

            Assert.AreEqual(@"Text \- has special characters \(\-\{\}\*\`\#\\\!\|\[\]\<\>\)", result);
        }
    }
}
