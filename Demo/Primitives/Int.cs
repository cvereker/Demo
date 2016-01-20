using System;
using System.Collections.Generic;

namespace SystemExtensions.Primitives
{
    public static class IntExtensions
    {
        public static DateTime FirstDayFromMonthID(this int monthID)
        {
            int month = 1 + (monthID - 1) % 12;
            int year = 1901 + (monthID - 1) / 12;
            return new DateTime(year, month, 1);
        }

        /*Provides weights for exponential weighting, assuming that you are going from t=0 to T-1
         in ascening time (which you always are!!)*/

        public static IEnumerable<double> ExpWgts(this int T, double λ)
        {
            for (int t = 0; t < T; t++)
            {
                yield return Math.Max(double.Epsilon, Math.Pow(λ, T - t - 1));
            }
        }

        public static IEnumerable<double> ExpWgtsLongDecay(this int T)
        {
            return T.ExpWgts(0.997);
        }

        public static IEnumerable<double> ExpWgtsShortDecay(this int T)
        {
            return T.ExpWgts(0.97);
        }

        public static IEnumerable<double> ExpWgtsWeights(this int T)
        {
            return T.ExpWgtsShortDecay();
        }

        public static IEnumerable<double> TriWgts(this int T)
        {
            int t0 = 0;
            for (int t = 0; t < T; t++)
            {
                yield return (t - t0) / (double)(T - 1 - t0);
            }
        }

        public static IEnumerable<double> UnitVector(this int T)
        {
            for (int t = 0; t < T; t++)
            {
                yield return 1;
            }
        }
    }
}