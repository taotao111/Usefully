using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Code.External.Engine.Sqlite
{
    public static partial class Extensions
    {

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
        public static string FormatString(this string source, params object[] args)
        {
            return string.Format(source, args);
        }
        #region MyRegion
        public static string SubstringStartsOfAny(this string source, params string[] value)
        {
            return StartsOfAny(source, System.StringComparison.CurrentCulture, true, value);
        }
        public static string SubstringStartsOfAny(this string source, System.StringComparison comparison, params string[] value)
        {
            return StartsOfAny(source, comparison, true, value);
        }
        public static string LastSubstringStartsOfAny(this string source, params string[] value)
        {
            return StartsOfAny(source, System.StringComparison.CurrentCulture, false, value);
        }
        /// <summary>
        /// filename: LastSubstringStartsWithAny("/","\\")
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparison"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LastSubstringStartsOfAny(this string source, System.StringComparison comparison, params string[] value)
        {
            return StartsOfAny(source, comparison, false, value);
        }
        private static string StartsOfAny(this string source, System.StringComparison comparison, bool first, params string[] value)
        {
            if (source == null)
                return null;
            int index = -1;
            int strIndex = 0;
            for (int i = 0, n; i < value.Length; i++)
            {
                if (first)
                    n = source.IndexOf(value[i], comparison);
                else
                    n = source.LastIndexOf(value[i], comparison);
                if (n < 0)
                    continue;
                if (n > index)
                {
                    index = n;
                    strIndex = i;
                }
            }
            if (index < 0)
                return source;

            return source.Substring(index + value[strIndex].Length);
        }
        public static string SubstringStartsOfAny(this string source, params char[] value)
        {
            return StartsWithAny(source, System.StringComparison.CurrentCulture, true, value);
        }
        public static string SubstringStartsOfAny(this string source, System.StringComparison comparison, params char[] value)
        {
            return StartsWithAny(source, comparison, true, value);
        }
        public static string LastSubstringStartsOfAny(this string source, params char[] value)
        {
            return StartsWithAny(source, System.StringComparison.CurrentCulture, false, value);
        }
        public static string LastSubstringStartsOfAny(this string source, System.StringComparison comparison, params char[] value)
        {
            return StartsWithAny(source, comparison, false, value);
        }
        private static string StartsWithAny(this string source, System.StringComparison comparison, bool first, params char[] value)
        {
            if (source == null)
                return null;
            int index = -1;
            int strIndex = 0;

            if (comparison == StringComparison.CurrentCultureIgnoreCase ||
                comparison == StringComparison.InvariantCultureIgnoreCase ||
                comparison == StringComparison.OrdinalIgnoreCase)
            {
                source = source.ToLower();
                for (int i = 0; i < value.Length; i++)
                    value[i] = (value[i] + "").ToLower()[0];
            }

            for (int i = 0, n; i < value.Length; i++)
            {
                if (first)
                    n = source.IndexOf(value[i]);
                else
                    n = source.LastIndexOf(value[i]);
                if (n < 0)
                    continue;
                if (n > index)
                {
                    index = n;
                    strIndex = i;
                }
            }
            if (index < 0)
                return source;

            return source.Substring(index + 1);
        }
        public static string SubstringEndsOfAny(this string source, params string[] value)
        {
            return EndsWithAny(source, System.StringComparison.CurrentCulture, true, value);
        }
        public static string SubstringEndsOfAny(this string source, System.StringComparison comparison, params string[] value)
        {
            return EndsWithAny(source, comparison, true, value);
        }
        /// <summary>
        /// file no extension name:LastEndsWithAny(".")
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparison"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LastSubstringEndsOfAny(this string source, params string[] value)
        {
            return EndsWithAny(source, System.StringComparison.CurrentCulture, false, value);
        }
        public static string LastSubstringEndsOfAny(this string source, System.StringComparison comparison, params string[] value)
        {
            return EndsWithAny(source, comparison, false, value);
        }
        private static string EndsWithAny(this string source, System.StringComparison comparison, bool first, params string[] value)
        {
            if (source == null)
                return null;
            int index = -1;

            for (int i = 0, n; i < value.Length; i++)
            {
                if (first)
                    n = source.IndexOf(value[i], comparison);
                else
                    n = source.LastIndexOf(value[i], comparison);
                if (n < 0)
                    continue;
                if (n > index)
                    index = n;
            }
            if (index < 0)
                return source;

            return source.Substring(0, index);

        }
        public static string SubstringEndsOfAny(this string source, params char[] value)
        {
            return EndsWithAny(source, System.StringComparison.CurrentCulture, true, value);
        }
        public static string SubstringEndsOfAny(this string source, System.StringComparison comparison, params char[] value)
        {
            return EndsWithAny(source, comparison, true, value);
        }
        public static string LastSubstringEndsOfAny(this string source, params char[] value)
        {
            return EndsWithAny(source, System.StringComparison.CurrentCulture, false, value);
        }
        public static string LastSubstringEndsOfAny(this string source, System.StringComparison comparison, params char[] value)
        {
            return EndsWithAny(source, comparison, false, value);
        }
        private static string EndsWithAny(this string source, System.StringComparison comparison, bool first, params char[] value)
        {
            if (source == null)
                return null;
            int index = -1;
            if (comparison == StringComparison.CurrentCultureIgnoreCase ||
  comparison == StringComparison.InvariantCultureIgnoreCase ||
  comparison == StringComparison.OrdinalIgnoreCase)
            {
                source = source.ToLower();
                for (int i = 0; i < value.Length; i++)
                    value[i] = (value[i] + "").ToLower()[0];
            }

            for (int i = 0, n; i < value.Length; i++)
            {
                if (first)
                    n = source.IndexOf(value[i]);
                else
                    n = source.LastIndexOf(value[i]);
                if (n < 0)
                    continue;
                if (n > index)
                    index = n;
            }
            if (index < 0)
                return source;

            return source.Substring(0, index);

        }


        #endregion



    }
}
