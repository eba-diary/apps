using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.Core;
using Sentry.data.Core;

namespace Sentry.data.Web.Tests
{

    [TestClass()]
    public class DomainModelUnitTests
    {

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        //[TestMethod(), ExpectedException(typeof(ValidationException))]
        //public void Can_prevent_asset_with_no_name()
        //{
        //    // //// Arrange ////
        //    Asset asset1 = new Asset("", "description");
        //    var vr = asset1.ValidateForSave();
        //    Assert.IsTrue(vr.Contains(Asset.ValidationErrors.nameIsBlank));
        //}

        //[TestMethod(), ExpectedException(typeof(ValidationException))]
        //public void Can_prevent_asset_with_no_description()
        //{
        //    // //// Arrange ////
        //    Asset asset1 = new Asset("name", "");
        //    var vr = asset1.ValidateForDelete();
        //    Assert.IsTrue(vr.Contains(Asset.ValidationErrors.descriptionIsBlank));
        //}

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }


}
