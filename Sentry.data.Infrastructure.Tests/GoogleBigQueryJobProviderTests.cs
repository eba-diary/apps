using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Net;
using Moq.Protected;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using NHibernate.Mapping;
using Sentry.data.Core.Entities.DataProcessing;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;
using Nest;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GoogleBigQueryJobProviderTests
    {
        [TestMethod]
        public void Execute_RetrieverJob_FieldsNotFound()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            HTTPSSource httpsSource = new HTTPSSource() { BaseUri = new Uri("https://bigquery.googleapis.com/bigquery/v2/") };

            Mock<IAuthorizationProvider> authorizationProvider = repository.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(httpsSource)).Returns("token");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage httpResponseMessageNotFound = GetResponseMessage(HttpStatusCode.NotFound, "NotFound");

            string fieldsUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{DateTime.Today:yyyyMMdd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == fieldsUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageNotFound);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> httpClientGenerator = repository.Create<IHttpClientGenerator>();
            httpClientGenerator.Setup(x => x.GenerateHttpClient()).Returns(httpClient);

            GoogleBigQueryJobProvider provider = new GoogleBigQueryJobProvider(null, null, authorizationProvider.Object, httpClientGenerator.Object, null);

            string datePartition = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

            RetrieverJob job = new RetrieverJob()
            {
                DataSource = httpsSource,
                RelativeUri = $"projects/project1/datasets/dataset1/tables/events_{datePartition}/data",
            };

            provider.Execute(job);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains("Authorization"));
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.GetValues("Authorization").First());

            repository.VerifyAll();
        }

        [TestMethod]
        public void Execute_RetrieverJob_SinglePage()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            DateTime today = DateTime.Today;

            HTTPSSource httpsSource = new HTTPSSource() { BaseUri = new Uri("https://bigquery.googleapis.com/bigquery/v2/") };

            Mock<IAuthorizationProvider> authorizationProvider = repository.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(httpsSource)).Returns("token");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();

            HttpResponseMessage httpResponseMessageFields = GetResponseMessage(HttpStatusCode.OK, "EventsSchema");
            HttpResponseMessage httpResponseMessageData = GetResponseMessage(HttpStatusCode.OK, "Data_NoPageToken");
            HttpResponseMessage httpResponseMessageNotFound = GetResponseMessage(HttpStatusCode.NotFound, "NotFound");

            string requestDatePartition = today.AddDays(-1).ToString("yyyyMMdd");
            string fieldsUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{requestDatePartition}";
            string dataUri = fieldsUri + "/data";
            string dataLastUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{today:yyyyMMdd}/data";

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == fieldsUri), 
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageFields);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataLastUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageNotFound);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> httpClientGenerator = repository.Create<IHttpClientGenerator>();
            httpClientGenerator.Setup(x => x.GenerateHttpClient()).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            DataFlow dataFlow = new DataFlow() { Id = 1, SchemaId = 2 };
            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerBucket = "bucket",
                TriggerKey = "key/"
            };

            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep>() { dataFlowStep }.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IS3ServiceProvider> s3ServiceProvider = repository.Create<IS3ServiceProvider>();
            s3ServiceProvider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), "bucket", It.Is<string>(s => s.StartsWith($"key/testfile_events_{requestDatePartition}_")))).Returns("");

            Mock<IGoogleBigQueryService> bigQueryService = repository.Create<IGoogleBigQueryService>();
            bigQueryService.Setup(x => x.UpdateSchemaFields(2, It.IsAny<JArray>()));

            GoogleBigQueryJobProvider provider = new GoogleBigQueryJobProvider(datasetContext.Object, s3ServiceProvider.Object, authorizationProvider.Object, httpClientGenerator.Object, bigQueryService.Object);

            string datePartition = today.AddDays(-2).ToString("yyyyMMdd");

            RetrieverJob job = new RetrieverJob()
            {
                DataSource = httpsSource,
                RelativeUri = $"projects/project1/datasets/dataset1/tables/events_{datePartition}/data",
                DataFlow = dataFlow,
                JobOptions = new RetrieverJobOptions()
                {
                    TargetFileName = "testfile"
                }
            };

            provider.Execute(job);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains("Authorization"));
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.GetValues("Authorization").First());
            Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{requestDatePartition}/data", job.RelativeUri);
            Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
            Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);

            repository.VerifyAll();
        }

        [TestMethod]
        public void Execute_RetrieverJob_MultiplePages()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            DateTime today = DateTime.Today;

            HTTPSSource httpsSource = new HTTPSSource() { BaseUri = new Uri("https://bigquery.googleapis.com/bigquery/v2/") };

            Mock<IAuthorizationProvider> authorizationProvider = repository.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(httpsSource)).Returns("token");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();

            HttpResponseMessage httpResponseMessageFields = GetResponseMessage(HttpStatusCode.OK, "EventsSchema");
            HttpResponseMessage httpResponseMessageData = GetResponseMessage(HttpStatusCode.OK, "Data_PageToken");
            HttpResponseMessage httpResponseMessageData2 = GetResponseMessage(HttpStatusCode.OK, "Data_PageTokenLast");
            HttpResponseMessage httpResponseMessageNotFound = GetResponseMessage(HttpStatusCode.NotFound, "NotFound");

            string requestDatePartition = today.AddDays(-1).ToString("yyyyMMdd");
            string fieldsUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{requestDatePartition}";
            string dataUri = fieldsUri + "/data";
            string dataUri2 = dataUri + "?pageToken=BFDECMLBQMAQAAASAUIIBAEAAUNAICAFCACSB77777777777757SUAA%3D";
            string dataLastUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{today:yyyyMMdd}/data";

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == fieldsUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageFields);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataUri2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData2);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataLastUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageNotFound);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> httpClientGenerator = repository.Create<IHttpClientGenerator>();
            httpClientGenerator.Setup(x => x.GenerateHttpClient()).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            DataFlow dataFlow = new DataFlow() { Id = 1, SchemaId = 2 };
            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerBucket = "bucket",
                TriggerKey = "key/"
            };

            string datePartition = today.AddDays(-2).ToString("yyyyMMdd");

            RetrieverJob job = new RetrieverJob()
            {
                DataSource = httpsSource,
                RelativeUri = $"projects/project1/datasets/dataset1/tables/events_{datePartition}/data",
                DataFlow = dataFlow,
                JobOptions = new RetrieverJobOptions()
                {
                    TargetFileName = "testfile"
                }
            };

            int count = 0;

            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep>() { dataFlowStep }.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true)).Callback(() =>
            {
                if (count == 0)
                {
                    Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{requestDatePartition}/data", job.RelativeUri);
                    Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
                    Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);
                }
                count++;
            });

            string firstKey = "";

            Mock<IS3ServiceProvider> s3ServiceProvider = repository.Create<IS3ServiceProvider>();
            s3ServiceProvider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), "bucket", It.Is<string>(s => s.StartsWith($"key/testfile_events_{requestDatePartition}_"))))
                .Callback<Stream, string, string>((stream, bucket, key) =>
                {
                    if (string.IsNullOrEmpty(firstKey))
                    {
                        firstKey = key;
                    }
                    else
                    {
                        Assert.AreNotEqual(firstKey, key);
                    }
                })
                .Returns("");

            Mock<IGoogleBigQueryService> bigQueryService = repository.Create<IGoogleBigQueryService>();
            bigQueryService.Setup(x => x.UpdateSchemaFields(2, It.IsAny<JArray>()));

            GoogleBigQueryJobProvider provider = new GoogleBigQueryJobProvider(datasetContext.Object, s3ServiceProvider.Object, authorizationProvider.Object, httpClientGenerator.Object, bigQueryService.Object);

            provider.Execute(job);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains("Authorization"));
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.GetValues("Authorization").First());
            Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{requestDatePartition}/data", job.RelativeUri);
            Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
            Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);

            repository.VerifyAll();
        }

        [TestMethod]
        public void Execute_RetrieverJob_MultiplePartitions()
        {
            MockRepository repository = new MockRepository(MockBehavior.Strict);

            DateTime today = DateTime.Today;

            HTTPSSource httpsSource = new HTTPSSource() { BaseUri = new Uri("https://bigquery.googleapis.com/bigquery/v2/") };

            Mock<IAuthorizationProvider> authorizationProvider = repository.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(httpsSource)).Returns("token");

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();

            HttpResponseMessage httpResponseMessageFields = GetResponseMessage(HttpStatusCode.OK, "EventsSchema");
            HttpResponseMessage httpResponseMessageData = GetResponseMessage(HttpStatusCode.OK, "Data_PageToken");
            HttpResponseMessage httpResponseMessageData2 = GetResponseMessage(HttpStatusCode.OK, "Data_PageTokenLast");
            HttpResponseMessage httpResponseMessageData3 = GetResponseMessage(HttpStatusCode.OK, "Data_NoPageToken");
            HttpResponseMessage httpResponseMessageNotFound = GetResponseMessage(HttpStatusCode.NotFound, "NotFound");

            string firstDatePartition = today.AddDays(-2).ToString("yyyyMMdd");
            string baseUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_";
            string fieldsUri = $"{baseUri}{firstDatePartition}";
            string dataUri = fieldsUri + "/data";
            string dataUri2 = dataUri + "?pageToken=BFDECMLBQMAQAAASAUIIBAEAAUNAICAFCACSB77777777777757SUAA%3D";
            string secondDatePartition = today.AddDays(-1).ToString("yyyyMMdd");
            string secondDataUri = $"{baseUri}{secondDatePartition}/data";
            string dataLastUri = $"https://bigquery.googleapis.com/bigquery/v2/projects/project1/datasets/dataset1/tables/events_{today:yyyyMMdd}/data";

            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == fieldsUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageFields);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataUri2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData2);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == secondDataUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageData3);
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == dataLastUri),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(httpResponseMessageNotFound);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object);

            Mock<IHttpClientGenerator> httpClientGenerator = repository.Create<IHttpClientGenerator>();
            httpClientGenerator.Setup(x => x.GenerateHttpClient()).Returns(httpClient);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            DataFlow dataFlow = new DataFlow() { Id = 1, SchemaId = 2 };
            DataFlowStep dataFlowStep = new DataFlowStep()
            {
                DataFlow = dataFlow,
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerBucket = "bucket",
                TriggerKey = "key/"
            };

            string datePartition = today.AddDays(-3).ToString("yyyyMMdd");

            RetrieverJob job = new RetrieverJob()
            {
                DataSource = httpsSource,
                RelativeUri = $"projects/project1/datasets/dataset1/tables/events_{datePartition}/data",
                DataFlow = dataFlow,
                JobOptions = new RetrieverJobOptions()
                {
                    TargetFileName = "testfile"
                }
            };

            int count = 0;

            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep>() { dataFlowStep }.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true)).Callback(() =>
            {
                if (count == 0)
                {
                    Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{firstDatePartition}/data", job.RelativeUri);
                    Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
                    Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);
                }

                if (count == 1)
                {
                    Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{firstDatePartition}/data", job.RelativeUri);
                    Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
                    Assert.AreEqual("10", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);
                }

                count++;
            });

            string firstKey = "";

            Mock<IS3ServiceProvider> s3ServiceProvider = repository.Create<IS3ServiceProvider>();
            s3ServiceProvider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), "bucket", It.Is<string>(s => s.StartsWith($"key/testfile_events_{firstDatePartition}_"))))
                .Callback<Stream, string, string>((stream, bucket, key) =>
                {
                    if (string.IsNullOrEmpty(firstKey))
                    {
                        firstKey = key;
                    }
                    else
                    {
                        Assert.AreNotEqual(firstKey, key);
                    }
                })
                .Returns("");
            s3ServiceProvider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), "bucket", It.Is<string>(s => s.StartsWith($"key/testfile_events_{secondDatePartition}_")))).Returns("");

            Mock<IGoogleBigQueryService> bigQueryService = repository.Create<IGoogleBigQueryService>();
            bigQueryService.Setup(x => x.UpdateSchemaFields(2, It.IsAny<JArray>()));

            GoogleBigQueryJobProvider provider = new GoogleBigQueryJobProvider(datasetContext.Object, s3ServiceProvider.Object, authorizationProvider.Object, httpClientGenerator.Object, bigQueryService.Object);

            provider.Execute(job);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains("Authorization"));
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.GetValues("Authorization").First());
            Assert.AreEqual($"projects/project1/datasets/dataset1/tables/events_{secondDatePartition}/data", job.RelativeUri);
            Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.LASTINDEX]);
            Assert.AreEqual("5", job.ExecutionParameters[ExecutionParameterKeys.GoogleBigQueryApi.TOTALROWS]);

            repository.VerifyAll();
        }

        [TestMethod]
        public void Execute_RetrieverJob_FailInMiddle()
        {

        }

        [TestMethod]
        public void Execute_RetrieverJob_StartFromFailure()
        {

        }

        #region Helpers
        private string GetBigQueryResponse(string filename)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\GoogleBigQuery_{filename}.json"))
            {
                return rdr.ReadToEnd().Replace("\r\n", string.Empty);
            }
        }

        private HttpResponseMessage GetResponseMessage(HttpStatusCode statusCode, string contentFilename)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(GetBigQueryResponse(contentFilename)),
                StatusCode = statusCode
            };
        }
        #endregion
    }
}
