using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.Extensions;
using Sentry.data.Web.Models.ApiModels.Job;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class JobExtensionsTests
    {
        [TestMethod]
        public void ToModel_DfsMonitorDto_DfsMonitorModel()
        {
            DfsMonitorDto dto = new DfsMonitorDto()
            {
                JobId = 1,
                MonitorTarget = "Target"
            };

            DfsMonitorModel model = dto.ToModel();

            Assert.AreEqual(1, model.JobId);
            Assert.AreEqual("Target", model.MonitorTarget);
        }
    }
}
