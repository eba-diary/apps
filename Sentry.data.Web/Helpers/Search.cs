using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LazyCache;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Web.Controllers;
using StackExchange.Profiling;

namespace Sentry.data.Web.Helpers
{
    public class Search
    {
        private IAppCache _cache;
        public Search(IAppCache cache)
        {
            _cache = cache;
        }

        public ListDatasetModel List(DatasetController dsc, ListDatasetModel ldm = null, string searchPhrase = null, string category = null, string ids = null)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null

            using (profiler.Step("List"))
            {
                var list = dsc.GetDatasetModelList();
                var filteredList = list;

                List<string> searchWords = new List<string>();

                List<string> ownerList = dsc._datasetContext.GetSentryOwnerList().ToList();

                if (searchPhrase != null)
                {
                    searchWords = searchPhrase.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else if (ldm != null && ldm.SearchText != null)
                {
                    searchWords = ldm.SearchText.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                if (searchWords.Any())
                {
                    filteredList = SearchTextFilter(searchWords, filteredList);
                }

                if (ldm == null)
                {
                    ldm = new ListDatasetModel();

                    ldm.CategoryList = new List<string>();
                    ldm.DatasetList = new List<BaseDatasetModel>();
                    ldm.SentryOwnerList = new List<string>();
                    ldm.SearchFilters = new List<FilterModel>();

                    using (profiler.Step("Search Filter Creation from Null"))
                    {
                        if (ids != null)
                        {
                            string[] idsArray = ids.Split(',').ToArray();

                            ldm.SearchFilters = GetDatasetFilters(dsc, ldm, ownerList, category, idsArray);
                        }
                    }
                }
                using (profiler.Step("Filter List"))
                {
                    using (profiler.Step("Category"))
                    {
                        var cat = ldm.SearchFilters.Where(f => f.FilterType == "Category").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
                        if (cat.Any() || category != null)
                        {
                            if (category != null)
                            {
                                filteredList =
                                (
                                    from item in filteredList
                                    where item.Category == category
                                    select item
                                ).ToList();
                            }
                            else
                            {
                                filteredList =
                                (
                                    from item in filteredList
                                    join c in cat on item.Category equals c.value
                                    select item
                                ).ToList();
                            }
                        }

                    }

                    using (profiler.Step("Owner"))
                    {
                        var own = ldm.SearchFilters.Where(f => f.FilterType == "Sentry Owner").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
                        if (own.Any())
                        {
                            filteredList =
                            (
                                from item in filteredList
                                join o in own on item.SentryOwner.FullName equals o.value
                                select item
                            ).ToList();
                        }
                    }

                    using (profiler.Step("Extension"))
                    {
                        var ext = ldm.SearchFilters.Where(f => f.FilterType == "Extension")
                        .SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
                        if (ext.Any())
                        {
                            filteredList =
                            (
                                from item in filteredList
                                from file in item.DistinctFileExtensions()
                                join e in ext on file equals e.value
                                select item
                            ).ToList();
                        }
                    }

                    using (profiler.Step("Group By"))
                    {
                        ldm.DatasetList = filteredList.GroupBy(x => x.DatasetId).Select(x => x.First()).ToList();
                    }
                }

                using (profiler.Step("Populate Search Filters"))
                {
                    ldm.SearchFilters = GetDatasetFilters(dsc, ldm, ownerList, category);
                }

                return ldm;
            }
        }
       

        private List<BaseDatasetModel> SearchTextFilter(List<string> searchWords, List<BaseDatasetModel> filteredList)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null

            using (profiler.Step("Text Filtering"))
            {
                var predicate = PredicateBuilder.False<BaseDatasetModel>();





                predicate = predicate.Or(f => f.DatasetDesc.ToLower().ContainsAny(searchWords.ToArray()));
                predicate = predicate.Or(f => f.Category.ToLower().ContainsAny(searchWords.ToArray()));
                predicate = predicate.Or(f => f.DatasetName.ToLower().ContainsAny(searchWords.ToArray()));
                predicate = predicate.Or(f => f.SentryOwner.FullName.ToLower().ContainsAny(searchWords.ToArray()));
                predicate = predicate.Or(f => f.SentryOwnerName.ToLower().ContainsAny(searchWords.ToArray()));




                return filteredList
                    .Where(predicate.Compile())
                    .OrderByDescending(x => PredicateBuilder.AmountContains(x, searchWords.ToArray()))  //The more search terms you match the higher in the results a object will be.
                    .ToList();
            }

            //filteredList =
            //        filteredList.Where(x =>
            //            ((x.Category.ToLower() + " " +
            //              x.DatasetDesc.ToLower() + " " +
            //              x.DatasetName.ToLower() + " " +
            //              x.SentryOwner.FullName + " " +
            //              x.SentryOwnerName.ToLower() + " ") +
            //              ((x.Columns != null && x.Columns.Count > 0) ?
            //                  x.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ") +
            //              ((x.Metadata != null && x.Metadata.Count > 0) ?
            //                  x.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " "))
            //            .Split(new Char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
            //            .Any(xi => searchWords.Where(s => xi.Contains(s)).Count() > 0)
            //        ).ToList();
            


            //return filteredList;
        }


