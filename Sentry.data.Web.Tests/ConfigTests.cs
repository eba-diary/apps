using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web;
using Sentry.data.Web.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sentry.data.Tests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        [TestCategory("Config Controller")]
        public void Config_Controller_Index_Returns_Index_View_With_Valid_ID()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset();
            var dfc = MockClasses.MockDataFileConfig(ds);

            ds.DatasetFileConfigs.Add(dfc);

            var cc = MockControllers.MockConfigController(dfc, user);

            var result = cc.Index(dfc.ParentDataset.DatasetId) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(BaseDatasetModel));

            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Config Controller")]
        public void Config_Controller_Index_Returns_Create_View()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset();
            var dfc = MockClasses.MockDataFileConfig(ds);

            ds.DatasetFileConfigs.Add(dfc);

            var cc = MockControllers.MockConfigController(dfc, user);

            var result = cc.Create(dfc.ParentDataset.DatasetId) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(DatasetFileConfigsModel));

            Assert.AreEqual("", result.ViewName);
        }
    }
}
