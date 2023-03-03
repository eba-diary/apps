using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using Sentry.data.Web.API;
using System.Collections.Generic;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ValidationRegistryTests
    {
        private List<IRequestModelValidator> _validators;

        [TestInitialize]
        public void Init()
        {
            Mock<IDatasetContext> context = new Mock<IDatasetContext>();
            Mock<ISAIDService> saidService = new Mock<ISAIDService>();
            Mock<IQuartermasterService> quartermasterService = new Mock<IQuartermasterService>();
            Mock<IAssociateInfoProvider> associateInfoProvider = new Mock<IAssociateInfoProvider>();

            _validators = new List<IRequestModelValidator>
            {
                new AddDatasetRequestValidator(context.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object),
                new UpdateDatasetRequestValidator(context.Object, associateInfoProvider.Object),
                new AddSchemaRequestValidator(context.Object, saidService.Object, quartermasterService.Object, associateInfoProvider.Object),
                new UpdateSchemaRequestValidator(context.Object, associateInfoProvider.Object)
            };
        }

        [TestMethod]
        public void TryGetValidatorFor_AddDatasetRequestValidator()
        {
            ValidationRegistry registry = new ValidationRegistry(_validators);

            Assert.IsTrue(registry.TryGetValidatorFor<AddDatasetRequestModel>(out IRequestModelValidator validator));
            Assert.IsInstanceOfType(validator, typeof(AddDatasetRequestValidator));
        }

        [TestMethod]
        public void TryGetValidatorFor_UpdateDatasetRequestValidator()
        {
            ValidationRegistry registry = new ValidationRegistry(_validators);

            Assert.IsTrue(registry.TryGetValidatorFor<UpdateDatasetRequestModel>(out IRequestModelValidator validator));
            Assert.IsInstanceOfType(validator, typeof(UpdateDatasetRequestValidator));
        }

        [TestMethod]
        public void TryGetValidatorFor_AddSchemaRequestValidator()
        {
            ValidationRegistry registry = new ValidationRegistry(_validators);

            Assert.IsTrue(registry.TryGetValidatorFor<AddSchemaRequestModel>(out IRequestModelValidator validator));
            Assert.IsInstanceOfType(validator, typeof(AddSchemaRequestValidator));
        }

        [TestMethod]
        public void TryGetValidatorFor_UpdateSchemaRequestValidator()
        {
            ValidationRegistry registry = new ValidationRegistry(_validators);

            Assert.IsTrue(registry.TryGetValidatorFor<UpdateSchemaRequestModel>(out IRequestModelValidator validator));
            Assert.IsInstanceOfType(validator, typeof(UpdateSchemaRequestValidator));
        }
    }
}