        private IList<FilterModel> GetDatasetFilters(DatasetController dsc, ListDatasetModel ldm, List<string> ownerList, string cat = null, string[] ids = null)
        {
            IList<FilterModel> FilterList = new List<FilterModel>();

            FilterList.Add(CategoryFilter(dsc, ldm, ids, 0, cat));
            FilterList.Add(OwnerFilter(dsc, ldm, ids, 1, ownerList));
            FilterList.Add(ExtensionFilter(dsc, ldm, ids, 2));

            return FilterList;
        }


        private FilterModel CategoryFilter(DatasetController dsc, ListDatasetModel ldm, string[] ids, int filterID, string cat = null)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null

            using (profiler.Step("Category Filter"))
            {
                //Generate Category Filers
                FilterModel Filter = new FilterModel();
                Filter.FilterType = "Category";

                IList<FilterNameModel> fList = new List<FilterNameModel>();

                int filterIndex = 0;

                List<string> categoryList = _cache.Get<List<string>>("categoryList");

                if (categoryList == null)
                {
                    categoryList = dsc._datasetContext.Categories.Select(x => x.Name).ToList();
                    _cache.Add("categoryList", categoryList);
                }

                foreach (string category in categoryList)
                {
                    FilterNameModel nf = new FilterNameModel();
                    nf.id = filterIndex;
                    nf.value = category;

                    Boolean hasCategoryID = false;

                    if (ids != null)
                    {
                        foreach (string id in ids)
                        {
                            if (id.StartsWith(filterID.ToString()) && id.Substring(id.IndexOf("_") + 1) == (nf.id.ToString()))
                            {
                                hasCategoryID = true;
                            }
                        }
                    }

                    //Match isChecked status to status on input model                
                    if (ldm.SearchFilters
                            .Where(f => f.FilterType == "Category")
                            .SelectMany(fi => fi.FilterNameList)
                            .Any(fil => fil.value == category && fil.isChecked == true) ||
                            category == cat ||
                            hasCategoryID
                        )
                    {
                        nf.isChecked = true;
                    }
                    else
                    {
                        nf.isChecked = false;
                    }


                    //Count of all datasets equal to this filter
                    nf.count = ldm.DatasetList.Count(f => f.Category == nf.value);
                    fList.Add(nf);

                    filterIndex++;

                }

                Filter.FilterNameList = fList.ToList();

                return Filter;
            }
        }


        private FilterModel OwnerFilter(DatasetController dsc, ListDatasetModel ldm, string[] ids, int filterID, List<string> ownerList)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null

