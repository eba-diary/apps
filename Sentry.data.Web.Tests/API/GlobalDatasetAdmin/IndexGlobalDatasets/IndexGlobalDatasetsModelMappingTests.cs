using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.API;
using System.Collections.Generic;

namespace Sentry.data.Web.Tests.API
{
    [TestClass]
    public class IndexGlobalDatasetsModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_IndexGlobalDatasetsRequestModel_To_IndexGlobalDatasetsDto()
        {
            IndexGlobalDatasetsRequestModel model = new IndexGlobalDatasetsRequestModel()
            {
                IndexAll = true,
                GlobalDatasetIds = new List<int> { 1, 2, 3 }
            };

            IndexGlobalDatasetsDto dto = _mapper.Map<IndexGlobalDatasetsDto>(model);

            Assert.IsTrue(dto.IndexAll);
            Assert.AreEqual(3, dto.GlobalDatasetIds.Count);
            Assert.AreEqual(1, dto.GlobalDatasetIds[0]);
            Assert.AreEqual(2, dto.GlobalDatasetIds[1]);
            Assert.AreEqual(3, dto.GlobalDatasetIds[2]);
        }

        [TestMethod]
        public void Map_IndexGlobalDatasetsResultDto_To_IndexGlobalDatasetsResponseModel()
        {
            IndexGlobalDatasetsResultDto model = new IndexGlobalDatasetsResultDto()
            {

            };

            IndexGlobalDatasetsResponseModel dto = _mapper.Map<IndexGlobalDatasetsResponseModel>(model);


        }
    }
}
