using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class NestHelperTests
    {
        [TestMethod]
        public void GetFilterAggregations_DataInventory_AggregationDictionary()
        {
            AggregationDictionary fields = NestHelper.GetFilterAggregations<DataInventory>();

            Assert.AreEqual(11, fields.Count());

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.ASSET));
            Assert.AreEqual("asset_cde.keyword", fields[FilterCategoryNames.DataInventory.ASSET].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.ENVIRONMENT));
            Assert.AreEqual("prod_typ.keyword", fields[FilterCategoryNames.DataInventory.ENVIRONMENT].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.DATABASE));
            Assert.AreEqual("database_nme.keyword", fields[FilterCategoryNames.DataInventory.DATABASE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.DATATYPE));
            Assert.AreEqual("column_typ.keyword", fields[FilterCategoryNames.DataInventory.DATATYPE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SOURCETYPE));
            Assert.AreEqual("source_nme.keyword", fields[FilterCategoryNames.DataInventory.SOURCETYPE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SERVER));
            Assert.AreEqual("server_nme.keyword", fields[FilterCategoryNames.DataInventory.SERVER].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLUMN));
            Assert.AreEqual("column_nme.keyword", fields[FilterCategoryNames.DataInventory.COLUMN].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLLECTIONNAME));
            Assert.AreEqual("base_nme.keyword", fields[FilterCategoryNames.DataInventory.COLLECTIONNAME].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLLECTIONTYPE));
            Assert.AreEqual("type_dsc.keyword", fields[FilterCategoryNames.DataInventory.COLLECTIONTYPE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.NULLABLE));
            Assert.AreEqual("isnullable_flg", fields[FilterCategoryNames.DataInventory.NULLABLE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SENSITIVE));
            Assert.AreEqual("issensitive_flg", fields[FilterCategoryNames.DataInventory.SENSITIVE].Terms.Field.Name);
        }

        [TestMethod]
        public void GetFilterAggregations_GlobalDataset_AggregationDictionary()
        {
            AggregationDictionary fields = NestHelper.GetFilterAggregations<GlobalDataset>();

            Assert.AreEqual(7, fields.Count());

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.DATASETASSET));
            Assert.AreEqual("datasetsaidassetcode.keyword", fields[FilterCategoryNames.Dataset.DATASETASSET].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.CATEGORY));
            Assert.AreEqual("environmentdatasets.categorycode.keyword", fields[FilterCategoryNames.Dataset.CATEGORY].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ENVIRONMENT));
            Assert.AreEqual("environmentdatasets.namedenvironment.keyword", fields[FilterCategoryNames.Dataset.ENVIRONMENT].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ENVIRONMENTTYPE));
            Assert.AreEqual("environmentdatasets.namedenvironmenttype.keyword", fields[FilterCategoryNames.Dataset.ENVIRONMENTTYPE].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.ORIGIN));
            Assert.AreEqual("environmentdatasets.originationcode.keyword", fields[FilterCategoryNames.Dataset.ORIGIN].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.SECURED));
            Assert.AreEqual("environmentdatasets.issecured", fields[FilterCategoryNames.Dataset.SECURED].Terms.Field.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.Dataset.PRODUCERASSET));
            Assert.AreEqual("environmentdatasets.environmentschemas.schemasaidassetcode.keyword", fields[FilterCategoryNames.Dataset.PRODUCERASSET].Terms.Field.Name);
        }

        [TestMethod]
        public void ToSearchQuery_DataInventory_MultiTermSearchText_BoolQuery()
        {
            FilterSearchDto filterSearchDto = new FilterSearchDto
            {
                SearchText = "search me",
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ENVIRONMENT,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = FilterCategoryOptions.ENVIRONMENT_PROD,
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.DataInventory.SENSITIVE,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "true",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "false",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.DataInventory.ASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "SAID",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "DATA",
                                Selected = true
                            }
                        }
                    }
                }
            };

            BoolQuery query = filterSearchDto.ToSearchQuery<DataInventory>();

            Assert.AreEqual(2, query.Should.Count());

            IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
            Assert.AreEqual("search me", stringQuery.Query);
            Assert.AreEqual(7, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "asset_cde"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "base_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "column_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "database_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "server_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "source_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "type_dsc"));

            stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
            Assert.AreEqual("*search* *me*", stringQuery.Query);
            Assert.AreEqual(7, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "asset_cde"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "base_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "column_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "database_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "server_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "source_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "type_dsc"));

            Assert.AreEqual(3, query.Filter.Count());

            List<IQueryContainer> filters = query.Filter.Select(x => (IQueryContainer)x).ToList();

            ITermsQuery termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "prod_typ.keyword").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual(1, termsQuery.Terms.Count());

            string term = termsQuery.Terms.First().ToString();
            Assert.AreEqual(FilterCategoryOptions.ENVIRONMENT_PROD, term);

            termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "issensitive_flg").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual(1, termsQuery.Terms.Count());

            term = termsQuery.Terms.First().ToString();
            Assert.AreEqual("true", term);

            termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "asset_cde.keyword").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual(2, termsQuery.Terms.Count());

            Assert.AreEqual("SAID", termsQuery.Terms.First().ToString());
            Assert.AreEqual("DATA", termsQuery.Terms.Last().ToString());
        }

        [TestMethod]
        public void ToSearchQuery_DataInventory_SingleTermSearchText_NoFilters_BoolQuery()
        {
            FilterSearchDto filterSearchDto = new FilterSearchDto
            {
                SearchText = "search",
                FilterCategories = new List<FilterCategoryDto>()
            };

            BoolQuery query = filterSearchDto.ToSearchQuery<DataInventory>();

            Assert.AreEqual(2, query.Should.Count());

            IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
            Assert.AreEqual("search", stringQuery.Query);
            Assert.AreEqual(7, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "asset_cde"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "base_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "column_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "database_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "server_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "source_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "type_dsc"));

            stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
            Assert.AreEqual("*search*", stringQuery.Query);
            Assert.AreEqual(7, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "asset_cde"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "base_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "column_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "database_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "server_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "source_nme"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "type_dsc"));

            Assert.IsNull(query.Filter);
        }

        [TestMethod]
        public void ToSearchQuery_DataInventory_NoSearchText_NoFilters_BoolQuery()
        {
            FilterSearchDto filterSearchDto = new FilterSearchDto
            {
                SearchText = "",
                FilterCategories = new List<FilterCategoryDto>()
            };

            BoolQuery query = filterSearchDto.ToSearchQuery<DataInventory>();

            Assert.IsNull(query.Should);
            Assert.IsNull(query.Filter);
        }

        [TestMethod]
        public void ToSearchQuery_GlobalDataset_BoolQuery()
        {
            FilterSearchDto filterSearchDto = new FilterSearchDto
            {
                SearchText = "search me",
                FilterCategories = new List<FilterCategoryDto>
                {
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.PRODUCERASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "TEST",
                                Selected = true
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.SECURED,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "true",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "false",
                                Selected = false
                            }
                        }
                    },
                    new FilterCategoryDto
                    {
                        CategoryName = FilterCategoryNames.Dataset.DATASETASSET,
                        CategoryOptions = new List<FilterCategoryOptionDto>
                        {
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "SAID",
                                Selected = true
                            },
                            new FilterCategoryOptionDto
                            {
                                OptionValue = "DATA",
                                Selected = true
                            }
                        }
                    }
                }
            };

            BoolQuery query = filterSearchDto.ToSearchQuery<GlobalDataset>();

            Assert.AreEqual(2, query.Should.Count());

            IQueryStringQuery stringQuery = ((IQueryContainer)query.Should.First()).QueryString;
            Assert.AreEqual("search me", stringQuery.Query);
            Assert.AreEqual(4, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 2));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

            stringQuery = ((IQueryContainer)query.Should.Last()).QueryString;
            Assert.AreEqual("*search* *me*", stringQuery.Query);
            Assert.AreEqual(4, stringQuery.Fields.Count());
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "datasetname" && x.Boost == 2));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.datasetdescription"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemaname"));
            Assert.IsTrue(stringQuery.Fields.Any(x => x.Name == "environmentdatasets.environmentschemas.schemadescription"));

            Assert.AreEqual(3, query.Filter.Count());

            List<IQueryContainer> filters = query.Filter.Select(x => (IQueryContainer)x).ToList();

            ITermsQuery termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "datasetsaidassetcode.keyword").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual(2, termsQuery.Terms.Count());
            Assert.AreEqual("SAID", termsQuery.Terms.First().ToString());
            Assert.AreEqual("DATA", termsQuery.Terms.Last().ToString());

            termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "environmentdatasets.issecured").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual(1, termsQuery.Terms.Count());
            Assert.AreEqual("true", termsQuery.Terms.First().ToString());

            termsQuery = filters.FirstOrDefault(x => x.Terms.Field.Name == "environmentdatasets.environmentschemas.schemasaidassetcode.keyword").Terms;
            Assert.IsNotNull(termsQuery);
            Assert.AreEqual("TEST", termsQuery.Terms.First().ToString());
        }

        [TestMethod]
        public void ToFilterCategories_AggregateDictionary_DataInventory_FilterCategoryDtos()
        {
            AggregateDictionary aggregates = new AggregateDictionary(new Dictionary<string, IAggregate>
            {
                [FilterCategoryNames.DataInventory.COLUMN] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 5,
                                Key = "Value1"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 3,
                                Key = "Value2"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 10,
                                Key = "Value3"
                            }
                        }.AsReadOnly()
                },
                [FilterCategoryNames.DataInventory.SENSITIVE] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "0",
                                KeyAsString = "false"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 6,
                                Key = "1",
                                KeyAsString = "true"
                            }
                        }.AsReadOnly()
                },
                [FilterCategoryNames.DataInventory.COLLECTIONNAME] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>().AsReadOnly()
                }
            });

            List<FilterCategoryDto> requestedFilters = new List<FilterCategoryDto>
            {
                new FilterCategoryDto
                {
                    CategoryName = FilterCategoryNames.DataInventory.COLUMN,
                    CategoryOptions = new List<FilterCategoryOptionDto>
                    {
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "Value1",
                            Selected = true
                        },
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "Value2",
                            Selected = false
                        },
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "Value3",
                            Selected = true
                        }
                    }
                }
            };

            List<FilterCategoryDto> filters = aggregates.ToFilterCategories<DataInventory>(requestedFilters);

            Assert.AreEqual(2, filters.Count);
            
            FilterCategoryDto filter = filters[0];
            Assert.AreEqual(FilterCategoryNames.DataInventory.COLUMN, filter.CategoryName);
            Assert.AreEqual(3, filter.CategoryOptions.Count);
            Assert.IsFalse(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            FilterCategoryOptionDto option = filter.CategoryOptions[0];
            Assert.AreEqual("Value1", option.OptionValue);
            Assert.AreEqual(5, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.COLUMN, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("Value2", option.OptionValue);
            Assert.AreEqual(3, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.COLUMN, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[2];
            Assert.AreEqual("Value3", option.OptionValue);
            Assert.AreEqual(10, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.COLUMN, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            filter = filters[1];
            Assert.AreEqual(FilterCategoryNames.DataInventory.SENSITIVE, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsFalse(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("false", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.SENSITIVE, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("true", option.OptionValue);
            Assert.AreEqual(6, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.DataInventory.SENSITIVE, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);
        }

        [TestMethod]
        public void ToFilterCategories_AggregateDictionary_GlobalDataset_FilterCategoryDtos()
        {
            AggregateDictionary aggregates = new AggregateDictionary(new Dictionary<string, IAggregate>
            {
                [FilterCategoryNames.Dataset.DATASETASSET] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 5,
                                Key = "SAID"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 3,
                                Key = "DATA"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 10,
                                Key = "OTHR"
                            }
                        }.AsReadOnly()
                },
                [FilterCategoryNames.Dataset.SECURED] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 4,
                                Key = "0",
                                KeyAsString = "false"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 6,
                                Key = "1",
                                KeyAsString = "true"
                            }
                        }.AsReadOnly()
                },
                [FilterCategoryNames.Dataset.PRODUCERASSET] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>
                        {
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 2,
                                Key = "SAID"
                            },
                            new KeyedBucket<object>(new Dictionary<string, IAggregate>())
                            {
                                DocCount = 8,
                                Key = "OTHR"
                            }
                        }.AsReadOnly()
                },
                [FilterCategoryNames.Dataset.ORIGIN] = new BucketAggregate()
                {
                    SumOtherDocCount = 0,
                    Items = new List<KeyedBucket<object>>().AsReadOnly()
                }
            });

            List<FilterCategoryDto> requestedFilters = new List<FilterCategoryDto>
            {
                new FilterCategoryDto
                {
                    CategoryName = FilterCategoryNames.Dataset.DATASETASSET,
                    CategoryOptions = new List<FilterCategoryOptionDto>
                    {
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "SAID",
                            Selected = true
                        },
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "DATA",
                            Selected = false
                        },
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "OTHR",
                            Selected = true
                        }
                    }
                },
                new FilterCategoryDto
                {
                    CategoryName = FilterCategoryNames.Dataset.PRODUCERASSET,
                    CategoryOptions = new List<FilterCategoryOptionDto>
                    {
                        new FilterCategoryOptionDto
                        {
                            OptionValue = "SAID",
                            Selected = true
                        }
                    }
                }
            };

            List<FilterCategoryDto> filters = aggregates.ToFilterCategories<GlobalDataset>(requestedFilters);

            Assert.AreEqual(3, filters.Count);

            FilterCategoryDto filter = filters[0];
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, filter.CategoryName);
            Assert.AreEqual(3, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            FilterCategoryOptionDto option = filter.CategoryOptions[0];
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.AreEqual(5, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("DATA", option.OptionValue);
            Assert.AreEqual(3, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[2];
            Assert.AreEqual("OTHR", option.OptionValue);
            Assert.AreEqual(10, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.DATASETASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            filter = filters[1];
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("false", option.OptionValue);
            Assert.AreEqual(4, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("true", option.OptionValue);
            Assert.AreEqual(6, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.SECURED, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);

            filter = filters[2];
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, filter.CategoryName);
            Assert.AreEqual(2, filter.CategoryOptions.Count);
            Assert.IsTrue(filter.HideResultCounts);
            Assert.IsFalse(filter.DefaultCategoryOpen);

            option = filter.CategoryOptions[0];
            Assert.AreEqual("SAID", option.OptionValue);
            Assert.AreEqual(2, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, option.ParentCategoryName);
            Assert.IsTrue(option.Selected);

            option = filter.CategoryOptions[1];
            Assert.AreEqual("OTHR", option.OptionValue);
            Assert.AreEqual(8, option.ResultCount);
            Assert.AreEqual(FilterCategoryNames.Dataset.PRODUCERASSET, option.ParentCategoryName);
            Assert.IsFalse(option.Selected);
        }
    }
}
