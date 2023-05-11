using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SearchHighlightableTests
    {
        [TestMethod]
        public void MergeSearchHighlights_NoneExist()
        {
            SearchHighlightable highlightable = new GlobalDataset();

            List<SearchHighlight> highlights = new List<SearchHighlight> 
            {
                new SearchHighlight
                {
                    PropertyName = "Name",
                    Highlights = new List<string> { "Value" }
                },
                new SearchHighlight
                {
                    PropertyName = "Name",
                    Highlights = new List<string> { "Value", "Value 2" }
                }
            };

            highlightable.MergeSearchHighlights(highlights);

            Assert.AreEqual(1, highlightable.SearchHighlights.Count);

            SearchHighlight searchHighlight = highlightable.SearchHighlights[0];
            Assert.AreEqual("Name", searchHighlight.PropertyName);
            Assert.AreEqual(2, searchHighlight.Highlights.Count);
            Assert.AreEqual("Value", searchHighlight.Highlights[0]);
            Assert.AreEqual("Value 2", searchHighlight.Highlights[1]);
        }

        [TestMethod]
        public void MergeSearchHighlights_SearchHighlighExists()
        {
            SearchHighlightable highlightable = new GlobalDataset
            {
                SearchHighlights = new List<SearchHighlight>
                {
                    new SearchHighlight
                    {
                        PropertyName = "Name",
                        Highlights = new List<string> { "Value" }
                    }
                }
            };

            List<SearchHighlight> highlights = new List<SearchHighlight>
            {
                new SearchHighlight
                {
                    PropertyName = "Name",
                    Highlights = new List<string> { "Value 2" }
                },
                new SearchHighlight
                {
                    PropertyName = "Other",
                    Highlights = new List<string> { "Value 3" }
                }
            };

            highlightable.MergeSearchHighlights(highlights);

            Assert.AreEqual(2, highlightable.SearchHighlights.Count);

            SearchHighlight searchHighlight = highlightable.SearchHighlights[0];
            Assert.AreEqual("Name", searchHighlight.PropertyName);
            Assert.AreEqual(2, searchHighlight.Highlights.Count);
            Assert.AreEqual("Value", searchHighlight.Highlights[0]);
            Assert.AreEqual("Value 2", searchHighlight.Highlights[1]);

            searchHighlight = highlightable.SearchHighlights[1];
            Assert.AreEqual("Other", searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("Value 3", searchHighlight.Highlights[0]);
        }
    }
}
