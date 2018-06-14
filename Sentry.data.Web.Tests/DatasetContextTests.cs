using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Rhino.Mocks;
using Sentry.Core;

namespace Sentry.data.Tests
{
    [TestClass]
    public class DatasetContextTests
    {

        //[TestInitialize()]
        //public void MyTestInitializeDataAsset()
        //{
        //    QueryableExtensions.ResetQueryableExtensionProviders();
        //    log4net.Config.XmlConfigurator.Configure();

        //    Bootstrapper.Init();

        //    _container = Bootstrapper.Container.GetNestedContainer();

        //    // We aren't mocking the database, but we are mocking other external dependencies
        //    //var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
        //    //_container.Inject(mockExtendedUserInfoProvider);

        //    //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        //    //var domainContext = _container.GetInstance<IDataAssetContext>();
        //    //var session = ((Sentry.data.Infrastructure.dataAssetContext)domainContext).Session;

        //    //var query = session.CreateSQLQuery(
        //    //    "delete from CategorizedAsset " +
        //    //    "delete from Category " +
        //    //    "delete from Asset " +
        //    //    "delete from [User]");

        //    //query.ExecuteUpdate();
        //    //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        //}



        //private IContainer _container;

        //[TestCategory("LiveDatabase")]
        //[TestMethod]
        //public void Can_save_and_retrive_dataset()
        //{
        //    //// Arrange ////
        //    var dataset = InitDataset();
        //    var domainContext = _container.GetInstance<IDatasetContext>();

        //    //// Act ////
        //    domainContext.Add(dataset);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    var datasetid = dataset.DatasetId;
        //    var dataset2 = domainContext.GetById<Dataset>(datasetid);
        //    var dataset2Id = dataset2.DatasetId;

        //    //// Assert ////
        //    Assert.AreEqual(datasetid, dataset2Id);

        //}

        //public Dataset InitDataset()
        //{
        //   Dataset dataset1 = new Dataset(999999,
        //                                 "Some_Category",
        //                                 "Some dataset",
        //                                 "A really cool dataset",
        //                                 "Creator Name",
        //                                 "SentryOwner Name",
        //                                 "UploadUser Name",
        //                                 "O",
        //                                 //"txt", 
        //                                 System.DateTime.Now.AddDays(-3),
        //                                 System.DateTime.Now.AddDays(-2),
        //                                 System.DateTime.Now.AddDays(-1),
        //                                 "Yearly",
        //                                 1000,
        //                                 100,
        //                                 "Some_Category/S3 key",
        //                                 true,
        //                                 null);
        //    return dataset1;
        //}
    }
}
