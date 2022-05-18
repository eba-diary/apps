using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Helpers
{
    public static class Utility
    {


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
                result = string.Format("{0} seconds ago", String.Format("{0:0}",span.TotalSeconds));
            }
            else
            {
                result = "just now";
            }

            return result;
        }

        [Obsolete("The function is fine but I wanted that this should not use the domain context and these should all be Enums")]
        public static void SetupLists(IDatasetContext _datasetContext, DatasetModel model)
        {
            //Categories
            var temp = GetCategoryList(_datasetContext).ToList();

            if(model.DatasetCategoryIds?.Count == 1 && model.DatasetCategoryIds[0] != 0)
            {
                temp.First(x => x.Value == model.DatasetCategoryIds.First().ToString()).Selected = true;
            }
            else
            {
                temp.Add(new SelectListItem()
                {
                    Text = "Pick a Category",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

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

            model.AllDataFileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() }).ToList();

            model.AllDataClassifications = BuildDataClassificationSelectList(model.DataClassification);

            //File Extension
            IEnumerable<SelectListItem> dFileExtensions;
            if (model.FileExtensionId == 0)
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
                        Selected = c.Id == model.FileExtensionId,
                        Text = c.Name.Trim(),
                        Value = c.Id.ToString()
                    });
            }
            model.AllExtensions = dFileExtensions.OrderByDescending(x => x.Selected);
        }

        public static IEnumerable<SelectListItem> BuildPreProcessingOptionsDropdown(int preProcessingOptionSelections)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (preProcessingOptionSelections == 0)
            {
                items.Add(new SelectListItem { Text = "Select Option", Value = "0", Selected = true});
                items.AddRange(Enum.GetValues(typeof(DataFlowPreProcessingTypes)).Cast<DataFlowPreProcessingTypes>().Select(v => new SelectListItem { Text = v.GetDescription(), Value = ((int)v).ToString() }).ToList());
            }
            else
            {
                items.AddRange(Enum.GetValues(typeof(DataFlowPreProcessingTypes)).Cast<DataFlowPreProcessingTypes>().Select(v =>
                    new SelectListItem
                    {
                        Selected = ((int)v == preProcessingOptionSelections),
                        Text = v.GetDescription(),
                        Value = ((int)v).ToString()
                    }).ToList());
            }
            return items;
        }

        public static IEnumerable<SelectListItem> BuildFileTypeDropdown(int fileTypeSelection)
        {
            List<SelectListItem> items = Enum.GetValues(typeof(FileType)).Cast<FileType>().Select(v =>
            new SelectListItem
            {
                Selected = ((int)(object)v).ToString() == fileTypeSelection.ToString(),
                Text = (v.GetDescription() == FileType.DataFile.GetDescription()) ? $"{v.GetDescription()} (most common)" : v.GetDescription(),
                Value = ((int)(object)v).ToString()
            }).ToList();

            return items.OrderBy(o => o.Text);
        }

        public static IEnumerable<SelectListItem> BuildIngestionTypeDropdown(int ingestionTypeSelection)
        {
            List<SelectListItem> items = Enum.GetValues(typeof(IngestionType)).Cast<IngestionType>().Select(v =>
                  new SelectListItem
                  {
                      Selected = (((int)(object)v).ToString() == ingestionTypeSelection.ToString()),
                      Text = v.GetDescription(),
                      Value = ((int)(object)v).ToString()
                  }).ToList();

            if (ingestionTypeSelection == 0)
            {
                items.Add(new SelectListItem
                {
                    Selected = true,
                    Text = "Select Ingestion Type",
                    Value = "0"
                });
            } 

            return items.OrderBy(o => o.Value);
        }

        public static IEnumerable<SelectListItem> GetCategoryList(IDatasetContext _datasetContext)
        {
            IEnumerable<SelectListItem> var = _datasetContext.Categories.Where(w => w.ObjectType == GlobalConstants.DataEntityCodes.DATASET).Select((c) => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });

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
                        Selected = c.Name.Contains(GlobalConstants.ExtensionNames.ANY) ? true : false,
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
                ObjectStatus = ObjectStatusEnum.Active
            };

            if(dataSource.Is<S3Basic>())
            {
                rj.Schedule = "*/1 * * * *";
            }
            else if(dataSource.Is<DfsBasic>())
            {
                rj.Schedule = "Instant";
            }
            else
            {
                throw new NotImplementedException("This method does not support this type of Data Source");
            }

            return rj;
        }
        public static List<SelectListItem> BuildDatasetSortByOptions()
        {
            List<SelectListItem> sortOptions = new List<SelectListItem>();

            foreach(DatasetSortByOption item in Enum.GetValues(typeof(DatasetSortByOption)))
            {
                sortOptions.Add(new SelectListItem
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString()
                });
            }

            return sortOptions;
        }

        public static List<SelectListItem> BuildSelectListitem(List<KeyValuePair<string,string>> list, string defaultText)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(defaultText))
            {
                items.Add(new SelectListItem() { Text = defaultText, Value = "", Selected = true });
            }
            else if (defaultText != null && defaultText == "")
            {
                items.Add(new SelectListItem() { Text = "", Value = ""});
            }

            list.ForEach(x => items.Add(new SelectListItem() { Text = x.Value, Value = x.Key }));

            return items;
        }

        private static List<SelectListItem> BuildDataClassificationSelectList(DataClassificationType selectedType)
        {
            List<SelectListItem> classifications = new List<SelectListItem>();

            if (selectedType == DataClassificationType.None)
            {
                classifications.Add(new SelectListItem()
                {
                    Text = "Pick a data classification",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            foreach(DataClassificationType item in Enum.GetValues(typeof(DataClassificationType)))
            {
                classifications.Add(new SelectListItem()
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString(),
                    Selected = selectedType == item
                });
            }

            return classifications;
        }

        public static string FormatCategoryList(List<string> Categories)
        {
            if(Categories != null)
            {
                StringBuilder catNames = new StringBuilder();
                bool firstCat = true;
                foreach (string item in Categories)
                {
                    if (firstCat)
                    {
                        catNames.Append(item);
                        firstCat = false;
                    }
                    else
                    {
                        catNames.Append(", " + item);
                    }
                }
                return catNames.ToString();
            }
            else
            {
                return "No Category Assigned";
            }            
        }

        public static IEnumerable<SelectListItem> BuildRequestMethodDropdown(HttpMethods requestMethod)
        {
            List<SelectListItem> requestMethodList = new List<SelectListItem>();

            if (requestMethod == HttpMethods.none) {
                requestMethodList.Add(new SelectListItem()
                {
                    Text = "Pick a Method",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            foreach(HttpMethods item in Enum.GetValues(typeof(HttpMethods)))
            {
                requestMethodList.Add(new SelectListItem()
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString(),
                    Selected = requestMethod == item
                });
            }

            return requestMethodList;
        }

        public static IEnumerable<SelectListItem> BuildRequestDataFormatDropdown(HttpDataFormat requestDataMethod)
        {
            List<SelectListItem> requestDataMethodList = new List<SelectListItem>();

            if (requestDataMethod == HttpDataFormat.none)
            {
                requestDataMethodList.Add(new SelectListItem()
                {
                    Text = "Pick a Data Format",
                    Value = "0",
                    Selected = true,
                    Disabled = true
                });
            }

            foreach (HttpDataFormat item in Enum.GetValues(typeof(HttpDataFormat)))
            {
                requestDataMethodList.Add(new SelectListItem()
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString(),
                    Selected = requestDataMethod == item
                });
            }

            return requestDataMethodList;
        }

        public static List<SelectListItem> BuildFtpPatternSelectList(FtpPattern selectedType)
        {
            List<SelectListItem> patterns = new List<SelectListItem>();

            if (selectedType == FtpPattern.None)
            {
                patterns.Add(new SelectListItem()
                {
                    Text = "Pick a ftp pattern",
                    Value = ((int)FtpPattern.None).ToString(),
                    Selected = true,
                    Disabled = false
                });
            }

            foreach (FtpPattern item in Enum.GetValues(typeof(FtpPattern)))
            {
                if (item == FtpPattern.None) 
                { 
                    continue; 
                }
                patterns.Add(new SelectListItem()
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString(),
                    Selected = selectedType == item
                });              
            }

            return patterns;
        }

        public static IEnumerable<SelectListItem> BuildCompressionDropdown(bool isCompressed)
        {
            List<SelectListItem> compressionList = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Text = "Yes",
                    Value = "true",
                    Selected = isCompressed == true
                },

                new SelectListItem()
                {
                    Text = "No",
                    Value = "false",
                    Selected = isCompressed == false
                }
            };

            return compressionList;
        }

        public static IEnumerable<SelectListItem> BuildCompressionTypesDropdown(string selectedType)
        {
            if (string.IsNullOrEmpty(selectedType))
            {
                //Setting selectedType to bogus value to not repeat list building logic
                selectedType = "NoSelection";
            }
            
                IEnumerable<SelectListItem> list = Enum.GetValues(typeof(CompressionTypes)).Cast<CompressionTypes>().Select(v
                => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString(), Selected = (v.ToString() == selectedType) }).ToList();
            
            return list;
        }

        public static IEnumerable<SelectListItem> BuildPreProcessingDropdown(bool isPreProcessingRequired)
        {
            List<SelectListItem> preProcessingRequired = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Text = "Yes",
                    Value = "true",
                    Selected = isPreProcessingRequired == true
                },

                new SelectListItem()
                {
                    Text = "No",
                    Value = "false",
                    Selected = isPreProcessingRequired == false
                }
            };

            return preProcessingRequired;
        }

        internal static IEnumerable<SelectListItem> BuildSchedulePickerDropdown(string schedule)
        {
            List<SelectListItem> ScheduleOptions = new List<SelectListItem>();

            ScheduleOptions.Add(new SelectListItem()
            {
                Text = "Pick a Schedule",
                Value = "0",
                Selected = (String.IsNullOrEmpty(schedule) || schedule == "0"),
                Disabled = true
            });

            foreach (RetrieverJobScheduleTypes item in Enum.GetValues(typeof(RetrieverJobScheduleTypes)))
            {
                ScheduleOptions.Add(new SelectListItem()
                {
                    Text = item.GetDescription(),
                    Value = ((int)item).ToString(),
                    Selected = schedule == item.GetDescription()
                });
            }

            return ScheduleOptions;
        }
    }
}