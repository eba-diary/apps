using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class CategoryQueries
    {
        public static IQueryable<Category> WhereIsRoot(this IQueryable<Category> source)
        {
            return source.Where(((c) => c.ParentCategory == null)).OrderBy(((c) => c.Name));
        }

        public static IEnumerable<Category> OrderByHierarchy(this IQueryable<Category> source)
        {
            List<Category> roots = source.WhereIsRoot().AsEnumerable().Union(source.Where(((s) => !source.Contains(s.ParentCategory)))).OrderBy(((c) => c.Name)).ToList();
            return GetOrderedList(roots);
        }

        private static List<Category> GetOrderedList(this IEnumerable<Category> categories)
        {
            List<Category> ordered = new List<Category>();
            foreach (Category category in categories)
            {
                ordered.Add(category);
                if (category.SubCategories != null)
                {
                    ordered.AddRange(GetOrderedList(category.SubCategories)); 
                }
            }
            return ordered;
        }
    }
}
