using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Web.Controllers;

namespace Sentry.data.Web.Helpers
{
    public class Search
    {

        public ListDatasetModel List(DatasetController dsc, ListDatasetModel ldm = null, string searchPhrase = null, string category = null, string ids = null)
        {
            var list = dsc.GetDatasetModelList();
            var filteredList = list;

            List<string> searchWords = new List<string>();

            if (searchPhrase != null)
            {
                searchWords = searchPhrase.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else if(ldm != null && ldm.SearchText != null)
            {
                searchWords = ldm.SearchText.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (searchWords.Any())
            {
                filteredList =
                    filteredList.Where(x =>
                        ((x.Category.ToLower() + " " +
                          x.DatasetDesc.ToLower() + " " +
                          x.DatasetName.ToLower() + " " +
                          x.SentryOwnerName.ToLower() + " " +
                          x.CreationFreqDesc.ToLower() + " ") +
                          ((x.Columns != null && x.Columns.Count > 0) ?
                              x.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ") +
                          ((x.Metadata != null && x.Metadata.Count > 0) ?
                              x.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " "))
                        .Split(new Char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(xi => searchWords.Where(s => xi.Contains(s)).Count() > 0)
                    ).ToList();
            }

            if (ldm == null)
            {
                ldm = new ListDatasetModel();

                if (ids != null)
                {
                    string[] idsArray = ids.Split(',').ToArray();

                    ldm.SearchFilters = GetDatasetFilters(dsc, ldm, category, idsArray);
                }
            }



            var cat = ldm.SearchFilters.Where(f => f.FilterType == "Category").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
            if (cat.Any() || category != null)
            {
                if(category != null)
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


            var freq = ldm.SearchFilters.Where(f => f.FilterType == "Frequency").SelectMany(fi => fi.FilterNameList).Where(fil => fil.isChecked == true).ToList();
            if (freq.Any())
            {
                filteredList =
                (
                    from item in filteredList
                    join f in freq on item.CreationFreqDesc equals f.value
                    select item
                ).ToList();
            }


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

            ldm.DatasetList = filteredList;
            ldm.SearchFilters = GetDatasetFilters(dsc, ldm, category);

            return ldm;
        }

        private List<BaseDatasetModel> Filter(string searchPhrase, List<BaseDatasetModel> dsList)
        {
            IList<string> searchWords = searchPhrase.Trim().ToLower().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            List<BaseDatasetModel> rspList =

                dsList.Where(x =>
                    ((x.Category.ToLower() + " " +
                      x.DatasetDesc.ToLower() + " " +
                      x.DatasetName.ToLower() + " " +
                      x.SentryOwnerName.ToLower() + " " +
                      x.CreationFreqDesc.ToLower() + " ") +
                      ((x.Columns != null && x.Columns.Count > 0) ?
                          x.Columns.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " ") +
                      ((x.Metadata != null && x.Metadata.Count > 0) ?
                          x.Metadata.Select((m) => m.Name + " " + m.Value).Aggregate((c, n) => c + " " + n) + " " : " "))
                    .Split(new Char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(xi => searchWords.Where(s => xi.Contains(s)).Count() > 0)
                ).ToList();


            return rspList;
        }

        private IList<FilterModel> GetDatasetFilters(DatasetController dsc, ListDatasetModel ldm, string cat = null, string[] ids = null)
        {
            IList<FilterModel> FilterList = new List<FilterModel>();
            IList<FilterNameModel> FilterNames = new List<FilterNameModel>();

            //Generate Category Filers
            FilterModel Filter = new FilterModel();
            Filter.FilterType = "Category";

            IDictionary<int, string> enumList = EnumToDictionary(typeof(DatasetFrequency));
            IList<FilterNameModel> fList = new List<FilterNameModel>();

            int filterIndex = 0;

            foreach(string category in dsc._datasetContext.Categories.Select(x => x.Name).ToList())
            {
                FilterNameModel nf = new FilterNameModel();
                nf.id = filterIndex;
                nf.value = category;

                Boolean hasCategoryID = false;

                if (ids != null)
                {
                    foreach (string id in ids)
                    {
                        if (id.StartsWith("0") && id.EndsWith(nf.id.ToString()))
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
            FilterList.Add(Filter);

            //Generate SentryOwner Filers
            Filter = new FilterModel();
            Filter.FilterType = "Sentry Owner";

            fList = new List<FilterNameModel>();

            foreach (string owner in dsc._datasetContext.GetSentryOwnerList().ToList())
            {
                FilterNameModel nf = new FilterNameModel();
                nf.id = filterIndex;
                nf.value = dsc._associateInfoProvider.GetAssociateInfo(owner).FullName;

                Boolean hasCategoryID = false;

                if (ids != null)
                {
                    foreach (string id in ids)
                    {
                        if (id.StartsWith("1") && id.EndsWith(nf.id.ToString()))
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
                //if (nf.count > 0)
                //{
                fList.Add(nf);
                //}

                filterIndex++;
            }
            Filter.FilterNameList = fList.ToList();
            FilterList.Add(Filter);


            //Generate Frequency Filters
            Filter = new FilterModel();
            Filter.FilterType = "Frequency";

            fList = new List<FilterNameModel>();

            foreach (var item in enumList)
            {
                FilterNameModel nf = new FilterNameModel();
                nf.id = item.Key;
                nf.value = item.Value;

                Boolean hasCategoryID = false;

                if (ids != null)
                {
                    foreach (string id in ids)
                    {
                        if (id.StartsWith("2") && id.EndsWith(nf.id.ToString()))
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
                            Where(f => f.FilterType == "Frequency").
                            SelectMany(fi => fi.FilterNameList).
                            Any(fil => fil.isChecked == true)
                        &&
                        ldm.SearchFilters.
                            Where(f => f.FilterType == "Frequency").
                            SelectMany(fi => fi.FilterNameList).
                            Any(fil => fil.value == item.Value && fil.isChecked == true)
                    )
                    ||
                        hasCategoryID
                    )
                {
                    nf.isChecked = true;
                }

                //Count of all datasets equal to this filter
                nf.count = ldm.DatasetList.Where(f => f.CreationFreqDesc == nf.value).Count();

                // Only add filter if there are datasets associated based on filtering 
                //if (nf.count > 0)
                //{
                fList.Add(nf);
                //}
            }
            Filter.FilterNameList = fList.ToList();
            FilterList.Add(Filter);

            return FilterList;
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