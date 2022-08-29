﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetExtensionsTests
    {
        [TestMethod]
        public void ToDatasetTileDto_Dataset_DatasetTileDto()
        {
            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "DatasetName",
                DatasetDesc = "Description",
                ObjectStatus = ObjectStatusEnum.Active,
                IsSecured = true,
                DatasetDtm = new DateTime(2022, 8, 19, 8, 0, 0),
                DatasetCategories = new List<Category>()
                {
                    new Category()
                    {
                        Name = "Category",
                        Color = "Blue"
                    }
                }
            };

            DatasetTileDto dto = dataset.ToDatasetTileDto();

            Assert.AreEqual("1", dto.Id);
            Assert.AreEqual("DatasetName", dto.Name);
            Assert.AreEqual("Description", dto.Description);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.Status);
            Assert.IsTrue(dto.IsSecured);
            Assert.AreEqual("Blue", dto.Color);
            Assert.AreEqual("Category", dto.Category);
        }
    }
}