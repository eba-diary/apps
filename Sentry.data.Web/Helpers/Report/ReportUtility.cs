using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using System.Web.Mvc;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Helpers
{
    public static class ReportUtility
    {
        public static List<T> IntersectAllIfEmpty<T>(params IEnumerable<T>[] lists)
        {
            IEnumerable<T> results = null;

            lists = lists.Where(l => l.Any()).ToArray();

            if (lists.Length > 0)
            {
                results = lists[0];

                for (int i = 1; i < lists.Length; i++)
                    results = results.Intersect(lists[i]);
            }
            else
            {
                results = new T[0];
            }

            List<T> var = results.ToList();

            //return results;
            return var;
        }
        public static string TimeDisplay(DateTime dt)
        {
            string result;
            TimeSpan span = DateTime.Now - dt;

            if (dt == DateTime.MinValue)
            {
                result = "No Metadata Found";
            }
            //else if (span.Days > 30)
            //{
            //    int months = (span.Days / 30);
            //    result = string.Format("{0} {1} ago", months, months == 1 ? "month" : "months");
            //}
            else if (span.TotalDays > 1)
            {
                result = dt.ToString("MM/dd/yyyy hh:mm:ss tt"); //12hr AM/PM
            }
            else if (span.TotalHours > 1)
            {
                var h = String.Format("{0:0}", span.TotalHours);

                result = string.Format("{0} {1} ago", h, h == "1" ? "hour" : "hours");
            }
            else if (span.TotalMinutes > 1)
            {
                var m = String.Format("{0:0}", span.TotalMinutes);

                result = string.Format("{0} {1} ago", m, m == "1" ? "minute" : "minutes");
            }
            else if (span.TotalSeconds > 5)
            {
                result = string.Format("{0} seconds ago", String.Format("{0:0}", span.TotalSeconds));
            }
            else
            {
                result = "just now";
            }

            return result;
        }

        [Obsolete("The function is fine but I wanted that this should not use the domain context and these should all be Enums")]
        public static void SetupLists(IDatasetContext _datasetContext, BusinessIntelligenceModel model)
        {
            var temp = GetCategoryList(_datasetContext).ToList();

            if (model.DatasetCategoryIds?.Count > 0)
            {
                foreach (var cat in model.DatasetCategoryIds)
                {
                    foreach (var t in temp)
                    {
                        if (t.Value == cat.ToString())
                        {
                            t.Selected = true;
                        }
                    }
                }
            }
            else
            {
                // add an empty option as the first item in the list; needed in order for the select2 placeholder text
                temp = temp.Prepend(new SelectListItem
                {
                    Selected = true,
                    Disabled = true,
                    Text = "",
                    Value = "0"
                }).ToList();
            }

            model.AllCategories = temp.OrderBy(x => x.Value);


            //Business Units
            temp = GetBusinessUnits(_datasetContext).ToList();

            if (model.DatasetBusinessUnitIds?.Count > 0)
            {
                foreach (var bu in model.DatasetBusinessUnitIds)
                {
                    foreach (var t in temp)
                    {
                        if (t.Value == bu.ToString())
                        {
                            t.Selected = true;
                        }
                    }
                }
            }
            else
            {
                // add an empty option as the first item in the list; needed in order for the select2 placeholder text
                temp = temp.Prepend(new SelectListItem
                {
                    Selected = true,
                    Disabled = true,
                    Text = "",
                    Value = "0"
                }).ToList();
            }

            model.AllBusinessUnits = temp;


            //Functions
            temp = GetDatasetFunctions(_datasetContext).ToList();

            if (model.DatasetFunctionIds?.Count > 0)
            {
                foreach (var id in model.DatasetFunctionIds)
                {
                    foreach (var t in temp)
                    {
                        if (t.Value == id.ToString())
                        {
                            t.Selected = true;
                        }
                    }
                }
            }
            else
            {
                // add an empty option as the first item in the list; needed in order for the select2 placeholder text
                temp = temp.Prepend(new SelectListItem
                {
                    Selected = true,
                    Disabled = true,
                    Text = "",
                    Value = "0"
                }).ToList();
            }

            model.AllDatasetFunctions = temp;


            //Business Intelligence Frequency
            temp = new List<SelectListItem>();

            // add an empty option as the first item in the list; needed in order for the select2 placeholder text
            temp.Add(new SelectListItem()
            {
                Text = "",
                Value = ""
            });

            temp.AddRange(GetDatasetFrequencyListItems().ToList());

            model.AllFrequencies = temp.OrderBy(x => x.Value);

            // Business Intelligence Exhibit Type
            temp = new List<SelectListItem>();

            // add an empty option as the first item in the list; needed in order for the select2 placeholder text
            temp.Add(new SelectListItem()
            {
                Text = "",
                Value = ""
            });

            temp.AddRange(default(ReportType).ToEnumSelectList((model).FileTypeId.ToString()).ToList());


            model.AllDataFileTypes = temp;
        }

        public static IEnumerable<SelectListItem> GetCategoryList(IDatasetContext _datasetContext)
        {
            IEnumerable<SelectListItem> var = _datasetContext.Categories.Where(x => x.ObjectType == GlobalConstants.DataEntityCodes.REPORT).
                                                                                            Select((c) => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

            return var;
        }
        public static IEnumerable<SelectListItem> GetBusinessUnits(IDatasetContext dsContext)
        {
            return dsContext.BusinessUnits.
                OrderBy(o => o.Sequence).
                ThenBy(t => t.Name).
                Select((x) => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }
        public static IEnumerable<SelectListItem> GetDatasetFunctions(IDatasetContext dsContext)
        {
            return dsContext.DatasetFunctions.
                OrderBy(o => o.Sequence).
                ThenBy(t => t.Name).
                Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
        }
        public static IEnumerable<Dataset> GetDatasetByCategoryId(IDatasetContext _datasetContext, int id)
        {
            IEnumerable<Dataset> dsQ = _datasetContext.GetDatasetByCategoryID(id);
            return dsQ;
        }
        public static IEnumerable<SelectListItem> GetDatasetFrequencyListItems(string freq = null)
        {
            List<SelectListItem> items;

            if (freq == null)
            {
                items = Enum.GetValues(typeof(ReportFrequency)).Cast<ReportFrequency>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            }
            else
            {
                items = Enum.GetValues(typeof(ReportFrequency)).Cast<ReportFrequency>().Select(v =>
                    new SelectListItem
                    {
                        Selected = (v.ToString() == freq),
                        Text = v.ToString(),
                        Value = v.ToString()
                    }).ToList();
            }
            return items;
        }

        public static IEnumerable<SelectListItem> GetDatasetOriginationListItems()
        {
            List<SelectListItem> items = Enum.GetValues(typeof(DatasetOriginationCode)).Cast<DatasetOriginationCode>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            return items;
        }
        public static IEnumerable<SelectListItem> GetDatasetScopeTypesListItems(IDatasetContext _datasetContext, int id = -1)
        {
            IEnumerable<SelectListItem> dScopeTypes;
            if (id == -1)
            {
                dScopeTypes = _datasetContext.GetAllDatasetScopeTypes()
                    .Select((c) => new SelectListItem { Text = c.Name, Value = c.ScopeTypeId.ToString() });
            }
            else
            {
                dScopeTypes = _datasetContext.GetAllDatasetScopeTypes()
                    .Select((c) => new SelectListItem
                    {
                        Selected = (c.ScopeTypeId == id),
                        Text = c.Name,
                        Value = c.ScopeTypeId.ToString()
                    });
            }

            return dScopeTypes;
        }
        public static IEnumerable<SelectListItem> GetFileExtensionListItems(IDatasetContext _datasetContext, int id = -1)
        {

            IEnumerable<SelectListItem> dFileExtensions;
            if (id == -1)
            {
                dFileExtensions = _datasetContext.FileExtensions
                    .Select((c) => new SelectListItem
                    {
                        Selected = c.Name.Contains(GlobalConstants.ExtensionNames.ANY),
                        Text = c.Name.Trim(),
                        Value = c.Id.ToString()
                    });
            }
            else
            {
                dFileExtensions = _datasetContext.FileExtensions
                    .Select((c) => new SelectListItem
                    {
                        Selected = c.Id == id,
                        Text = c.Name.Trim(),
                        Value = c.Id.ToString()
                    });
            }

            return dFileExtensions.OrderByDescending(x => x.Selected);
        }
        public static RetrieverJob InstantiateJobsForCreation(DatasetFileConfig dfc, DataSource dataSource)
        {
            Compression compression = new Compression()
            {
                IsCompressed = false,
                CompressionType = null,
                FileNameExclusionList = new List<string>()
            };

            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                OverwriteDataFile = false,
                TargetFileName = "",
                CreateCurrentFile = false,
                IsRegexSearch = true,
                SearchCriteria = "\\.",
                CompressionOptions = compression
            };
            RetrieverJob rj = new RetrieverJob()
            {
                TimeZone = "Central Standard Time",
                RelativeUri = null,
                DataSource = dataSource,
                DatasetConfig = dfc,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,

                JobOptions = rjo,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
            };

            if (dataSource.Is<S3Basic>())
            {
                rj.Schedule = "*/1 * * * *";
            }
            else if (dataSource.Is<DfsBasic>())
            {
                rj.Schedule = "Instant";
            }
            else
            {
                throw new NotImplementedException("This method does not support this type of Data Source");
            }



            return rj;
        }

    }

}