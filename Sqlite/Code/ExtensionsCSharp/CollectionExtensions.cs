using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Code.External.Engine.Sqlite
{
    public static partial class Extensions
    {
        public static bool HasElement(this ICollection source)
        {
            return source != null && source.Count > 0;
        }
        public static void RemoveNullOrEmpty(this IList<string> source)
        {
            if (source == null)
                return;

            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(source[i]))
                    source.RemoveAt(i);
            }

        }
        public static void RemoveNull<T>(this IList<T> source)
            where T : class
        {

            if (source == null || source.Count <= 0)
                return;

            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i] == null)
                    source.RemoveAt(i);
            }
        }
        public static string[] RemoveNullOrEmpty(this string[] source)
        {

            if (source == null)
                return new string[0];

            List<string> result = new List<string>(source.Length);
            foreach (var item in source)
            {
                if (!string.IsNullOrEmpty(item))
                    result.Add(item);
            }
            return result.ToArray();
        }
        public static T[] RemoveNull<T>(this T[] source)
          where T : class
        {

            if (source == null || source.Length <= 0)
                return new T[0];

            List<T> result = new List<T>(source.Length);
            foreach (var item in source)
            {
                if (item != null)
                    result.Add(item);
            }
            return result.ToArray();
        }
        public static T[] ToArrayOf<T>(this IEnumerable source)
        {
            List<T> result = new List<T>();
            foreach (var o in source)
                result.Add((T)o);
            return result.ToArray();
        }
        public static IEnumerable<TSource> Fill<TSource, TKey>(this IEnumerable<TSource> source, TKey[] fill, Func<TSource, TKey> keySelector)
        {
            List<TSource> tmp = new List<TSource>(new TSource[fill.Length]);
            int idLength = fill.Length;
            TKey itemID;
            foreach (TSource item in source)
            {
                itemID = keySelector(item);
                for (int i = 0; i < idLength; i++)
                {
                    if (object.Equals(fill[i], itemID))
                        tmp[i] = item;
                }
            }
            return tmp;
        }
        #region Unity ios compatibility

        /// <summary>
        /// ios compatibility
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        class OrderedEnumerableHelper<TResult, TKey> : IOrderedEnumerable<TResult>
        {
            public IEnumerable<TResult> source;

            private Func<TResult, TKey> keySelector;
            private IComparer<TKey> comparer;
            private bool descending;


            public OrderedEnumerableHelper(IEnumerable<TResult> source)
            {
                this.source = source;
            }

            public IEnumerable<TResult> Source
            {
                get { return source; }
            }


            #region IOrderedEnumerable[System.Int32] implementation
            public IOrderedEnumerable<TResult> CreateOrderedEnumerable<TKey>(Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool @descending)
            {
                var order = new OrderedEnumerableHelper<TResult, TKey>(source);
                order.keySelector = keySelector;
                order.comparer = comparer;
                order.descending = @descending;
                return order;
            }
            #endregion

            #region IEnumerable[System.Int32] implementation
            public IEnumerator<TResult> GetEnumerator()
            {
                TResult[] source = this.source.ToArray();
                if (source.Length < 2)
                {
                    if (source.Length == 1)
                        yield return source[0];
                    yield break;
                }
                int i, j, t;
                int n = source.Length;
                TKey[] keys = new TKey[n];

                TResult tmp;
                TKey tmpKey;

                for (i = 0; i < n; i++)
                {
                    keys[i] = keySelector(source[i]);
                }


                if (descending)
                {
                    for (i = 0; i < n - 1; i++)
                    {
                        for (j = 0; j < n - i - 1; j++)
                        {

                            if (comparer.Compare(keys[j + 1], keys[j]) < 0)
                            {
                                tmp = source[j + 1];
                                source[j + 1] = source[j];
                                source[j] = tmp;

                                tmpKey = keys[j + 1];
                                keys[j + 1] = keys[j];
                                keys[j] = tmpKey;

                            }
                        }
                        yield return source[n - i - 1];
                    }
                }
                else
                {
                    for (i = 0; i < n - 1; i++)
                    {
                        for (j = 0; j < n - i - 1; j++)
                        {
                            if (comparer.Compare(keys[j + 1], keys[j]) > 0)
                            {
                                tmp = source[j + 1];
                                source[j + 1] = source[j];
                                source[j] = tmp;

                                tmpKey = keys[j + 1];
                                keys[j + 1] = keys[j];
                                keys[j] = tmpKey;

                            }
                        }
                        yield return source[n - i - 1];
                    }
                }

                yield return source[0];

            }
            #endregion

            #region IEnumerable implementation
            IEnumerator IEnumerable.GetEnumerator()
            {

                return this.GetEnumerator();
            }
            #endregion


        }
        public static IOrderedEnumerable<TResult> OrderByUnity<TResult, TKey>(this IEnumerable<TResult> source, Func<TResult, TKey> keySelector)
        {
            var o = new OrderedEnumerableHelper<TResult, TKey>(source);
            return o.CreateOrderedEnumerable(keySelector, Comparer<TKey>.Default, false);
        }
        public static IOrderedEnumerable<TResult> OrderByDescendingUnity<TResult, TKey>(this IEnumerable<TResult> source, Func<TResult, TKey> keySelector)
        {
            var o = new OrderedEnumerableHelper<TResult, TKey>(source);
            return o.CreateOrderedEnumerable(keySelector, Comparer<TKey>.Default, true);
        }
        public static T FirstOrDefaultUnity<T>(this T[] source)
        {

            if (source == null || source.Length <= 0)
                return default(T);
            return source[0];
        }

        public static T FirstOrDefaultUnity<T>(this IEnumerable<T> source)
        {

            if (source == null)
                return default(T);
            T ret = default(T);
            foreach (var i in source)
            {
                ret = i;
                break;
            }
            return ret;
        }
        #endregion
        public static void Swap<T>(this T[] source, int index1, int index2)
        {
            T tmp = source[index1];
            source[index1] = source[index2];
            source[index2] = tmp;
        }
        public static int IndexOf<T>(this T[] source, T value)
        {

            for (int i = 0, len = source.Length; i < len; i++)
                if (object.Equals(source[i], value))
                    return i;

            return -1;
        }
        public static int IndexOf<T>(this T[] source, Func<T, bool> equals)
        {
            for (int i = 0, len = source.Length; i < len; i++)
                if (equals(source[i]))
                    return i;

            return -1;
        }
    }
}
