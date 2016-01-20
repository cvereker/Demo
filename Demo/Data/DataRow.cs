using SystemExtensions.Primitives;
using System;
using System.Data;

namespace SystemExtensions.Data
{
    public static partial class DataRowExtensions
    {
        public static DataRow SetValue<T>(this DataRow row, DataColumn column, T value)
        {
            row.SetField(column, value);
            return row;
        }

        public static DataRow SetValue<T>(this DataRow row, int columnIndex, T value)
        {
            row.SetField(columnIndex, value);
            return row;
        }

        public static DataRow SetValue<T>(this DataRow row, string columnName, T value)
        {
            row.SetField(columnName, value);
            return row;
        }

        /// <summary>
        /// Sets double field with value. Checks for non-SQL values (NaN and infinity) and replaces with DBNull.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="value">Double value to set to the column</param>
        /// <returns></returns>
        public static DataRow SetDoubleValue(this DataRow row, string columnName, double value)
        {
            if (value.IsNumeric())
                row.SetField(columnName, value);
            else
                row.SetField(columnName, DBNull.Value);
            return row;
        }

        #region Field extraction and conversion

        public static T ToEnum<T>(this DataRow row, string columnName)
        {
            return (T)Enum.ToObject(typeof(T), row.ToInt32(columnName));
        }

        public static Int32 ToInt32(this DataRow row, string columnName)
        {
            return Convert.ToInt32(row[columnName]);
        }

        public static double ToDouble(this DataRow row, string columnName)
        {
            return Convert.ToDouble(row[columnName]);
        }

        public static DateTime ToDateTime(this DataRow row, string columnName)
        {
            return Convert.ToDateTime(row[columnName]);
        }

        public static string ToString(this DataRow row, string columnName)
        {
            return Convert.ToString(row[columnName]);
        }

        public static bool ToBoolean(this DataRow row, string columnName)
        {
            return Convert.ToBoolean(row[columnName]);
        }

        #endregion Field extraction and conversion
    }
}