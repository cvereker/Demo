using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MathLib
{
    public static partial class MatrixExtensions
    {
        public static Matrix LastRow(this Matrix me)
        {
            return me.GetRows(me.Height - 1);
        }

        public static Matrix Log(this Matrix me)
        {
            for (int j = 0; j < me.Width; j++)
            {
                for (int i = 0; i < me.Height; i++)
                {
                    me[i, j] = Math.Log(me[i, j]);
                }
            }
            return me;
        }

        public static Matrix Change(this Matrix me)
        {
            for (int j = 0; j < me.Width; j++)
            {
                double prev_val = me[0, j];
                me[0, j] = 0;

                for (int i = 1; i < me.Height; i++)
                {
                    double val = me[i, j];
                    me[i, j] = val - prev_val;
                    prev_val = val;
                }
            }
            return me;
        }

        public static Matrix InsertColumn(this Matrix me, double value)
        {
            Matrix newMatrix = new Matrix(me.Height, me.Width + 1);
            for (int i = 0; i < me.Height; i++)
            {
                newMatrix[i, 0] = value;
                for (int j = 0; j < me.Width; j++)
                {
                    newMatrix[i, j + 1] = me[i, j];
                }
            }
            if (me.ColumnNames != null)
            {
                newMatrix.ColumnNames = new List<string>();
                newMatrix.ColumnNames.Add("0");
                for (int j = 0; j < me.Width; j++)
                    newMatrix.ColumnNames.Add(me.ColumnNames[j]);
            }

            return newMatrix;
        }

        public static bool IsIdentity(this Matrix me, int columnIndex)
        {
            double x = me[0, columnIndex];
            for (int i = 1; i < me.Height; i++)
            {
                if (me[i, columnIndex] != x)
                    return false;
            }
            return true;
        }

        public static MathLib.Matrix ToMatrix(this DataTable table)
        {
            return table.ToMatrix(0, table.Columns.Count - 1, 0);
        }

        public static MathLib.Matrix ToMatrix(this DataTable table, int firstCol)
        {
            return table.ToMatrix(firstCol, table.Columns.Count - 1);
        }

        public static MathLib.Matrix ToMatrix(this DataTable table, int firstCol, int lastCol)
        {
            return table.ToMatrix(firstCol, lastCol, double.NaN);
        }

        public static MathLib.Matrix ToMatrix(this DataTable me, int firstCol, int lastCol, double nullDataValue)
        {
            if (lastCol >= me.Columns.Count)
                lastCol = me.Columns.Count - 1;

            int width = lastCol - firstCol + 1;
            int height = me.Rows.Count;

            MathLib.Matrix mat = new MathLib.Matrix(height, width);

            DataRow dr;

            mat.ColumnNames = Enumerable.Repeat("", width).ToList();

            for (int j = 0, c = firstCol; j < width; j++, c++)
                mat.ColumnNames[j] = me.Columns[c].ColumnName;

            for (int i = 0; i < height; i++)
            {
                dr = me.Rows[i];
                for (int j = 0, c = firstCol; j < width; j++, c++)
                    mat[i, j] = dr[c] == DBNull.Value ? nullDataValue : Convert.ToDouble(dr[c]);
            }

            return mat;
        }

        public static MathLib.Matrix ToMatrix(this DataView me, int firstCol, int lastCol, double nullDataValue)
        {
            if (lastCol >= me.Table.Columns.Count)
                lastCol = me.Table.Columns.Count - 1;

            int width = lastCol - firstCol + 1;
            int height = me.Count;

            MathLib.Matrix mat = new MathLib.Matrix(height, width);

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