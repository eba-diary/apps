﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Models.ApiModels.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void ToDto_SchemaInfoModel_ReturnFileSchemaDto()
        {
            SchemaInfoModel mdl = new SchemaInfoModel()
            {
                ConfigId = 1,
                SchemaId = 1,
                SchemaEntity_NME = "EntityName",
                Name = "SchemaName",
                Description = "Schema Description",
                StorageCode = "SC",
                Format = "CSV",
                CurrentView = true,
                Delimiter = ",",
                HasHeader = false,
                HiveTable = "HiveTable",
                HiveDatabase = "HiveDB",
                HiveLocation = "HiveLocation",
                HiveTableStatus = "HiveStatus",
                Options = new List<string>() { "CLA1286_KafkaFlag|true", "CLA1396_NewEtlColumns|false", "CLA3014_LoadDataToSnowflake|true" },
                DeleteInd = false,
                SnowflakeDatabase = "SFDB",
                SnowflakeSchema = "SFSchema",
                SnowflakeTable = "SFTable",
                SnowflakeStage = "SFStage",
                SnowflakeStatus = "SFStatus",
                SnowflakeWarehouse = "SFWarehouse",
                ObjectStatus = "ACTIVE",
                SchemaRootPath = new string[] { "start", "middle", "end" },
                ParquetStorageBucket = "PSB",
                ParquetStoragePrefix = "PSP"
            };

            FileSchemaDto dto = mdl.ToDto(1, (x) => 1);

            Assert.AreEqual(1, dto.SchemaId);
            Assert.AreEqual("EntityName", dto.SchemaEntity_NME);
            Assert.AreEqual("SchemaName", dto.Name);
            Assert.AreEqual("Schema Description", dto.Description);
            Assert.AreEqual(1, dto.ParentDatasetId);
            Assert.AreEqual(ObjectStatusEnum.Active, dto.ObjectStatus);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(null, dto.DeleteIssuer);
            Assert.AreEqual(DateTime.MinValue, dto.DeleteIssueDTM);
            Assert.AreEqual(false, dto.CLA1396_NewEtlColumns);
            Assert.AreEqual(true, dto.CLA1286_KafkaFlag);
            Assert.AreEqual(true, dto.CLA3014_LoadDataToSnowflake);
            Assert.AreEqual(false, dto.CLA1580_StructureHive);
            Assert.AreEqual(false, dto.CLA2472_EMRSend);
            Assert.AreEqual(1, dto.FileExtensionId);
            Assert.AreEqual("CSV", dto.FileExtensionName);
            Assert.AreEqual(",", dto.Delimiter);
            Assert.AreEqual(false, dto.HasHeader);
            Assert.AreEqual(true, dto.CreateCurrentView);
            Assert.AreEqual("HiveTable", dto.HiveTable);
            Assert.AreEqual("HiveDB", dto.HiveDatabase);
            Assert.AreEqual("HiveLocation", dto.HiveLocation);
            Assert.AreEqual("HiveStatus", dto.HiveStatus);
            Assert.AreEqual("SC", dto.StorageCode);
            Assert.AreEqual(null, dto.StorageLocation);
            Assert.AreEqual(null, dto.RawQueryStorage);
            Assert.AreEqual("SFDB", dto.SnowflakeDatabase);
            Assert.AreEqual("SFSchema", dto.SnowflakeSchema);
            Assert.AreEqual("SFTable", dto.SnowflakeTable);
            Assert.AreEqual("SFStage", dto.SnowflakeStage);
            Assert.AreEqual("SFStatus", dto.SnowflakeStatus);
            Assert.AreEqual("SFWarehouse", dto.SnowflakeWarehouse);
            Assert.AreEqual("start,middle,end", dto.SchemaRootPath);
            Assert.AreEqual("PSB", dto.ParquetStorageBucket);
            Assert.AreEqual("PSP", dto.ParquetStoragePrefix);
        }

        [TestMethod]
        public void ToDto_SchemaInfoModel_NullOptions_NullSchemaRootPath_ReturnFileSchemaDto()
        {
            SchemaInfoModel mdl = new SchemaInfoModel()
            {
                Options = null,
                SchemaRootPath = null
            };

            FileSchemaDto dto = mdl.ToDto(1, (x) => 1);

            Assert.AreEqual(false, dto.CLA1396_NewEtlColumns);
            Assert.IsFalse(dto.CLA1286_KafkaFlag);
            Assert.IsFalse(dto.CLA3014_LoadDataToSnowflake);
            Assert.IsFalse(dto.CLA1580_StructureHive);
            Assert.IsFalse(dto.CLA2472_EMRSend);
            Assert.IsNull(dto.SchemaRootPath);
        }

        [TestMethod]
        public void ToModel_SchemaRevisionJsonStructureDto_SchemaRevisionJsonStructureModel()
        {
            SchemaRevisionJsonStructureDto dto = new SchemaRevisionJsonStructureDto()
            {
                Revision = new SchemaRevisionDto()
                {
                    RevisionId = 1,
                    RevisionNumber = 1,
                    SchemaRevisionName = "RevisionName",
                    CreatedBy = "000000",
                    CreatedByName = "User",
                    CreatedDTM = DateTime.Parse("2021-12-15 15:15:00"),
                    LastUpdatedDTM = DateTime.Parse("2021-12-15 15:15:00"),
                    JsonSchemaObject = "Object"
                },
                JsonStructure = new JObject() { { "Property", "Value" } }
            };

            SchemaRevisionJsonStructureModel mdl = dto.ToModel();

            Assert.AreEqual(1, mdl.Revision.RevisionId);
            Assert.AreEqual(1, mdl.Revision.RevisionNumber);
            Assert.AreEqual("RevisionName", mdl.Revision.SchemaRevisionName);
            Assert.AreEqual("000000", mdl.Revision.CreatedBy);
            Assert.AreEqual("User", mdl.Revision.CreatedByName);
            Assert.AreEqual("2021-12-15T15:15:00", mdl.Revision.CreatedDTM);
            Assert.AreEqual("2021-12-15T15:15:00", mdl.Revision.LastUpdatedDTM);
            Assert.AreEqual(null, mdl.Revision.JsonSchemaObject);
            Assert.IsNotNull(mdl.JsonStructure);
        }

        [TestMethod]
        public void ToModel_SchemaRevisionJsonStructureDto_NullRevision_SchemaRevisionJsonStructureModel()
        {
            SchemaRevisionJsonStructureDto dto = new SchemaRevisionJsonStructureDto()
            {
                Revision = null,
                JsonStructure = null
            };

            SchemaRevisionJsonStructureModel mdl = dto.ToModel();

            Assert.IsNull(mdl.Revision);
            Assert.IsNull(mdl.JsonStructure);
        }

        [TestMethod]
        public void ToDto_SchemaRevision_SchemaRevisionDto()
        {
            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Id = 1,
                Revision_NBR = 1,
                SchemaRevision_Name = "RevisionName",
                CreatedBy = "000000",
                CreatedDTM = DateTime.Parse("2021-12-15 15:15:00"),
                LastUpdatedDTM = DateTime.Parse("2021-12-15 15:15:00"),
                JsonSchemaObject = "Object"
            };

            SchemaRevisionDto dto = revision.ToDto();

            Assert.AreEqual(1, dto.RevisionId);
            Assert.AreEqual(1, dto.RevisionNumber);
            Assert.AreEqual("RevisionName", dto.SchemaRevisionName);
            Assert.AreEqual("000000", dto.CreatedBy);
            Assert.AreEqual(DateTime.Parse("2021-12-15 15:15:00"), dto.CreatedDTM);
            Assert.AreEqual(DateTime.Parse("2021-12-15 15:15:00"), dto.LastUpdatedDTM);
            Assert.AreEqual("Object", dto.JsonSchemaObject);
        }

        [TestMethod]
        public void ToDto_FilterSearchModel_DaleSearchDto()
        {
            FilterSearchModel model = new FilterSearchModel()
            {
                SearchText = "Search",
                FilterCategories = new List<FilterCategoryModel>()
                {
                    new FilterCategoryModel()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value",
                                ResultCount = 10,
                                ParentCategoryName = "Category",
                                Selected = true
                            },
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value2",
                                ResultCount = 4,
                                ParentCategoryName = "Category",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryModel()
                    {
                        CategoryName = "Category2",
                        CategoryOptions = new List<FilterCategoryOptionModel>()
                        {
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value",
                                ResultCount = 5,
                                ParentCategoryName = "Category2",
                                Selected = false
                            },
                            new FilterCategoryOptionModel()
                            {
                                OptionValue = "Value2",
                                ResultCount = 9,
                                ParentCategoryName = "Category2",
                                Selected = false
                            }
                        }
                    }
                }
            };

            DaleSearchDto dto = model.ToDto();

            Assert.AreEqual("Search", dto.Criteria);
            Assert.AreEqual(2, dto.FilterCategories.Count);

            FilterCategoryDto categoryDto = dto.FilterCategories.First();
            Assert.AreEqual("Category", categoryDto.CategoryName);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            FilterCategoryOptionDto optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual("Value", optionDto.OptionValue);
            Assert.AreEqual(10, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsTrue(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionDto.OptionValue);
            Assert.AreEqual(4, optionDto.ResultCount);
            Assert.AreEqual("Category", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);

            categoryDto = dto.FilterCategories.Last();
            Assert.AreEqual("Category2", categoryDto.CategoryName);
            Assert.AreEqual(2, categoryDto.CategoryOptions.Count);

            optionDto = categoryDto.CategoryOptions.First();
            Assert.AreEqual("Value", optionDto.OptionValue);
            Assert.AreEqual(5, optionDto.ResultCount);
            Assert.AreEqual("Category2", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);

            optionDto = categoryDto.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionDto.OptionValue);
            Assert.AreEqual(9, optionDto.ResultCount);
            Assert.AreEqual("Category2", optionDto.ParentCategoryName);
            Assert.IsFalse(optionDto.Selected);
        }

        [TestMethod]
        public void ToModel_FilterSearchDto_FilterSearchModel()
        {
            FilterSearchDto dto = new FilterSearchDto()
            {
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value",
                                ResultCount = 10,
                                ParentCategoryName = "Category",
                                Selected = true
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value2",
                                ResultCount = 4,
                                ParentCategoryName = "Category",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category2",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value",
                                ResultCount = 5,
                                ParentCategoryName = "Category2",
                                Selected = false
                            },
                            new FilterCategoryOptionDto()
                            {
                                OptionValue = "Value2",
                                ResultCount = 9,
                                ParentCategoryName = "Category2",
                                Selected = false
                            }
                        }
                    }
                }
            };

            FilterSearchModel model = dto.ToModel();

            Assert.AreEqual(2, model.FilterCategories.Count);

            FilterCategoryModel categoryModel = model.FilterCategories.First();
            Assert.AreEqual("Category", categoryModel.CategoryName);
            Assert.AreEqual(2, categoryModel.CategoryOptions.Count);

            FilterCategoryOptionModel optionModel = categoryModel.CategoryOptions.First();
            Assert.AreEqual("Value", optionModel.OptionValue);
            Assert.AreEqual(10, optionModel.ResultCount);
            Assert.AreEqual("Category", optionModel.ParentCategoryName);
            Assert.IsTrue(optionModel.Selected);

            optionModel = categoryModel.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionModel.OptionValue);
            Assert.AreEqual(4, optionModel.ResultCount);
            Assert.AreEqual("Category", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);

            categoryModel = model.FilterCategories.Last();
            Assert.AreEqual("Category2", categoryModel.CategoryName);
            Assert.AreEqual(2, categoryModel.CategoryOptions.Count);

            optionModel = categoryModel.CategoryOptions.First();
            Assert.AreEqual("Value", optionModel.OptionValue);
            Assert.AreEqual(5, optionModel.ResultCount);
            Assert.AreEqual("Category2", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);

            optionModel = categoryModel.CategoryOptions.Last();
            Assert.AreEqual("Value2", optionModel.OptionValue);
            Assert.AreEqual(9, optionModel.ResultCount);
            Assert.AreEqual("Category2", optionModel.ParentCategoryName);
            Assert.IsFalse(optionModel.Selected);
        }
    }
}