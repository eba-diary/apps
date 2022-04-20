﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class UserFavoriteServiceTests
    {
        [TestMethod]
        public void GetUserFavoriteItems_000000_FavoriteItems()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.UserFavorites).Returns(GetUserFavorites());
            datasetContext.SetupGet(x => x.Favorites).Returns(new List<Favorite>().AsQueryable());
            datasetContext.SetupGet(x => x.SavedSearches).Returns(GetSavedSearches());

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            IList<FavoriteItem> favoriteItems = userFavoriteService.GetUserFavoriteItems("000000");

            datasetContext.VerifyAll();

            Assert.AreEqual(1, favoriteItems.Count);

            FavoriteItem favoriteItem = favoriteItems.First();
            Assert.AreEqual(1, favoriteItem.Id);
            Assert.AreEqual(1, favoriteItem.Sequence);
            Assert.AreEqual("SearchName", favoriteItem.Title);
            Assert.AreEqual("DataInventory/Search?savedSearch=SearchName", favoriteItem.Url);
            Assert.AreEqual("/Images/DataInventory/DataInventoryIcon.svg", favoriteItem.Img);
            Assert.AreEqual("WEB", favoriteItem.FeedUrlType);
            Assert.IsFalse(favoriteItem.IsLegacyFavorite);
        }

        [TestMethod]
        public void GetUserFavoriteItems_000000_LegacyFavoriteItems()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.UserFavorites).Returns(new List<UserFavorite>().AsQueryable());
            datasetContext.SetupGet(x => x.Favorites).Returns(GetLegacyFavorites());
            datasetContext.SetupGet(x => x.Datasets).Returns(GetDatasets());

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            IList<FavoriteItem> favoriteItems = userFavoriteService.GetUserFavoriteItems("000000");

            datasetContext.VerifyAll();

            Assert.AreEqual(1, favoriteItems.Count);

            FavoriteItem favoriteItem = favoriteItems.First();
            Assert.AreEqual(1, favoriteItem.Id);
            Assert.AreEqual(1, favoriteItem.Sequence);
            Assert.AreEqual("DatasetName", favoriteItem.Title);
            Assert.AreEqual("/Dataset/Detail/2", favoriteItem.Url);
            Assert.AreEqual("/Images/Icons/Datasets.svg", favoriteItem.Img);
            Assert.AreEqual(GlobalConstants.DataEntityCodes.DATASET, favoriteItem.FeedUrlType);
            Assert.AreEqual(2, favoriteItem.FeedId);
            Assert.AreEqual(GlobalConstants.DataFeedName.DATASET, favoriteItem.FeedName);
            Assert.AreEqual("/Datasets/Detail/2", favoriteItem.FeedUrl);
            Assert.IsTrue(favoriteItem.IsLegacyFavorite);
        }

        [TestMethod]
        public void RemoveUserFavorite_1_False_Deleted()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.UserFavorites).Returns(GetUserFavorites());
            datasetContext.Setup(x => x.Remove(It.IsAny<UserFavorite>())).Callback<UserFavorite>(x =>
            {
                Assert.AreEqual(1, x.UserFavoriteId);
                Assert.AreEqual("000000", x.AssociateId);
                Assert.AreEqual(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, x.FavoriteType);
                Assert.AreEqual(2, x.FavoriteEntityId);
                Assert.AreEqual(1, x.Sequence);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            userFavoriteService.RemoveUserFavorite(1, false);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void RemoveUserFavorite_1_True_Deleted()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.Favorites).Returns(GetLegacyFavorites());
            datasetContext.Setup(x => x.Remove(It.IsAny<Favorite>())).Callback<Favorite>(x =>
            {
                Assert.AreEqual(1, x.FavoriteId);
                Assert.AreEqual("000000", x.UserId);
                Assert.AreEqual(2, x.DatasetId);
                Assert.AreEqual(1, x.Sequence);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            userFavoriteService.RemoveUserFavorite(1, true);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void SetUserFavoritesOrder_OrderedIds_FavoriteItems()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.UserFavorites).Returns(GetUserFavorites());
            datasetContext.SetupGet(x => x.Favorites).Returns(GetLegacyFavorites());
            datasetContext.Setup(x => x.SaveChanges(true));
            datasetContext.SetupGet(x => x.SavedSearches).Returns(GetSavedSearches());
            datasetContext.SetupGet(x => x.Datasets).Returns(GetDatasets());

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            List<KeyValuePair<int, bool>> kvps = new List<KeyValuePair<int, bool>>()
            {
                new KeyValuePair<int, bool>(1, false),
                new KeyValuePair<int, bool>(1, true)
            };

            IList<FavoriteItem> favoriteItems = userFavoriteService.SetUserFavoritesOrder(kvps);

            datasetContext.VerifyAll();

            Assert.AreEqual(2, favoriteItems.Count);

            FavoriteItem favoriteItem = favoriteItems.First();
            Assert.AreEqual(1, favoriteItem.Id);
            Assert.AreEqual(0, favoriteItem.Sequence);
            Assert.AreEqual("SearchName", favoriteItem.Title);
            Assert.AreEqual("DataInventory/Search?savedSearch=SearchName", favoriteItem.Url);
            Assert.AreEqual("/Images/DataInventory/DataInventoryIcon.svg", favoriteItem.Img);
            Assert.AreEqual("WEB", favoriteItem.FeedUrlType);
            Assert.IsFalse(favoriteItem.IsLegacyFavorite);

            favoriteItem = favoriteItems.Last();
            Assert.AreEqual(1, favoriteItem.Id);
            Assert.AreEqual(1, favoriteItem.Sequence);
            Assert.AreEqual("DatasetName", favoriteItem.Title);
            Assert.AreEqual("/Dataset/Detail/2", favoriteItem.Url);
            Assert.AreEqual("/Images/Icons/Datasets.svg", favoriteItem.Img);
            Assert.AreEqual(GlobalConstants.DataEntityCodes.DATASET, favoriteItem.FeedUrlType);
            Assert.AreEqual(2, favoriteItem.FeedId);
            Assert.AreEqual(GlobalConstants.DataFeedName.DATASET, favoriteItem.FeedName);
            Assert.AreEqual("/Datasets/Detail/2", favoriteItem.FeedUrl);
            Assert.IsTrue(favoriteItem.IsLegacyFavorite);
        }

        [TestMethod]
        public void AddUserFavorite_Favorite_000000_Add()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            datasetContext.SetupGet(x => x.UserFavorites).Returns(new List<UserFavorite>().AsQueryable());
            datasetContext.Setup(x => x.Add(It.IsAny<UserFavorite>())).Callback<UserFavorite>(x =>
            {
                Assert.AreEqual("000000", x.AssociateId);
                Assert.AreEqual(GlobalConstants.UserFavoriteTypes.SAVEDSEARCH, x.FavoriteType);
                Assert.AreEqual(2, x.FavoriteEntityId);
                Assert.AreEqual(0, x.Sequence);
            });
            datasetContext.Setup(x => x.SaveChanges(true));

            UserFavoriteService userFavoriteService = new UserFavoriteService(datasetContext.Object);

            userFavoriteService.AddUserFavorite(GetSavedSearches().First(), "000000");

            datasetContext.VerifyAll();
        }

        #region Helpers
        private IQueryable<UserFavorite> GetUserFavorites()
        {
            return new List<UserFavorite>()
            {
                new UserFavorite()
                {
                    UserFavoriteId = 1,
                    AssociateId = "000000",
                    FavoriteType = GlobalConstants.UserFavoriteTypes.SAVEDSEARCH,
                    FavoriteEntityId = 2,
                    Sequence = 1
                }
            }.AsQueryable();
        }

        private IQueryable<Favorite> GetLegacyFavorites()
        {
            return new List<Favorite>()
            {
                new Favorite()
                {
                    FavoriteId = 1,
                    DatasetId = 2,
                    UserId = "000000",
                    Sequence = 1
                }
            }.AsQueryable();
        }

        private IQueryable<SavedSearch> GetSavedSearches()
        {
            return new List<SavedSearch>()
            {
                new SavedSearch()
                {
                    SavedSearchId = 2,
                    SearchType = GlobalConstants.SearchType.DATA_INVENTORY,
                    SearchName = "SearchName"
                }
            }.AsQueryable();
        }

        private IQueryable<Dataset> GetDatasets()
        {
            return new List<Dataset>()
            {
                new Dataset()
                {
                    DatasetId = 2,
                    ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active,
                    DatasetName = "DatasetName",
                    DatasetType = GlobalConstants.DataEntityCodes.DATASET
                }
            }.AsQueryable();
        }
        #endregion
    }
}
