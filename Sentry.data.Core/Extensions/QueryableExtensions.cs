using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// This code was inspired from the following:
// http://stackoverflow.com/questions/4809948/how-do-i-wrap-linq2nhibernates-fetch-and-thenfetch-inside-my-abstract-reposit


/// <summary>

/// ''' Provides support for various ways of eager fetching data, without taking a dependency on nHibernate

/// ''' </summary>

/// ''' <remarks></remarks>
public static class QueryableExtensions
{
    private static IQueryableExtensionProvider _queryableExtensionProvider = new NullQueryableExtensionProvider();

    public static System.Collections.Generic.IEnumerable<TQueried> ToFuture<TQueried, TFetch>(this IFetchRequest<TQueried, TFetch> source)
    {
        return _queryableExtensionProvider.ToFuture(source);
    }

    public static System.Collections.Generic.IEnumerable<T> ToFuture<T>(this System.Linq.IQueryable<T> source) where T : class
    {
        return _queryableExtensionProvider.ToFuture(source);
    }

    public static IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(this System.Linq.IQueryable<TOriginating> source, Expression<Func<TOriginating, TRelated>> relatedObjectSelector)
    {
        return _queryableExtensionProvider.Fetch(source, relatedObjectSelector);
    }

    public static IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(this System.Linq.IQueryable<TOriginating> source, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector)
    {
        return _queryableExtensionProvider.FetchMany(source, relatedObjectSelector);
    }

    public static IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(this IFetchRequest<TQueried, TFetch> source, Expression<Func<TFetch, TRelated>> relatedObjectSelector)
    {
        return _queryableExtensionProvider.ThenFetch(source, relatedObjectSelector);
    }

    public static IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(this IFetchRequest<TQueried, TFetch> source, Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector)
    {
        return _queryableExtensionProvider.ThenFetchMany(source, relatedObjectSelector);
    }

    public interface IQueryableExtensionProvider
    {
        IEnumerable<T> ToFuture<T>(IQueryable<T> source) where T : class;
        IEnumerable<TQueried> ToFuture<TQueried, TFetch>(IFetchRequest<TQueried, TFetch> source);
        IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(System.Linq.IQueryable<TOriginating> source, Expression<Func<TOriginating, TRelated>> relatedObjectSelector);
        IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(System.Linq.IQueryable<TOriginating> source, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector);
        IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> source, Expression<Func<TFetch, TRelated>> relatedObjectSelector);
        IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> source, Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector);
    }


    public static void SetQueryableExtensionProvider(IQueryableExtensionProvider queryableExtensionProvider)
    {
        _queryableExtensionProvider = queryableExtensionProvider;
    }

    public interface IFetchRequest<TQueried, TFetch> : IOrderedQueryable<TQueried>
    {
    }

    public class NullQueryableExtensionProvider : IQueryableExtensionProvider
    {
        public System.Collections.Generic.IEnumerable<T> ToFuture<T>(System.Linq.IQueryable<T> source) where T : class
        {
            return source;
        }

        public System.Collections.Generic.IEnumerable<TQueried> ToFuture<TQueried, TFetch>(IFetchRequest<TQueried, TFetch> source)
        {
            return source;
        }

        public IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(System.Linq.IQueryable<TOriginating> source, System.Linq.Expressions.Expression<System.Func<TOriginating, TRelated>> relatedObjectSelector)
        {
            return new NullFetchRequest<TOriginating, TRelated>(source);
        }

        public IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(System.Linq.IQueryable<TOriginating> source, System.Linq.Expressions.Expression<System.Func<TOriginating, System.Collections.Generic.IEnumerable<TRelated>>> relatedObjectSelector)
        {
            return new NullFetchRequest<TOriginating, TRelated>(source);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> source, System.Linq.Expressions.Expression<System.Func<TFetch, TRelated>> relatedObjectSelector)
        {
            return new NullFetchRequest<TQueried, TRelated>(source);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> source, System.Linq.Expressions.Expression<System.Func<TFetch, System.Collections.Generic.IEnumerable<TRelated>>> relatedObjectSelector)
        {
            return new NullFetchRequest<TQueried, TRelated>(source);
        }
    }

    public class NullFetchRequest<TQueried, TFetch> : IFetchRequest<TQueried, TFetch>
    {
        private IQueryable<TQueried> _source;
        public NullFetchRequest(System.Linq.IQueryable<TQueried> source)
        {
            _source = source;
        }


        public System.Collections.Generic.IEnumerator<TQueried> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        public System.Collections.IEnumerator GetEnumerator1()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        public System.Type ElementType
        {
            get
            {
                return _source.ElementType;
            }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                return _source.Expression;
            }
        }

        public System.Linq.IQueryProvider Provider
        {
            get
            {
                return _source.Provider;
            }
        }
    }
}
