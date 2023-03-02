using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class UpdateSchemaRequestValidatorTests
    {
        [TestMethod]
        public void Validate_Success()
        {
            //IRequestModel requestModel = GetBaseSuccessModel();

            //MockRepository mr = new MockRepository(MockBehavior.Strict);

            //Mock<IDatasetContext> datasetContext = GetDatasetContext(mr);
            //Mock<IAssociateInfoProvider> associateInfoProvider = GetAssociateInfoProvider(mr);

            //UpdateSchemaRequestValidator validator = new UpdateSchemaRequestValidator(datasetContext.Object, associateInfoProvider.Object);

            //ConcurrentValidationResponse validationResponse = validator.ValidateAsync(requestModel).Result;

            //Assert.IsTrue(validationResponse.IsValid());

            //mr.VerifyAll();
        }
    }
}
