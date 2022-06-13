using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Helpers.Paginate;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class PaginateTests
    {
        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_TotalCount_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.TotalCount);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_PageSize_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(10, list.PageSize);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_CurrentPage_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1, list.CurrentPage);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_TotalPages_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(100, list.TotalPages);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_Items_Added_Initialized()
        {
            // Arrange
            List<int> intArray = Enumerable.Range(0, 1000).ToList();

            // Act
            PagedList<int> list = new PagedList<int>(intArray, intArray.Count, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.Count);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_CurrentPage_Initialized()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(1, list.CurrentPage);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_PageSize_Initialized()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(10, list.PageSize);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_Total_Count()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1000).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 10);

            // Assert
            Assert.AreEqual(1000, list.TotalCount);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_HasPrevious_and_HasNext_on_First_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 1);

            // Assert
            Assert.AreEqual(false, list.HasPrevious);
            Assert.AreEqual(true, list.HasNext);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_HasPrevious_and_HasNext_on_Middle_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 2, 1);

            // Assert
            Assert.AreEqual(true, list.HasPrevious);
            Assert.AreEqual(true, list.HasNext);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_HasPrevious_and_HasNext_on_Last_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 3).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 3, 1);

            // Assert
            Assert.AreEqual(true, list.HasPrevious);
            Assert.AreEqual(false, list.HasNext);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_HasPrevious_and_HasNext_on_Only_Page()
        {
            // Arrange
            IQueryable<int> intQueryArray = Enumerable.Range(0, 1).AsQueryable();

            // Act
            PagedList<int> list = PagedList<int>.ToPagedList(intQueryArray, 1, 1);

            // Assert
            Assert.AreEqual(false, list.HasPrevious);
            Assert.AreEqual(false, list.HasNext);
        }

        [TestCategory("PagedList")]
        [TestMethod]
        public void PagedList_ToPagedList_Page_Greater_Than_Exists()
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

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageNumber_Non_Null_Value_Is_Not_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(123, null,true);

            // Assert
            Assert.AreEqual(123, pageParams.PageNumber);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageNumber_Null_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, null, true);

            // Assert
            Assert.AreEqual(1, pageParams.PageNumber);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageNumber_Zero_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(0, null, true);

            // Assert
            Assert.AreEqual(1, pageParams.PageNumber);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageNumber_Negative_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(-1, null, true);

            // Assert
            Assert.AreEqual(1, pageParams.PageNumber);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageSize_Value_Greater_Than_Max_Size_Is_Defaulted_To_MaxPageSize()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 10001, true);

            // Assert
            Assert.AreEqual(10000, pageParams.PageSize);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageSize_Non_Null_Value_Is_Not_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 543, true);

            // Assert
            Assert.AreEqual(543, pageParams.PageSize);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageSize_Null_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, null, true);

            // Assert
            Assert.AreEqual(10, pageParams.PageSize);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageSize_Zero_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 0, true);

            // Assert
            Assert.AreEqual(10, pageParams.PageSize);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_PageSize_Negative_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, -1, true);

            // Assert
            Assert.AreEqual(10, pageParams.PageSize);
        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_SortDesc_False() 
        {
            // Arrange
            PageParameters pageParam = new PageParameters(null, 543, false);

            // Assert
            Assert.AreEqual(false, pageParam.SortDesc);

        }

        [TestCategory("PageParameters")]
        [TestMethod]
        public void PageParameters_SortDesc_True()
        {
            // Arrange
            PageParameters pageParam = new PageParameters(null, null, true);

            // Assert
            Assert.AreEqual(true, pageParam.SortDesc);

        }

    }
}
