using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Web.Tests.API;
using StructureMap.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class ViewModelMappingTests : BaseModelMappingTests
    {
        [TestMethod]
        public void Map_GlobalDatasetPageRequestViewModel_GlobalDatasetPageRequestDto()
        {
            GlobalDatasetPageRequestViewModel viewModel = new GlobalDatasetPageRequestViewModel
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = 2,
                Layout = 3,
                GlobalDatasets = new List<GlobalDatasetViewModel>
                {
                    new GlobalDatasetViewModel
                    {
                        GlobalDatasetId = 1,
                        DatasetName = "Name",
                        DatasetDescription = "Description",
                        DatasetSaidAssetCode = "SAID",
                        CategoryCode = "Category",
                        NamedEnvironments = new List<string> { "DEV", "TEST" },
                        IsFavorite = true,
                        IsSecured = true,
                        DatasetDetailPage = "/page/1"
                    },
                    new GlobalDatasetViewModel
                    {
                        GlobalDatasetId = 2,
                        DatasetName = "Name 2",
                        DatasetDescription = "Description 2",
                        DatasetSaidAssetCode = "DATA",
                        CategoryCode = "Category 2",
                        NamedEnvironments = new List<string> { "NRTEST" },
                        IsFavorite = false,
                        IsSecured = false,
                        DatasetDetailPage = "/page/2"
                    }
                }
            };

            GlobalDatasetPageRequestDto dto = _mapper.Map<GlobalDatasetPageRequestDto>(viewModel);

            Assert.AreEqual(1, dto.PageNumber);
            Assert.AreEqual(10, dto.PageSize);
            Assert.AreEqual(2, dto.SortBy);
            Assert.AreEqual(3, dto.Layout);
            Assert.AreEqual(2, dto.GlobalDatasets.Count);

            SearchGlobalDatasetDto globalDataset = dto.GlobalDatasets[0];
            Assert.AreEqual(1, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name", globalDataset.DatasetName);
            Assert.AreEqual("Description", globalDataset.DatasetDescription);
            Assert.AreEqual("SAID", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category", globalDataset.CategoryCode);
            Assert.AreEqual(2, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("DEV", globalDataset.NamedEnvironments[0]);
            Assert.AreEqual("TEST", globalDataset.NamedEnvironments[1]);
            Assert.IsTrue(globalDataset.IsFavorite);
            Assert.IsTrue(globalDataset.IsSecured);
            Assert.AreEqual("/page/1", globalDataset.DatasetDetailPage);

            globalDataset = dto.GlobalDatasets[1];
            Assert.AreEqual(2, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 2", globalDataset.DatasetName);
            Assert.AreEqual("Description 2", globalDataset.DatasetDescription);
            Assert.AreEqual("DATA", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category 2", globalDataset.CategoryCode);
            Assert.AreEqual(1, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("NRTEST", globalDataset.NamedEnvironments[0]);
            Assert.IsFalse(globalDataset.IsFavorite);
            Assert.IsFalse(globalDataset.IsSecured);
            Assert.AreEqual("/page/2", globalDataset.DatasetDetailPage);
        }

        [TestMethod]
        public void Map_GlobalDatasetPageResultDto_GlobalDatasetResultsViewModel()
        {
            GlobalDatasetPageResultDto dto = new GlobalDatasetPageResultDto
            {
                PageNumber = 1,
                PageSize = 15,
                SortBy = 2,
                Layout = 1,
                GlobalDatasets = new List<SearchGlobalDatasetDto>
                {
                    new SearchGlobalDatasetDto
                    {
                        GlobalDatasetId = 1,
                        DatasetName = "Name",
                        DatasetDescription = "Description",
                        DatasetSaidAssetCode = "SAID",
                        CategoryCode = "Category",
                        NamedEnvironments = new List<string> { "DEV", "TEST" },
                        IsFavorite = true,
                        IsSecured = true,
                        DatasetDetailPage = "/page/1"
                    },
                    new SearchGlobalDatasetDto
                    {
                        GlobalDatasetId = 2,
                        DatasetName = "Name 2",
                        DatasetDescription = "Description 2",
                        DatasetSaidAssetCode = "DATA",
                        CategoryCode = "Category 2",
                        NamedEnvironments = new List<string> { "NRTEST" },
                        IsFavorite = false,
                        IsSecured = false,
                        DatasetDetailPage = "/page/2"
                    }
                },
                TotalResults = 2
            };

            GlobalDatasetResultsViewModel viewModel = _mapper.Map<GlobalDatasetResultsViewModel>(dto);


            Assert.AreEqual(1, viewModel.PageItems.Count);
            Assert.AreEqual("1", viewModel.PageItems.First(x => x.IsActive).PageNumber);
            Assert.AreEqual(4, viewModel.PageSizeOptions.Count);
            Assert.AreEqual("15", viewModel.PageSizeOptions.First(x => x.Selected).Value);
            Assert.AreEqual(3, viewModel.SortByOptions.Count);
            Assert.AreEqual("2", viewModel.SortByOptions.First(x => x.Selected).Value);
            Assert.AreEqual(2, viewModel.LayoutOptions.Count);
            Assert.AreEqual("1", viewModel.LayoutOptions.First(x => x.Selected).Value);

            Assert.AreEqual(2, dto.TotalResults);
            Assert.AreEqual(2, viewModel.GlobalDatasets.Count);

            GlobalDatasetViewModel globalDataset = viewModel.GlobalDatasets[0];
            Assert.AreEqual(1, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name", globalDataset.DatasetName);
            Assert.AreEqual("Description", globalDataset.DatasetDescription);
            Assert.AreEqual("SAID", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category", globalDataset.CategoryCode);
            Assert.AreEqual(2, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("DEV", globalDataset.NamedEnvironments[0]);
            Assert.AreEqual("TEST", globalDataset.NamedEnvironments[1]);
            Assert.IsTrue(globalDataset.IsFavorite);
            Assert.IsTrue(globalDataset.IsSecured);
            Assert.AreEqual("/page/1", globalDataset.DatasetDetailPage);

            globalDataset = viewModel.GlobalDatasets[1];
            Assert.AreEqual(2, globalDataset.GlobalDatasetId);
            Assert.AreEqual("Name 2", globalDataset.DatasetName);
            Assert.AreEqual("Description 2", globalDataset.DatasetDescription);
            Assert.AreEqual("DATA", globalDataset.DatasetSaidAssetCode);
            Assert.AreEqual("Category 2", globalDataset.CategoryCode);
            Assert.AreEqual(1, globalDataset.NamedEnvironments.Count);
            Assert.AreEqual("NRTEST", globalDataset.NamedEnvironments[0]);
            Assert.IsFalse(globalDataset.IsFavorite);
            Assert.IsFalse(globalDataset.IsSecured);
            Assert.AreEqual("/page/2", globalDataset.DatasetDetailPage);
        }
    }
}