            using (profiler.Step("Owner Filter"))
            {
                //Generate SentryOwner Filers
                FilterModel Filter = new FilterModel();
                Filter.FilterType = "Sentry Owner";

                List<FilterNameModel> fList = new List<FilterNameModel>();

                int filterIndex = 0;

                foreach (string owner in ownerList)
                {
                    FilterNameModel nf = new FilterNameModel();
                    nf.id = filterIndex;

                    string name = _cache.Get<string>(owner);

                    if (name == null)
                    {
                        name = dsc._associateInfoProvider.GetAssociateInfo(owner).FullName;
                        _cache.Add(owner, name);
                    }

                    nf.value = name;

                    Boolean hasCategoryID = false;

                    if (ids != null)
                    {
                        foreach (string id in ids)
                        {
                            var a = id.Substring(id.IndexOf("_") + 1);
                            var b = nf.id.ToString();

                            if (id.StartsWith(filterID.ToString()) && id.Substring(id.IndexOf("_") + 1) == (nf.id.ToString()))
                            {
                                hasCategoryID = true;
                            }

                        }
                    }

                    if (
                        (
                            ldm.SearchFilters.Any()
                            &&
                            ldm.SearchFilters.
                                Where(f => f.FilterType == "Sentry Owner").
                                SelectMany(fi => fi.FilterNameList).
                                Any(fil => fil.isChecked == true)
                            &&
                            ldm.SearchFilters.
                                Where(f => f.FilterType == "Sentry Owner").
                                SelectMany(fi => fi.FilterNameList).
                                Any(fil => fil.value == owner && fil.isChecked == true)
                        )
                        ||
                            hasCategoryID
                        )
                    {
                        nf.isChecked = true;
                    }

                    //Count of all datasets equal to this filter
                    nf.count = ldm.DatasetList.Where(f => f.SentryOwner.FullName == nf.value).Count();

                    // Only add filter if there are datasets associated based on filtering 
                    fList.Add(nf);

                    filterIndex++;
                }
                Filter.FilterNameList = fList.ToList();

                return Filter;
            }
        }

        private FilterModel ExtensionFilter(DatasetController dsc, ListDatasetModel ldm, string[] ids, int filterID)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null

            using (profiler.Step("Extension Filter"))
            {
                //Generate Frequency Filters
                FilterModel Filter = new FilterModel();
                Filter.FilterType = "Extension";

                List<FilterNameModel> fList = new List<FilterNameModel>();

                // Utilities.GetFileExtension(item.FileName)
                List<string> fileExtensionList = _cache.Get<List<string>>("fileExtensionList");

                if (fileExtensionList == null)
                {
                    fileExtensionList = new List<string>();
                    foreach (var a in dsc._datasetContext.Datasets)
                    {
                        foreach (var b in a.DatasetFiles)
                        {
                            fileExtensionList.Add(Utilities.GetFileExtension(b.FileName));
                        }
                    }
                    fileExtensionList = fileExtensionList.Distinct().ToList();
                    _cache.Add("fileExtensionList", fileExtensionList);
                }

                int filterIndex = 0;

                foreach (var item in fileExtensionList)
                {
                    FilterNameModel nf = new FilterNameModel();
                    nf.id = filterIndex;
                    nf.value = item;

                    Boolean hasCategoryID = false;

                    if (ids != null)
                    {
                        foreach (string id in ids)
                        {
                            if (id.StartsWith(filterID.ToString()) && id.Substring(id.IndexOf("_") + 1) == (nf.id.ToString()))
                            {
                                hasCategoryID = true;
                            }
                        }
                    }
                    if (
                        (
                            ldm.SearchFilters.Any()
                            &&
                            ldm.SearchFilters.
                                Where(f => f.FilterType == "Extension").
                                SelectMany(fi => fi.FilterNameList).
                                Any(fil => fil.isChecked == true)
                            &&
                            ldm.SearchFilters.
                                Where(f => f.FilterType == "Extension").
                                SelectMany(fi => fi.FilterNameList).
                                Any(fil => fil.value == item && fil.isChecked == true)
                        )
                        ||
                            hasCategoryID
                        )
                    {
                        nf.isChecked = true;
                    }

                    //Count of all datasets equal to this filter
                    nf.count = ldm.DatasetList.Where(f => f.DistinctFileExtensions().Contains(nf.value)).Count();

                    fList.Add(nf);
                    filterIndex++;
                }
                Filter.FilterNameList = fList.ToList();
                return Filter;
            }
            
        }

        private IDictionary<int, string> EnumToDictionary(Type e)
        {
            if (!(e.IsEnum))
            {
                throw new InvalidOperationException("Enum list view model must have Enum generic type constraint");
            }

            Dictionary<int, string> kvp = new Dictionary<int, string>();

            string[] namedValues = System.Enum.GetNames(e);

            foreach (var nv in namedValues)
            {
                int castValue = (int)(Enum.Parse(e, nv));
                var enumVal = System.Enum.Parse(e, nv);
                if (!(kvp.ContainsKey(castValue) && castValue > 0))
                {
                    kvp.Add(castValue, enumVal.ToString());
                }
            }

            return kvp;

        }
    }
}