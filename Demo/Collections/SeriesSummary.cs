using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemExtensions.Collections
{
    public class SeriesSummary<T> : IEnumerable<SeriesSummary<T>.SeriesSummaryRow<T>>
    {
        /// <summary>
        /// Nested class to hold the timeseries summary results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class SeriesSummaryRow<T>
        {
            public int SeriesID { get; set; }

            public DateTime MinDate { get; set; }

            public DateTime MaxDate { get; set; }

            public int NumObs { get; set; }

            public T FirstObs { get; set; }

            public T LastObs { get; set; }

            public override string ToString()
            {
                return string.Format("SeriesID {0}: {1:d}-{2:d} ({3} obs)", SeriesID, MinDate, MaxDate, NumObs);
            }
        }

        public SeriesSummary(IEnumerable<T> series, Func<T, int> groupSelector, Func<T, DateTime> dateSelector)
        {
            var series_summary =
                                (from v in series
                                 group v by groupSelector(v) into g
                                 select new SeriesSummaryRow<T>
                                 {
                                     SeriesID = g.Key,
                                     MinDate = g.Min(d => dateSelector(d)),
                                     MaxDate = g.Max(d => dateSelector(d)),
                                     NumObs = g.Count(),
                                     FirstObs = g.OrderBy(d => dateSelector(d)).First(),
                                     LastObs = g.OrderBy(d => dateSelector(d)).Last()
                                 }).ToList();

            _rows = series_summary;
        }

        private ICollection<SeriesSummaryRow<T>> _rows;

        public ICollection<SeriesSummaryRow<T>> Rows
        {
            get { return _rows; }
        }

        public IEnumerator<SeriesSummary<T>.SeriesSummaryRow<T>> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
    }
}