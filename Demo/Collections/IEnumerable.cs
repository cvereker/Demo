using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemExtensions.Collections
{
    public static class IEnumerableExtensions
    {
        public static V[,] Pivot<S, T, U, V>(this IEnumerable<S> values,
            Func<S, T> RowSelector,
            Func<S, U> ColumnSelector,
            Func<S, V> ValueSelector,
            IEnumerable<T> rows,
            IEnumerable<U> columns,
                        out T[] rowsIndex,
            out U[] columnsIndex,
            V defaultValue = default(V))
        {
            if (rows == null)
                rows = values.Select(RowSelector);

            if (columns == null)
                columns = values.Select(ColumnSelector);

            var rows_map = rows.Distinct().OrderBy(r => r).Select((rowID, I) => new { rowID, I });
            var column_map = columns.Distinct().OrderBy(r => r).Select((columnID, J) => new { columnID, J });

            int numSeries = column_map.Count();
            int numDate = rows_map.Count();

            var dblValues = new V[numDate, numSeries];

            // set a default value - this can be useful if you need to process NULL values afterwards
            if (defaultValue.Equals(default(V)) == false)
            {
                for (int i = 0; i < numDate; i++)
                    for (int j = 0; j < numSeries; j++)
                        dblValues[i, j] = defaultValue;
            }

            foreach (var column in column_map)
            {
                //var series_values = values.Where(r => ColumnSelector(r).Equals(column.columnID));

                var q =
                    from d in rows_map
                    join v in values.Where(r => ColumnSelector(r).Equals(column.columnID))
                    on d.rowID equals RowSelector(v)
                    select new { d.I, column.J, value = ValueSelector(v) };

                q.AsParallel().ToList().ForEach(a => dblValues[a.I, a.J] = a.value);
            }

            columnsIndex = column_map.Select(r => r.columnID).ToArray();
            rowsIndex = rows_map.Select(r => r.rowID).ToArray();
            return dblValues;
        }

        public static IEnumerable<double> Divide(this IEnumerable<double> me, double value)
        {
            foreach (double d in me)
                yield return d / value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <example>var query = list.SelectWithPrevious((prev, cur) =>new { ID = cur.ID, Date = cur.Date, DateDiff = (cur.Date - prev.Date).Days);</example>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="projection"></param>
        /// <seealso cref="http://stackoverflow.com/questions/3683105/calculate-difference-from-previous-item-with-linq/3683217#3683217"/>
        /// <returns></returns>
        public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(previous, iterator.Current);
                    previous = iterator.Current;
                }
            }
        }

        public static IEnumerable<TResult> SelectWithNext<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(previous, iterator.Current);
                    previous = iterator.Current;
                }
                yield return projection(previous, default(TSource));
            }
        }

        public static double Variance<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            // Compute the average of the sequence
            var avg = source.Average(selector);

            // Sum up the difference of each element from the average, squared
            double runningSum = 0;
            foreach (var value in source.Select(selector))
                runningSum += (value - avg) * (value - avg);

            // return the runningSum divided by the number of elements
            return Convert.ToDouble(runningSum / source.Count());
        }

        public static double StdDeviation<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            // The standard deviation is the square root of the variance
            return Math.Sqrt(source.Variance(selector));
        }

        public static IEnumerable<double> DivideBySum(this IEnumerable<double> me)
        {
            double sum = me.Sum();
            foreach (double d in me)
                yield return d / sum;
        }

        public static string ToCsv<T>(this IEnumerable<T> me)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (T value in me)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                sb.Append(value.ToString());
            }
            return sb.ToString();
        }
    }
}