using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SearchHighlightTests
    {
        [TestMethod]
        public void AddHighlights_HighlightsExists()
        {
            SearchHighlight searchHighlight = new SearchHighlight
            {
                Highlights = new List<string> { "Value" }
            };

            searchHighlight.AddHighlights(new List<string> { "Value", "Value 2" });

            Assert.AreEqual(2, searchHighlight.Highlights.Count);
            Assert.AreEqual("Value", searchHighlight.Highlights[0]);
            Assert.AreEqual("Value 2", searchHighlight.Highlights[1]);
        }

        [TestMethod]
        public void AddHighlights_HighlightsNotExist()
        {
            SearchHighlight searchHighlight = new SearchHighlight();

            searchHighlight.AddHighlights(new List<string> { "Value", "Value 2" });

            Assert.AreEqual(2, searchHighlight.Highlights.Count);
            Assert.AreEqual("Value", searchHighlight.Highlights[0]);
            Assert.AreEqual("Value 2", searchHighlight.Highlights[1]);
        }
    }
}
