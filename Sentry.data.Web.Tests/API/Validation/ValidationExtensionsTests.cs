using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class ValidationExtensionsTests
    {
        [TestMethod]
        public void Validate_FromValidationResponseModel_FluentValidationResponse()
        {
            //ValidationResponseModel model = new ValidationResponseModel();

            //AddDatasetRequestModel propertyModel = new AddDatasetRequestModel();

            //List<AddDatasetRequestModel> models = new List<AddDatasetRequestModel>();
            //models.Select
            //FluentValidationResponse fluent = model.Validate(() => propertyModel.)

            AddDatasetRequestModel model = new AddDatasetRequestModel();

            model.Validate(x => x.DatasetName)
                    .Required()
                    .MaxLength(1024)
                 .Validate(x => x.DataClassificationTypeCode)
                    .EnumValue(typeof(DataClassificationType))
                 .ToValidationResponse();
        }
    }
}
