using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Sentry.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Core;
using Rhino.Mocks;
using System.Linq;

namespace Sentry.data.Web.Tests
{

    // All test methods that go against a live database should be marked with a specific Test Category
    // so they can be excluded from your CI build if needed
    // <TestCategory("LiveDatabase")>

    // In your Live Database Unit Tests, use the following to get to your application context object:
    // Dim domainContext = _container.GetInstance(Of IDataAssetContext)()

    [TestClass()]
    public class LiveDatabaseUnitTests
    {
        //private IContainer _container;

        //#region "Additional test attributes"
        ////
        //// You can use the following additional attributes as you write your tests:
        ////
        //// Use ClassInitialize to run code before running the first test in the class
        //// <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
        //// End Sub
        ////
        //// Use ClassCleanup to run code after all tests in a class have run
        //// <ClassCleanup()> Public Shared Sub MyClassCleanup()
        //// End Sub
        ////


        //// Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitializeDataAsset()
        //{
        //    QueryableExtensions.ResetQueryableExtensionProviders();
        //    log4net.Config.XmlConfigurator.Configure();

        //    Bootstrapper.Init();

        //    _container = Bootstrapper.Container.GetNestedContainer();

        //    // We aren't mocking the database, but we are mocking other external dependencies
        //    var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();

        //    _container.Inject(mockExtendedUserInfoProvider);

        //    //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        //    var domainContext = _container.GetInstance<IDataAssetContext>();
        //    var session = ((Sentry.data.Infrastructure.dataAssetContext)domainContext).Session;

        //    var query = session.CreateSQLQuery(
        //        "delete from CategorizedAsset " + 
        //        "delete from Category " + 
        //        "delete from Asset " + 
        //        "delete from [User]");

        //    query.ExecuteUpdate();
        //    //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        //}

        //// Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanupDataAsset()
        //{
        //    //This call will close the NHibernate session without committing the transaction, so the
        //    //database returns to the pre-test state
        //    var domainContext = _container.GetInstance<IDataAssetContext>();
        //    domainContext.Dispose();

        //}
        ////
        //#endregion

        ////###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_save_and_retrieve_user()
        //{
        //    // //// Arrange ////
        //    DomainUser user = new DomainUser("123456");
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    // //// Act //// 
        //    domainContext.Add(user);
        //    domainContext.SaveChanges(false);
        //    //uow.Clear()

        //    var userId = user.Id;
        //    var user2 = domainContext.GetById<DomainUser>(userId);

        //    // //// Assert ////
        //    Assert.IsTrue(userId > 0);
        //    Assert.AreEqual("123456", user2.AssociateId);
        //    Assert.AreSame(user, user2);
        //    //Assert.AreNotSame(user, user2)

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_save_and_retrieve_asset()
        //{
        //    // //// Arrange ////
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    Asset asset = new Asset("testName", "testDesc");

        //    // //// Act ////
        //    domainContext.Add(asset);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    var assetId = asset.Id;
        //    var asset2 = domainContext.GetById<Asset>(assetId);

        //    // //// Assert ////
        //    Assert.IsTrue(assetId > 0);

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_update_detached_user()
        //{
        //    // //// Arrange ////
        //    DomainUser user = new DomainUser("123456");
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(user);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // //// Act ////
        //    var user2 = domainContext.GetById<DomainUser>(user.Id);
        //    domainContext.Clear();

        //    //Dim user4 = userRepository.GetById(user.Id)

        //    user2.Ranking = 555;

        //    user2 = domainContext.Merge(user2);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();


        //    // //// Assert ////
        //    var user3 = domainContext.GetById<DomainUser>(user.Id);
        //    Assert.AreEqual(555, user3.Ranking);

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod(), ExpectedException(typeof(global::NHibernate.StaleObjectStateException))]
        //public void Concurrent_modifications_demo()
        //{
        //    // //// Arrange ////
        //    DomainUser user = new DomainUser("123456");
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(user);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // //// Act ////
        //    // Two different people (A and B) load up George in independent units of work 
        //    // (so they have seperate copies of the object):
        //    var userA = domainContext.GetById<DomainUser>(user.Id);
        //    domainContext.Clear();

        //    var userB = domainContext.GetById<DomainUser>(user.Id);
        //    domainContext.Clear();

        //    // Next, person A makes a change to George, attaches it, and flushes it
        //    userA.Ranking = 111;
        //    domainContext.AttachAsModified(userA);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // Then, person B makes a different change to their copy of George, attaches it, and flushes it
        //    userB.Ranking = 222;
        //    domainContext.AttachAsModified(userB);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // Now, what's in the database?  Which person's changes (A, B, or both) are now in the database?
        //    var userFinal = domainContext.GetById<DomainUser>(user.Id);

