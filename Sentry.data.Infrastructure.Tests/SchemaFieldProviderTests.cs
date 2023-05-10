using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class SchemaFieldProviderTests
    {
        [TestMethod]
        public async Task SearchSchemaFieldsWithHighlightingAsync_WithDatasetIds_ElasticSchemaFields()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> documentClient = mr.Create<IElasticDocumentClient>();

            Mock<IHit<ElasticSchemaField>> hit = mr.Create<IHit<ElasticSchemaField>>();
            ElasticSchemaField schemaField = new ElasticSchemaField();
            hit.SetupGet(x => x.Source).Returns(schemaField);
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "Name", new List<string> { "Field1" } }
            };
            hit.SetupGet(x => x.Highlight).Returns(highlight);

            Mock<IHit<ElasticSchemaField>> hit2 = mr.Create<IHit<ElasticSchemaField>>();
            ElasticSchemaField schemaField2 = new ElasticSchemaField();
            hit2.SetupGet(x => x.Source).Returns(schemaField2);
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight2 = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "Name", new List<string> { "Field2" } }
            };
            hit2.SetupGet(x => x.Highlight).Returns(highlight2);

            ElasticResult<ElasticSchemaField> elasticResult = new ElasticResult<ElasticSchemaField>
            {
                Hits = new List<IHit<ElasticSchemaField>> { hit.Object, hit2.Object }
            };

            documentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<ElasticSchemaField>>())).ReturnsAsync(elasticResult).Callback<SearchRequest<ElasticSchemaField>>(x =>
            {
                IBoolQuery query = ((IQueryContainer)x.Query).Bool;
                Assert.AreEqual(2, query.Should.Count());

                IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
                Assert.AreEqual("search", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
                Assert.AreEqual("*search*", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                Assert.AreEqual(1, query.Filter.Count());

                ITermsQuery terms = ((IQueryContainer)query.Filter.First()).Terms;
                Assert.AreEqual(2, terms.Terms.Count());
                Assert.AreEqual("1", terms.Terms.First().ToString());
                Assert.AreEqual("2", terms.Terms.Last().ToString());

                Assert.AreEqual(10000, x.Size);
                Assert.IsNotNull(x.Highlight);

                IHighlight requestHighlight = x.Highlight;
                Assert.AreEqual(1, requestHighlight.Fields.Count);
                Assert.AreEqual("Name", requestHighlight.Fields.Keys.First().Name);
            });

            SchemaFieldProvider schemaFieldProvider = new SchemaFieldProvider(documentClient.Object);

            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                DatasetIds = new List<int> { 1, 2 },
                SearchText = "search"
            };

            List<ElasticSchemaField> schemaFields = await schemaFieldProvider.SearchSchemaFieldsWithHighlightingAsync(searchSchemaFieldsDto);

            Assert.AreEqual(2, schemaFields.Count);

            ElasticSchemaField resultField = schemaFields[0];
            Assert.AreEqual(1, resultField.SearchHighlights.Count);

            SearchHighlight searchHighlight = resultField.SearchHighlights[0];
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("Field1", searchHighlight.Highlights[0]);

            resultField = schemaFields[1];
            Assert.AreEqual(1, resultField.SearchHighlights.Count);

            searchHighlight = resultField.SearchHighlights[0];
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("Field2", searchHighlight.Highlights[0]);
        }

        [TestMethod]
        public async Task SearchSchemaFieldsWithHighlightingAsync_NoDatasetIds_ElasticSchemaFields()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> documentClient = mr.Create<IElasticDocumentClient>();

            Mock<IHit<ElasticSchemaField>> hit = mr.Create<IHit<ElasticSchemaField>>();
            ElasticSchemaField schemaField = new ElasticSchemaField();
            hit.SetupGet(x => x.Source).Returns(schemaField);
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "Name", new List<string> { "Field1" } }
            };
            hit.SetupGet(x => x.Highlight).Returns(highlight);

            Mock<IHit<ElasticSchemaField>> hit2 = mr.Create<IHit<ElasticSchemaField>>();
            ElasticSchemaField schemaField2 = new ElasticSchemaField();
            hit2.SetupGet(x => x.Source).Returns(schemaField2);
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlight2 = new Dictionary<string, IReadOnlyCollection<string>>
            {
                { "Name", new List<string> { "Field2" } }
            };
            hit2.SetupGet(x => x.Highlight).Returns(highlight2);

            ElasticResult<ElasticSchemaField> elasticResult = new ElasticResult<ElasticSchemaField>
            {
                Hits = new List<IHit<ElasticSchemaField>> { hit.Object, hit2.Object }
            };

            documentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<ElasticSchemaField>>())).ReturnsAsync(elasticResult).Callback<SearchRequest<ElasticSchemaField>>(x =>
            {
                IBoolQuery query = ((IQueryContainer)x.Query).Bool;
                Assert.AreEqual(2, query.Should.Count());

                IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
                Assert.AreEqual("search", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
                Assert.AreEqual("*search*", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                Assert.IsNull(query.Filter);

                Assert.AreEqual(10000, x.Size);
                Assert.IsNotNull(x.Highlight);

                IHighlight requestHighlight = x.Highlight;
                Assert.AreEqual(1, requestHighlight.Fields.Count);
                Assert.AreEqual("Name", requestHighlight.Fields.Keys.First().Name);
            });

            SchemaFieldProvider schemaFieldProvider = new SchemaFieldProvider(documentClient.Object);

            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                DatasetIds = new List<int>(),
                SearchText = "search"
            };

            List<ElasticSchemaField> schemaFields = await schemaFieldProvider.SearchSchemaFieldsWithHighlightingAsync(searchSchemaFieldsDto);

            Assert.AreEqual(2, schemaFields.Count);

            ElasticSchemaField resultField = schemaFields[0];
            Assert.AreEqual(1, resultField.SearchHighlights.Count);

            SearchHighlight searchHighlight = resultField.SearchHighlights[0];
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("Field1", searchHighlight.Highlights[0]);

            resultField = schemaFields[1];
            Assert.AreEqual(1, resultField.SearchHighlights.Count);

            searchHighlight = resultField.SearchHighlights[0];
            Assert.AreEqual(SearchDisplayNames.SchemaField.COLUMNNAME, searchHighlight.PropertyName);
            Assert.AreEqual(1, searchHighlight.Highlights.Count);
            Assert.AreEqual("Field2", searchHighlight.Highlights[0]);
        }

        [TestMethod]
        public async Task SearchSchemaFieldsAsync_ElasticSchemaFields()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IElasticDocumentClient> documentClient = mr.Create<IElasticDocumentClient>();

            ElasticResult<ElasticSchemaField> elasticResult = new ElasticResult<ElasticSchemaField>
            {
                Documents = new List<ElasticSchemaField> { new ElasticSchemaField(), new ElasticSchemaField() }
            };

            documentClient.Setup(x => x.SearchAsync(It.IsAny<SearchRequest<ElasticSchemaField>>())).ReturnsAsync(elasticResult).Callback<SearchRequest<ElasticSchemaField>>(x =>
            {
                IBoolQuery query = ((IQueryContainer)x.Query).Bool;
                Assert.AreEqual(2, query.Should.Count());

                IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
                Assert.AreEqual("search", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
                Assert.AreEqual("*search*", stringQuery.Query);
                Assert.AreEqual(1, stringQuery.Fields.Count());
                Assert.IsTrue(stringQuery.Fields.Any(f => f.Name == "Name"));

                Assert.IsNull(query.Filter);

                Assert.AreEqual(10000, x.Size);
                Assert.IsNull(x.Highlight);
            });

            SchemaFieldProvider schemaFieldProvider = new SchemaFieldProvider(documentClient.Object);

            SearchSchemaFieldsDto searchSchemaFieldsDto = new SearchSchemaFieldsDto
            {
                SearchText = "search"
            };

            List<ElasticSchemaField> schemaFields = await schemaFieldProvider.SearchSchemaFieldsAsync(searchSchemaFieldsDto);

            Assert.AreEqual(2, schemaFields.Count);

            ElasticSchemaField resultField = schemaFields[0];
            Assert.IsNull(resultField.SearchHighlights);

            resultField = schemaFields[1];
            Assert.IsNull(resultField.SearchHighlights);
        }
    }
}
