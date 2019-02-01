using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Sentry.data.Web.Helpers
{
    //http://www.albahari.com/nutshell/predicatebuilder.aspx
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        //public static int AmountContains(this BaseEntityModel haystack, params string[] needles)
        //{
        //    int i = 0;
        //    foreach (string needle in needles)
        //    {
        //        if (haystack.DatasetDesc.ToLower().Contains(needle))
        //            i++;
        //        if (haystack.Category.ToLower().Contains(needle))
        //            i++;
        //        if (haystack.DatasetName.ToLower().Contains(needle))
        //            i++;
        //        if (haystack.SentryOwner.FullName.ToLower().Contains(needle))
        //            i++;
        //        if (haystack.SentryOwnerName.ToLower().Contains(needle))
        //            i++;
        //    }

        //    return i;
        //}
    }


}