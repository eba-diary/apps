using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.Core;
using static Sentry.Common.SystemClock;
using System.Linq;

namespace Sentry.data.Tests
{
    [TestClass()]
    public class MockBasedUnitTests
    {

        //// Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //    QueryableExtensions.ResetQueryableExtensionProviders();

        //}


        ////###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        //[TestMethod()]
        //public void Can_do_domain_service_stuff()
        //{
        //    // //// Arrange ////
        //    var mockEmailService = MockRepository.GenerateMock<IEmailService>();
        //    var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
        //    var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();

        //    System.DateTime startDate = Now().AddDays(-1);
        //    System.DateTime endDate = startDate.AddDays(2);
        //    DomainUser domainUser1 = new DomainUser("123456");
        //    DomainUser domainUser2 = new DomainUser("999999");
        //    DomainUser domainUser3 = new DomainUser("345678");

        //    var user1 = MockRepository.GenerateStub<IApplicationUser>();
        //    var user2 = MockRepository.GenerateStub<IApplicationUser>();
        //    var user3 = MockRepository.GenerateStub<IApplicationUser>();

        //    user3.Stub(x => x.EmailAddress).Return("user1@a.com");
        //    user1.Stub(x => x.EmailAddress).Return("user2@b.com");

        //    user2.Stub(x => x.CanApproveItems).Return(true);

        //    var userService = MockRepository.GenerateStub<UserService>(null, null, null);
        //    userService.Stub(x => x.GetByDomainUser(domainUser1)).Return(user1);
        //    userService.Stub(x => x.GetByDomainUser(domainUser2)).Return(user2);
        //    userService.Stub(x => x.GetByDomainUser(domainUser3)).Return(user3);

        //    Asset asset = new Asset("Some Asset", "A fast asset");
        //    List<Asset> assetsInList = new List<Asset>();
        //    assetsInList.Add(asset);
        //    mockDataAssetContext.Stub(x => x.Assets).Return(assetsInList.AsQueryable());

        //    Dataset dataset = new Dataset(999999, 
        //                                  "Some Category", 
        //                                  "Some dataset", 
        //                                  "A really cool dataset", 
        //                                  "Creator Name", 
        //                                  "SentryOwner Name", 
        //                                  "UploadUser Name", 
        //                                  "O", 
        //                                  //"txt", 
        //                                  System.DateTime.Now.AddDays(-3), 
        //                                  System.DateTime.Now.AddDays(-2), 
        //                                  System.DateTime.Now.AddDays(-1),
        //                                  "Yearly", 
        //                                  1000, 
        //                                  100, 
        //                                  "S3 key",
        //                                  true, 
        //                                  null);
        //    List<Dataset> datasetsInList = new List<Dataset>();
        //    datasetsInList.Add(dataset);
        //    mockDatasetContext.Stub(x => x.Datasets).Return(datasetsInList.AsQueryable());

        //    // //// Act ////
        //    ////Note the use of manual constructor injection here:
        //    //AssetDomainService assetTester = new AssetDomainService(mockDataAssetContect, mockEmailService, userService);

        //    //assetTester.DoSomethingOrAnother();

        //}
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific

    }

}
