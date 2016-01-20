using System;
using System.Data;

namespace SystemExtensions.Data
{
    public static partial class DataViewExtensions
    {
        public static double[,] ToMatrix(this DataView me, int firstCol, int lastCol, double nullDataValue)
        {
            if (lastCol >= me.Table.Columns.Count)
                lastCol = me.Table.Columns.Count - 1;

            int width = lastCol - firstCol + 1;
            int height = me.Count;

            double[,] mat = new double[height, width];

            DataRowView dr;
            for (int i = 0; i < height; i++)
            {
                dr = me[i];
                for (int j = 0, c = firstCol; j < width; j++, c++)
                    mat[i, j] = dr[c] == DBNull.Value ? nullDataValue : Convert.ToDouble(dr[c]);
            }

            return mat;
        }
    }
}