using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class PagedListTests
    {
        [TestMethod]
        public void TotalCount_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.TotalCount);
        }

        [TestMethod]
        public void PageSize_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(10, list.PageSize);
        }

        [TestMethod]
        public void CurrentPage_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1, list.CurrentPage);
        }

        [TestMethod]
        public void TotalPages_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(100, list.TotalPages);
        }

        [TestMethod]
        public void Items_Added_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.Count);
        }

        [TestMethod]
        public void ToPagedList_CurrentPage_Initialized()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(1, list.CurrentPage);
        }

        [TestMethod]
        public void ToPagedList_PageSize_Initialized()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(10, list.PageSize);
        }

        [TestMethod]
        public void ToPagedList_Total_Count()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.TotalCount);
        }

        [TestMethod]
        public void ToPagedList_HasPrevious_and_HasNext_on_First_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 1);

            // Assert
            Assert.AreEqual(false, list.HasPrevious);
            Assert.AreEqual(true, list.HasNext);
        }

        [TestMethod]
        public void ToPagedList_HasPrevious_and_HasNext_on_Middle_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 2, 1);

            // Assert
            Assert.AreEqual(true, list.HasPrevious);
            Assert.AreEqual(true, list.HasNext);
        }

        [TestMethod]
        public void ToPagedList_HasPrevious_and_HasNext_on_Last_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 3, 1);

            // Assert
            Assert.AreEqual(true, list.HasPrevious);
            Assert.AreEqual(false, list.HasNext);
        }

        [TestMethod]
        public void ToPagedList_HasPrevious_and_HasNext_on_Only_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 1);

            // Assert
            Assert.AreEqual(false, list.HasPrevious);
            Assert.AreEqual(false, list.HasNext);
        }

        [TestMethod]
        public void ToPagedList_Page_Greater_Than_Exists()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 2, 1);

            // Assert
            Assert.AreEqual(true, list.HasPrevious);
            Assert.AreEqual(false, list.HasNext);
            Assert.AreEqual(0, list.Count);
        }
    }
}
