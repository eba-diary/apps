﻿using Sentry.data.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class DataInventoryController : BaseDataInventoryController
    {
        public DataInventoryController(IDataFeatures featureFlags) : base(featureFlags)
        {

        }

        public ActionResult Search(FilterSearchModel searchModel)
        {
            searchModel.FilterCategories = new List<FilterCategoryModel>()
            {
                new FilterCategoryModel()
                {
                    CategoryName = "Sensitivity",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "1",
                            OptionName = "Sensitive",
                            ResultCount = 1
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "2",
                            OptionName = "Public",
                            ResultCount = 12
                        }
                    }
                },
                new FilterCategoryModel()
                {
                    CategoryName = "Environment",
                    CategoryOptions = new List<FilterCategoryOptionModel>()
                    {
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "1",
                            OptionName = "Prod",
                            ResultCount = 3,
                            DefaultSelected = true
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "2",
                            OptionName = "NonProd",
                            ResultCount = 6
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "3",
                            OptionName = "Dev",
                            ResultCount = 0
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "4",
                            OptionName = "Test",
                            ResultCount = 1
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "5",
                            OptionName = "Qual",
                            ResultCount = 1
                        },
                        new FilterCategoryOptionModel()
                        {
                            OptionId = "6",
                            OptionName = "Beta",
                            ResultCount = 1
                        }
                    }
                }
            };

            return View(searchModel);
        }
    }
}