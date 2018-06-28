using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.Text;
using System.Web.Mvc;

namespace Sentry.data.Web.Helpers
{
    public static class Utility
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

            //if (span.Days > 365)
            //{
            //    int years = (span.Days / 365);
            //    result = string.Format("{0} {1} ago", years, years == 1 ? "year" : "years");
            //}
            //else if (span.Days > 30)
            //{
            //    int months = (span.Days / 30);
            //    result = string.Format("{0} {1} ago", months, months == 1 ? "month" : "months");
            //}
            if (span.TotalDays > 1)
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
                result = string.Format("{0} seconds ago", String.Format("{0:0}",span.TotalSeconds));
            }
            else
            {
                result = "just now";
            }

            return result;
        }

        public static BaseDatasetModel setupLists(IDatasetContext _datasetContext, BaseDatasetModel model)
        {
            var temp = GetCategoryList(_datasetContext).ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Category",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.AllCategories = temp.OrderBy(x => x.Value);

            //Origination Codes
            temp = GetDatasetOriginationListItems().ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick an Origination Location",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.AllOriginationCodes = temp.OrderBy(x => x.Value);

            //Dataset Frequency
            temp = GetDatasetFrequencyListItems().ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Frequency",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            model.AllFrequencies = temp.OrderBy(x => x.Value);

            //Dataset Scope
            temp = GetDatasetScopeTypesListItems(_datasetContext).ToList();

            temp.Add(new SelectListItem()
            {
                Text = "Pick a Scope",
                Value = "0",
                Selected = true,
                Disabled = true
            });
            model.AllDatasetScopeTypes = temp.OrderBy(x => x.Value);
            model.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            List<string> obj = new List<string>();
            obj.Add("Restricted");
            obj.Add("Highly Sensitive");
            obj.Add("Internal Use Only");
            obj.Add("Public");

            List<SelectListItem> dataClassifications = new List<SelectListItem>();

            dataClassifications.Add(new SelectListItem()
            {
                Text = "Pick a Classification",
                Value = "0",
                Selected = true,
                Disabled = true
            });

            int index = 1;
            foreach (String classification in obj)
            {
                dataClassifications.Add(new SelectListItem()
                {
                    Text = classification,
                    Value = index.ToString()
                });
                index++;
            }


            model.AllDataClassifications = dataClassifications;

            return model;
        }

        public static IEnumerable<SelectListItem> GetCategoryList(IDatasetContext _datasetContext)
        {
            IEnumerable<SelectListItem> var = _datasetContext.Categories.Select((c) => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

            return var;
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
                items = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();
            }
            else
            {
                items = Enum.GetValues(typeof(DatasetFrequency)).Cast<DatasetFrequency>().Select(v =>
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
                        Selected = c.Name.Contains("ANY") ? true : false,
                        Text = c.Name.Trim(),
                        Value = c.Id.ToString()
                    });
            }
            else
            {
                dFileExtensions = _datasetContext.FileExtensions
                    .Select((c) => new SelectListItem
                    {
                        Selected = c.Id == id ? true : false,
                        Text = c.Name.Trim(),
                        Value = c.Id.ToString()
                    });
            }

            return dFileExtensions.OrderByDescending(x => x.Selected);
        }


    }

}