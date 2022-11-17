using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.DataSource;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataSourceTests
    {
        [TestMethod]
        public void HttpsDataSourceValidations()
        {
            HTTPSSource httpsSource = new HTTPSSource();
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"

                },
                RelativeUri = "relative_uri_value"
            };

            RetrieverJob retrieverJob_No_Relative_Uri = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.none,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = null
            };
            
            RetrieverJob retrieverJob_No_Target_Filename = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.none
                    },
                    TargetFileName = null
                },
                RelativeUri = "relative_uri_value"
            };

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRelativeUri = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoTargetFileName = new Sentry.Core.ValidationResults();

            //Act
            httpsSource.Validate(retrieverJob, results);
            httpsSource.Validate(retrieverJob_No_Relative_Uri, results_NoRelativeUri);
            httpsSource.Validate(retrieverJob_No_Target_Filename, results_NoTargetFileName);

            //
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoRelativeUri.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.relativeUriNotSpecified, results_NoRelativeUri.GetAll().First().Id);
            Assert.AreEqual(1, results_NoTargetFileName.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsTargetFileNameIsBlank, results_NoTargetFileName.GetAll().First().Id);
        }

        [TestMethod]
        public void GoogleApiDataSourceValidations()
        {
            Mock<GoogleApiSource> googleApiSource = new Mock<GoogleApiSource>() { CallBase = true };
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"

                },
                RelativeUri = "relative_uri_value",
                Schedule = "*****"
            };
            RetrieverJob retrieverJob_No_Request_Method = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the Body",
                        RequestMethod = HttpMethods.none,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value",
                Schedule = "*****"
            };
            RetrieverJob retrieverJob_No_Body_Format = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = "This is the body",
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.none
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_Body = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        Body = null,
                        RequestMethod = HttpMethods.post,
                        RequestDataFormat = HttpDataFormat.json
                    },
                    TargetFileName = "filename"
                },
                RelativeUri = "relative_uri_value"
            };

            googleApiSource.Setup(s => s.ValidateBase(It.IsAny<RetrieverJob>(), It.IsAny<Sentry.Core.ValidationResults>()));

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRequestMethod = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoBodyFormat = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoBody = new Sentry.Core.ValidationResults();

            //Act
            googleApiSource.Object.Validate(retrieverJob, results);
            googleApiSource.Object.Validate(retrieverJob_No_Request_Method, results_NoRequestMethod);
            googleApiSource.Object.Validate(retrieverJob_No_Body_Format, results_NoBodyFormat);
            googleApiSource.Object.Validate(retrieverJob_No_Body, results_NoBody);

            //
            googleApiSource.Verify(v => v.ValidateBase(It.IsAny<RetrieverJob>(), It.IsAny<Sentry.Core.ValidationResults>()), Times.Exactly(4));
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoRequestMethod.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestMethodNotSelected, results_NoRequestMethod.GetAll().First().Id);
            Assert.AreEqual(1, results_NoBodyFormat.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestDataFormatNotSelected, results_NoBodyFormat.GetAll().First().Id);
            Assert.AreEqual(1, results_NoBody.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.httpsRequestBodyIsBlank, results_NoBody.GetAll().First().Id);
        }

        [TestMethod]
        public void FtpSourceValidations()
        {
            Mock<FtpSource> googleApiSource = new Mock<FtpSource>() { CallBase = true };
            RetrieverJob retrieverJob = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {                    
                    FtpPattern = FtpPattern.RegexFileSinceLastExecution
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_FtpPattern = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.None
                },
                RelativeUri = "relative_uri_value"
            };
            RetrieverJob retrieverJob_No_Relative_Uri = new RetrieverJob()
            {
                JobOptions = new RetrieverJobOptions()
                {
                    FtpPattern = FtpPattern.RegexFileSinceLastExecution
                },
                RelativeUri = null
            };

            Sentry.Core.ValidationResults results = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoFtpPattern = new Sentry.Core.ValidationResults();
            Sentry.Core.ValidationResults results_NoRelativeUri = new Sentry.Core.ValidationResults();

            //Act
            googleApiSource.Object.Validate(retrieverJob, results);
            googleApiSource.Object.Validate(retrieverJob_No_FtpPattern, results_NoFtpPattern);
            googleApiSource.Object.Validate(retrieverJob_No_Relative_Uri, results_NoRelativeUri);

            //
            Assert.IsFalse(results.GetAll().Any());
            Assert.AreEqual(1, results_NoFtpPattern.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.ftpPatternNotSelected, results_NoFtpPattern.GetAll().First().Id);
            Assert.AreEqual(1, results_NoRelativeUri.GetAll().Count);
            Assert.AreEqual(DataSource.ValidationErrors.relativeUriNotSpecified, results_NoRelativeUri.GetAll().First().Id);
        }

        [TestMethod]
        public void GoogleBigQueryApiSource_Validate_Success()
        {
            ValidationResults validationResults = new ValidationResults();

            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "RelativeUri",
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        RequestMethod = HttpMethods.get
                    },
                    TargetFileName = "FileName"
                }
            };

            GoogleBigQueryApiSource source = new GoogleBigQueryApiSource();

            source.Validate(job, validationResults);

            Assert.IsTrue(validationResults.IsValid());
        }

        [TestMethod]
        public void GoogleBigQueryApiSource_Validate_Fail()
        {
            ValidationResults validationResults = new ValidationResults();

            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "/RelativeUri",
                JobOptions = new RetrieverJobOptions()
                {
                    HttpOptions = new RetrieverJobOptions.HttpsOptions()
                    {
                        RequestMethod = HttpMethods.none
                    }
                }
            };

            GoogleBigQueryApiSource source = new GoogleBigQueryApiSource();

            source.Validate(job, validationResults);

            Assert.IsFalse(validationResults.IsValid());

            List<ValidationResult> results = validationResults.GetAll();
            Assert.AreEqual(3, results.Count);

            ValidationResult result = results.First();
            Assert.AreEqual(ValidationErrors.httpsTargetFileNameIsBlank, result.Id);
            Assert.AreEqual("Target file name is required for HTTPS data sources", result.Description);

            result = results[1];
            Assert.AreEqual(ValidationErrors.relativeUriStartsWithForwardSlash, result.Id);
            Assert.AreEqual("Relative Uri cannot start with '/' for HTTPS data sources", result.Description);

            result = results.Last();
            Assert.AreEqual(ValidationErrors.httpsRequestMethodNotSelected, result.Id);
            Assert.AreEqual("Request method is required", result.Description);
        }

        [TestMethod]
        public void DfsDataFlowBasic_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                DataFlow = new DataFlow() { FlowStorageCode = "000001" }
            };

            DfsDataFlowBasic dataSource = new DfsDataFlowBasic();

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/DatasetLoader/000001", result.ToString());
        }

        [TestMethod]
        public void DfsBasic_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                DatasetConfig = new DatasetFileConfig()
                {
                    Name = "Config Name",
                    ParentDataset = new Dataset()
                    {
                        DatasetName = "Dataset Name",
                        DatasetCategories = new List<Category>()
                        {
                            new Category() { Name = "Sentry" }
                        }
                    }
                }
            };

            DfsBasic dataSource = new DfsBasic();

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/DatasetLoader/sentry/dataset_name/config_name", result.ToString());
        }

        [TestMethod]
        public void DfsSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/child"
            };

            DfsSource dataSource = new DfsSource();

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/parent/child", result.ToString());
        }

        [TestMethod]
        public void DfsNonProdSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/childNP"
            };

            DfsNonProdSource dataSource = new DfsNonProdSource()
            {
                BaseUri = new Uri("c:/tmp/nonprod/")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/nonprod/parent/childNP", result.ToString());
        }

        [TestMethod]
        public void DfsProdSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/child"
            };

            DfsProdSource dataSource = new DfsProdSource()
            {
                BaseUri = new Uri("c:/tmp/prod/")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/prod/parent/child", result.ToString());
        }

        [TestMethod]
        public void FtpSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/child"
            };

            FtpSource dataSource = new FtpSource()
            {
                BaseUri = new Uri("c:/tmp")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/parent/child", result.ToString());
        }

        [TestMethod]
        public void HTTPSSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/child"
            };

            HTTPSSource dataSource = new HTTPSSource()
            {
                BaseUri = new Uri("https://www.google.com")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("https://www.google.com/parent/child", result.ToString());
        }

        [TestMethod]
        public void JavaAppSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob();

            JavaAppSource dataSource = new JavaAppSource();

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void S3Basic_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                DatasetConfig = new DatasetFileConfig()
                {
                    Schema = new FileSchema()
                    {
                        StorageCode = "000001"
                    }
                }
            };

            S3Basic dataSource = new S3Basic()
            {
                BaseUri = new Uri("http://s3-us-east-2.amazonaws.com/bucket")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("http://s3-us-east-2.amazonaws.com/bucket/000001", result.ToString());
        }

        [TestMethod]
        public void S3Source_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob();
            S3Source dataSource = new S3Source();

            Assert.ThrowsException<NotImplementedException>(() => dataSource.CalcRelativeUri(job));
        }

        [TestMethod]
        public void SFtpSource_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                RelativeUri = "parent/child"
            };

            SFtpSource dataSource = new SFtpSource()
            {
                BaseUri = new Uri("c:/tmp")
            };

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/parent/child", result.ToString());
        }

        [TestMethod]
        public void DfsBasicHsz_CalcRelativeUri()
        {
            RetrieverJob job = new RetrieverJob()
            {
                DatasetConfig = new DatasetFileConfig()
                {
                    Schema = new FileSchema()
                    {
                        StorageCode = "000001"
                    }
                }
            };

            DfsBasicHsz dataSource = new DfsBasicHsz();

            Uri result = dataSource.CalcRelativeUri(job);

            Assert.AreEqual("file:///c:/tmp/DatasetLoader/000001", result.ToString());
        }
    }
}