        //    // //// Assert ////
        //    Assert.AreEqual(111, userFinal.Ranking);
        //    Assert.AreEqual(222, userFinal.Ranking);
        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_save_and_retrieve_category_hierarchy()
        //{
        //    // //// Arrange ////
        //    Category cat1 = new Category("SQLServer");
        //    Category cat2 = new Category("Teradata", cat1);
        //    Category cat3 = new Category("MongoDB", cat1);
        //    Category cat4 = new Category("S3", cat2);
        //    Category cat5 = new Category("XML", cat2);

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    // //// Act ////

        //    domainContext.Add(cat1);

        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    var rootCateogry = domainContext.GetById<Category>(cat1.Id);

        //    // //// Assert ////
        //    Assert.AreEqual("SQLServer", rootCateogry.Name);
        //    Assert.AreEqual(2, rootCateogry.SubCategories.Count());
        //    Assert.AreEqual(2, rootCateogry.SubCategories.ElementAt(0).SubCategories.Count());
        //    Assert.AreEqual(0, rootCateogry.SubCategories.ElementAt(1).SubCategories.Count());

        //    Assert.AreEqual(rootCateogry, rootCateogry.SubCategories.ElementAt(0).ParentCategory);
        //    Assert.AreEqual(rootCateogry, rootCateogry.SubCategories.ElementAt(1).ParentCategory);
        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]

        //public void Can_delete_part_of_category_hierarchy()
        //{
        //    // //// Arrange ////
        //    Category cat1 = new Category("SQLServer");
        //    Category cat2 = new Category("Teradata", cat1);
        //    Category cat3 = new Category("MongoDB", cat1);
        //    Category cat4 = new Category("S3", cat2);
        //    Category cat5 = new Category("XML", cat2);

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(cat1);
        //    domainContext.SaveChanges(false);

        //    // //// Act ////
        //    cat2.ParentCategory = null;
        //    domainContext.Remove(cat2);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();


        //    var rootCateogry = domainContext.GetById<Category>(cat1.Id);

        //    // //// Assert ////
        //    Assert.AreEqual("SQLServer", rootCateogry.Name);
        //    Assert.AreEqual(1, rootCateogry.SubCategories.Count());

        //    Assert.AreEqual(rootCateogry, rootCateogry.SubCategories.ElementAt(0).ParentCategory);

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]

        //public void Can_save_and_retrieve_asset_with_categories()
        //{

        //    // //// Arrange ////
        //    DomainUser seller = new DomainUser("123456");
        //    Category databaseCategory = new Category("Databases");
        //    Category newAssetsCategory = new Category("NewAssets");

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(seller);
        //    domainContext.Add(databaseCategory);
        //    domainContext.Add(newAssetsCategory);

        //    // //// Act ////
        //    Asset asset = new Asset("New SQL Repo", "Crazy new analytical DB");

        //    asset.AddCategory(databaseCategory);
        //    asset.AddCategory(newAssetsCategory);

        //    domainContext.Add(asset);

        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();
        //    var asset2 = domainContext.GetById<Asset>(asset.Id);

        //    // //// Assert ////
        //    Assert.AreEqual(2, asset2.Categories.Count());
        //    Assert.AreEqual("Databases", asset2.Categories.ElementAt(0).Name);
        //    Assert.AreEqual("NewAssets", asset2.Categories.ElementAt(1).Name);

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_get_categories_by_linq_queries()
        //{
        //    // //// Arrange ////
        //    Category cat1 = new Category("Trees");
        //    Category cat2 = new Category("Deciduous", cat1);
        //    Category cat3 = new Category("Evergreen", cat1);
        //    Category cat4 = new Category("Structures");
        //    Category cat5 = new Category("Houses", cat4);
        //    Category cat6 = new Category("Treehouses", cat4);
        //    Category cat7 = new Category("Green buildings", cat4);

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(cat1);
        //    domainContext.Add(cat4);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // //// Act ////
        //    var allCategories = domainContext.Categories.ToList();
        //    var rootCategories = domainContext.Categories.WhereIsRoot().ToList();
        //    var categoriesWithHouse = domainContext.Categories.Where(c => c.Name.Contains("house"));
        //    var categoriesWithEe = domainContext.Categories.Where(c => c.Name.Contains("ee"));

        //    // //// Assert ////
        //    Assert.AreEqual(7, allCategories.Count());
        //    Assert.AreEqual(2, rootCategories.Count());
        //    Assert.AreEqual(2, categoriesWithHouse.Count());
        //    Assert.AreEqual(4, categoriesWithEe.Count());

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_get_assets_by_various_ways()
        //{
        //    // //// Arrange ////        
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    Category catCloud = new Category("Cloud");
        //    Category catDB = new Category("DB", catCloud);
        //    Category catFiles = new Category("Flat Files", catCloud);
        //    Category catOnPrem = new Category("OnPrem");
        //    domainContext.Add(catCloud);
        //    domainContext.Add(catOnPrem);

        //    DomainUser user1 = new DomainUser("123456");
        //    domainContext.Add(user1);
        //    DomainUser user2 = new DomainUser("345678");
        //    domainContext.Add(user2);
        //    DomainUser admin = new DomainUser("999999");
        //    domainContext.Add(admin);

        //    var mockAdminUser = MockRepository.GenerateStub<IApplicationUser>();
        //    mockAdminUser.Stub(x => x.DomainUser).Return(admin);

        //    Asset asset1 = new Asset("test Asset 1", "test Desc 1");
        //    asset1.AddCategory(catCloud);
        //    asset1.AddCategory(catDB);
        //    domainContext.Add(asset1);

        //    Asset asset2 = new Asset("test Asset 2", "test Desc 2");
        //    asset2.AddCategory(catCloud);
        //    asset2.AddCategory(catFiles);
        //    domainContext.Add(asset2);

        //    Asset asset3 = new Asset("test Asset 3", "test Desc 3");
        //    asset3.AddCategory(catOnPrem);
        //    domainContext.Add(asset3);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // //// Act ////
        //    var upAssets = domainContext.Assets.WhereUp().ToList();
        //    var downAssets = domainContext.Assets.WhereDown().ToList();
        //    var waitingAssets = domainContext.Assets.WhereWaiting().ToList();
        //    var assetsUnknownState = domainContext.Assets.WhereUnknown().ToList();
        //    var inCloud = domainContext.Assets.InCategory(catCloud).ToList();

        //    // //// Assert ////
        //    Assert.AreEqual(1, upAssets.Count());
        //    Assert.AreEqual(1, downAssets.Count());
        //    Assert.AreEqual(1, waitingAssets.Count());
        //    Assert.AreEqual(1, assetsUnknownState.Count());
        //    Assert.AreEqual(2, inCloud.Count());
        //    Assert.IsTrue(inCloud.ElementAt(0).Categories.Any(cat => cat.Name == "test Asset 1"));

        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Can_get_assets_using_FetchMany_and_ToFuture()
        //{
        //    // //// Arrange ////        
        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    Category catAppliances = new Category("Appliances");
        //    Category catBlasters = new Category("Blasters", catAppliances);
        //    Category catAtomizers = new Category("Atomizers", catAppliances);
        //    Category catFuzzyStuff = new Category("Fuzzy stuff");
        //    domainContext.Add(catAppliances);
        //    domainContext.Add(catFuzzyStuff);

        //    DomainUser user1 = new DomainUser("123456");
        //    domainContext.Add(user1);
        //    DomainUser user2 = new DomainUser("345678");
        //    domainContext.Add(user2);
        //    DomainUser admin = new DomainUser("999999");
        //    domainContext.Add(admin);

        //    var mockAdminUser = MockRepository.GenerateStub<IApplicationUser>();
        //    mockAdminUser.Stub(x => x.DomainUser).Return(admin);

        //    Asset asset1 = new Asset("Fuzz Blaster 3000", "Blasts fuzz like never before");
        //    asset1.AddCategory(catBlasters);
        //    asset1.AddCategory(catFuzzyStuff);
        //    domainContext.Add(asset1);

        //    Asset asset2 = new Asset("Fuzz Atomizer XL", "Atomizes fuzz without destroying it");
        //    asset2.AddCategory(catAtomizers);
        //    asset2.AddCategory(catFuzzyStuff);
        //    domainContext.Add(asset2);

        //    Asset asset3 = new Asset("Generic Atomizer", "Atomizes anything you point it at");
        //    asset3.AddCategory(catAtomizers);
        //    domainContext.Add(asset3);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    // //// Act ////
        //    var assets = domainContext.Assets.Where(i => i.Name.Contains("a")).FetchMany(i => i.Categories).ToFuture();

        //    domainContext.Assets.Where(i => i.Name.Contains("a")).FetchMany(i => i.Categories).ToFuture();

        //    var assetsList = assets.ToList();


        //    Assert.AreEqual(3, assetsList.Count());
        //    Assert.AreEqual(2, assetsList.ElementAt(1).Categories.Count());

        //    // //// Assert ////
        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Batch_data_update_with_HQL_demo()
        //{
        //    // //// Arrange ////
        //    Category cat1 = new Category("Trees");
        //    Category cat2 = new Category("Deciduous", cat1);
        //    Category cat3 = new Category("Evergreen", cat1);
        //    Category cat4 = new Category("Structures");
        //    Category cat5 = new Category("Houses", cat4);
        //    Category cat6 = new Category("Treehouses", cat4);
        //    Category cat7 = new Category("Green buildings", cat4);

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(cat1);
        //    domainContext.Add(cat4);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    var session = ((Sentry.data.Infrastructure.dataAssetContext)domainContext).Session;

        //    var query = session.CreateQuery("update Category c set c.Name=c.Name+' (modified)'");
        //    query.ExecuteUpdate();

        //    var allCategories = domainContext.Categories.ToList();

        //    foreach (var category in allCategories)
        //    {
        //        Assert.IsTrue(category.Name.EndsWith("(modified)"));
        //    }
        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void Batch_data_update_with_persistent_objects_demo()
        //{
        //    // //// Arrange ////
        //    Category cat1 = new Category("Trees");
        //    Category cat2 = new Category("Deciduous", cat1);
        //    Category cat3 = new Category("Evergreen", cat1);
        //    Category cat4 = new Category("Structures");
        //    Category cat5 = new Category("Houses", cat4);
        //    Category cat6 = new Category("Treehouses", cat4);
        //    Category cat7 = new Category("Green buildings", cat4);

        //    var domainContext = _container.GetInstance<IDataAssetContext>();

        //    domainContext.Add(cat1);
        //    domainContext.Add(cat4);
        //    domainContext.SaveChanges(false);
        //    domainContext.Clear();

        //    var session = ((Sentry.data.Infrastructure.dataAssetContext)domainContext).Session;

        //    int currentPage = 0;
        //    int pageSize = 3;

        //    IEnumerable<Category> categories = default(IEnumerable<Category>);
        //    do
        //    {
        //        categories = domainContext.Categories.Skip(currentPage * pageSize).Take(pageSize).ToList();

        //        foreach (var category in categories)
        //        {
        //            category.Name = category.Name + " (modified)";
        //        }
        //        domainContext.SaveChanges(false);
        //        domainContext.Clear();
        //        // <--- Very important, to keep the in-memory persistence context small
        //        currentPage += 1;
        //    } while (categories.Any());

        //    var allCategories = domainContext.Categories.ToList();
        //    foreach (var category in allCategories)
        //    {
        //        Assert.IsTrue(category.Name.EndsWith("(modified)"));
        //    }
        //}

        //[TestCategory("LiveDatabase")]
        //[TestMethod()]
        //public void NestedContainer_Demonstration()
        //{
        //    StructureMap.Registry registry = new StructureMap.Registry();

        //    registry.For<IEmailService>().Use<EmailService>();
        //    registry.For<IAssociateInfoProvider>().Singleton().Use<AssociateInfoProvider>();


        //    var rootContainer = new Container(registry);
        //    var nestedContanier = rootContainer.GetNestedContainer();

        //    //Transient in the root container, so each instance we get is unique
        //    var rootEmailService1 = rootContainer.GetInstance<IEmailService>();
        //    var rootEmailService2 = rootContainer.GetInstance<IEmailService>();
        //    Assert.AreNotEqual(rootEmailService1, rootEmailService2);

        //    //Transient in the nested container, so we get one instance per nested container
        //    var nestedEmailService1 = nestedContanier.GetInstance<IEmailService>();
        //    var nestedEmailService2 = nestedContanier.GetInstance<IEmailService>();
        //    Assert.AreEqual(nestedEmailService1, nestedEmailService2);

        //    //Singleton in the root container, so we get the same instance every time
        //    var rootAssocInfoProv1 = rootContainer.GetInstance<IAssociateInfoProvider>();
        //    var rootAssocInfoProv2 = rootContainer.GetInstance<IAssociateInfoProvider>();
        //    Assert.AreEqual(rootAssocInfoProv1, rootAssocInfoProv2);

        //    //Singleton in the nested container, so we get the same instance as the root container
        //    var nestedAssocInfoProv1 = nestedContanier.GetInstance<IAssociateInfoProvider>();
        //    var nestedAssocInfoProv2 = nestedContanier.GetInstance<IAssociateInfoProvider>();
        //    Assert.AreEqual(nestedAssocInfoProv1, rootAssocInfoProv1);
        //    Assert.AreEqual(nestedAssocInfoProv1, nestedAssocInfoProv2);

        //    //Instances of Transient-scoped types are not shared between nested containers
        //    var nestedContanier2 = rootContainer.GetNestedContainer();
        //    var nestedEmailService3 = nestedContanier2.GetInstance<IEmailService>();
        //    Assert.AreNotEqual(nestedEmailService1, nestedEmailService3);
        //}

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
