using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DatasetFolderQueries
    {
        public static IQueryable<DatasetFolder> WhereIsRoot(this IQueryable<DatasetFolder> source)
        {
            return source.Where(((c) => c.ParentFolder == null)).OrderBy(((c) => c.Name));
        }

        public static IEnumerable<DatasetFolder> OrderByHierarchy(this IQueryable<DatasetFolder> source)
        {
            List<DatasetFolder> roots = source.WhereIsRoot().AsEnumerable().Union(source.Where(((s) => !source.Contains(s.ParentFolder)))).OrderBy(((c) => c.Name)).ToList();
            return GetOrderedList(roots);
        }

        private static List<DatasetFolder> GetOrderedList(this IEnumerable<DatasetFolder> dataSetFolders)
        {
            List<DatasetFolder> ordered = new List<DatasetFolder>();
            foreach (DatasetFolder dsFolder in dataSetFolders)
            {
                ordered.Add(dsFolder);
                if (dsFolder.SubFolders != null)
                {
                    ordered.AddRange(GetOrderedList(dsFolder.SubFolders)); 
                }
            }
            return ordered;
        }
    }
}
