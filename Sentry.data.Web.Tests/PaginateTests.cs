using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Helpers;
using Sentry.data.Web.Models.ApiModels;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class PaginateTests
    {
        [TestMethod]
        public void PageResponse_Fields_Are_Inititalized()
        {
            IQueryable<int> intList = Enumerable.Range(0, 123).AsQueryable();
            PagedList<int> pagedList = PagedList<int>.ToPagedList(intList, 1, 10);


            PagedResponse<int> pageResponse = new PagedResponse<int>(pagedList);

            Assert.AreEqual(10, pageResponse.Records.Count);
            Assert.IsNotNull(pageResponse.Metadata);
        }

        [TestMethod]
        public void PagedReponseMetadata_Fields_are_Initialized()
        {
            IQueryable<int> intList = Enumerable.Range(0, 123).AsQueryable();
            PagedList<int> pagedList = PagedList<int>.ToPagedList(intList, 1, 10);

            PagedResponseMetadata<int> responseMetadata = new Models.ApiModels.PagedResponseMetadata<int>(pagedList);

            Assert.AreEqual(1, responseMetadata.CurrentPage);
            Assert.AreEqual(13, responseMetadata.TotalPages);
            Assert.AreEqual(10, responseMetadata.PageSize);
            Assert.AreEqual(123, responseMetadata.TotalCount);
            Assert.AreEqual(false, responseMetadata.HasPrevious);
            Assert.AreEqual(true, responseMetadata.HasNext);
        }
    }
}
