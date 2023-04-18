using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.API;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class IndexGlobalDatasetsValidatorTests
    {
        [TestMethod]
        public async Task ValidateAsync_IndexAll_Success()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                IndexAll = true
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsTrue(validationResponse.IsValid());
        }

        [TestMethod]
        public async Task ValidateAsync_GlobalDatasetIds_Success()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                GlobalDatasetIds = new List<int> { 1 }
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsTrue(validationResponse.IsValid());
        }

        [TestMethod]
        public async Task ValidateAsync_GlobalDatasetIdsWithIndexAll_Fail()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                IndexAll = true,
                GlobalDatasetIds = new List<int> { 1 }
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(IndexGlobalDatasetsRequestModel.GlobalDatasetIds), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Value(s) not accepted when {nameof(IndexGlobalDatasetsRequestModel.IndexAll)} set to true", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public async Task ValidateAsync_GlobalDatasetIds_Empty_Fail()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                GlobalDatasetIds = new List<int>()
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(IndexGlobalDatasetsRequestModel.GlobalDatasetIds), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public async Task ValidateAsync_GlobalDatasetIds_Null_Fail()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                GlobalDatasetIds = null
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(IndexGlobalDatasetsRequestModel.GlobalDatasetIds), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Required field", fieldValidation.ValidationMessages.First());
        }

        [TestMethod]
        public async Task ValidateAsync_GlobalDatasetIds_MaxLength_Fail()
        {
            IRequestModel model = new IndexGlobalDatasetsRequestModel
            {
                GlobalDatasetIds = Enumerable.Range(1, 21).ToList()
            };

            IndexGlobalDatasetsRequestValidator validator = new IndexGlobalDatasetsRequestValidator();

            ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(model);

            Assert.IsFalse(validationResponse.IsValid());

            ConcurrentQueue<ConcurrentFieldValidationResponse> fieldValidations = validationResponse.FieldValidations;
            Assert.AreEqual(1, fieldValidations.Count);

            ConcurrentFieldValidationResponse fieldValidation = fieldValidations.FirstOrDefault();
            Assert.AreEqual(nameof(IndexGlobalDatasetsRequestModel.GlobalDatasetIds), fieldValidation.Field);
            Assert.AreEqual(1, fieldValidation.ValidationMessages.Count);
            Assert.AreEqual($"Max length of 20 values", fieldValidation.ValidationMessages.First());
        }
    }
}
