using System.Collections.Generic;

namespace SystemExtensions.Primitives
{
    public static class DoubleExtensions
    {
        public static bool IsNumeric(this double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return false;
            else
                return true;
        }

        public static object DBDouble(this double? value)
        {
            return value.HasValue ? (object)value.Value : System.DBNull.Value;
        }

        public static IEnumerable<double> AsEnumerableEx(this double[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                    yield return arr[i, j];
        }

        public static object ToDBFriendlyDouble(this double d)
        {
            return double.IsNaN(d) || double.IsInfinity(d) ? System.DBNull.Value : (object)d;
        }
    }
}