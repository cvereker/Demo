using System;
using System.Collections.Generic;

// Code Based Off .NET Mapack
// -------------------------------------------------------------------------
// Lutz Roeder's .NET Mapack, adapted from Mapack for COM and Jama routines.
// Copyright (C) 2001-2003 Lutz Roeder. All rights reserved.
// http://www.aisto.com/roeder/dotnet
// roeder@aisto.com
// -------------------------------------------------------------------------
namespace MathLib
{
    public delegate double DoubleScalar(double a);

    /// <summary>
    /// Simple Class For Handling Matrices.
    /// Implements:
    /// </summary>
    public class Matrix : object, ICloneable
    {
        #region Class Parameters

        /// <summary>
        /// Internal array of the matrix data (access via indexer)
        /// </summary>
        protected double[,] _MatrixData;

        private LUDecomposition LUDecomp;
        private QRDecomposition QRDecomp;
        private AutoCorrelationComposition _aCorrelComp;
        private EigenvalueDecomposition EigenDecomp;
        private SVDDecomposition SVDDecomp;
        private Matrix.CholeskyDecomposition CholeskyDecomp;

        /// <summary>
        /// Used with the EigenVectorMatrix2 and RealEigenValues2 properties - assume matrix is symmetric
        /// </summary>
        private EigenvalueDecomposition EigenDecomp2;

        protected /*ShallowTransposeMatrix*/ Matrix _shallowtranspose;

        #endregion Class Parameters

        #region Constructor

        private Matrix()
        {
        }

        /// <summary>
        /// Construct A Matrix Of Specified Size
        /// </summary>
        /// <param name="Height"></param>
        /// <param name="Width"></param>
        public Matrix(int Height, int Width)
            : this()
        {
            this._MatrixData = new double[Height, Width];
        }

        /// <summary>
        /// Construct A Matrix Vector Of Specified Size
        /// </summary>
        /// <param name="Height">The height of the matrix. (The width is 1)</param>
        public Matrix(int Height)
            : this()
        {
            this._MatrixData = new double[Height, 1];
        }

        /// <summary>
        /// Construct A Matrix From A 2D Double Array
        /// </summary>
        /// <param name="data"></param>
        public Matrix(double[,] data)
            : this()
        {
            this._MatrixData = (double[,])data.Clone();
        }

        public Matrix(int[,] data)
            : this()
        {
            this._MatrixData = (double[,])data.Clone();
        }

        /// <summary>
        /// Create matrix from jagged array
        /// </summary>
        /// <param name="data"></param>
        public Matrix(double[][] data)
            : this()
        {
            int rows = data.GetLength(0);
            int cols = data[0].Length;

            //find the size of smallest row
            for (int i = 0; i < rows; i++)
                cols = Math.Min(cols, data[i].Length);

            this._MatrixData = new double[rows, cols];

            //copy data from jagged array to 2D array
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    this._MatrixData[i, j] = data[i][j];
        }

        public Matrix(int[] data)
            : this()
        {
            this._MatrixData = new double[data.Length, 1];

            // unfortunately .NET does not allow array copying between double and single arrays.
            int i = 0, I = data.Length;
            for (; i < I; i++)
            {
                this._MatrixData[i, 0] = data[i];
            }
        }

        /// <summary>
        /// Construct an Nx1 Matrix From A 1D Double Array
        ///
        /// </summary>
        /// <param name="data"></param>
        public Matrix(double[] data)
            : this()
        {
            this._MatrixData = new double[data.Length, 1];

            // unfortunately .NET does not allow array copying between double and single arrays.
            int i = 0, I = data.Length;
            for (; i < I; i++)
            {
                this._MatrixData[i, 0] = data[i];
            }
        }

        public Matrix(double[] data, bool bRow)
            : this()
        {
            if (bRow)
            {
                this._MatrixData = new double[1, data.Length];

                int i, I = data.Length;
                for (i = 0; i < I; i++)
                    this._MatrixData[0, i] = data[i];
            }
            else
            {
                this._MatrixData = new double[data.Length, 1];

                int i = 0, I = data.Length;
                for (; i < I; i++)
                {
                    this._MatrixData[i, 0] = data[i];
                }
            }
        }

        public Matrix(Matrix mat)
            : this()
        {
            this._MatrixData = (double[,])(mat._MatrixData.Clone());
        }

        #endregion Constructor

        #region Public Properties

        // non virtual method should be faster
        virtual public double this[int iRow, int jCol, int ignoredIntegerForThisSignature]
        {
            get
            {
                return this._MatrixData[iRow, jCol];
            }
            set
            {
                this._MatrixData[iRow, jCol] = value;
            }
        }

        /// <summary>
        /// Indexer For The Matrix. i is the row number. j is the column number.
        /// </summary>
        public virtual double this[int iRow, int jCol]
        {
            get
            {
                return this._MatrixData[iRow, jCol];
            }
            set
            {
                this._MatrixData[iRow, jCol] = value;
                this.LUDecomp = null;
                this.QRDecomp = null;
                this.EigenDecomp = null;
            }
        }

        /// <summary>
        /// Indexer For a Vector Matrix. i is the row number.
        /// </summary>
        public virtual double this[int iRow]
        {
            get
            {
                return this._MatrixData[iRow, 0];
            }
            set
            {
                this._MatrixData[iRow, 0] = value;
                this.LUDecomp = null;
                this.QRDecomp = null;
                this.EigenDecomp = null;
            }
        }

        /// <summary>
        /// The Width Of The Matrix
        /// </summary>
        public virtual int Width
        {
            get
            {
                return this._MatrixData.GetLength(1);//_width;
            }
        }

        public virtual int Columns
        {
            get
            {
                return this._MatrixData.GetLength(1);//_width;
            }
        }

        /// <summary>
        /// The Height Of The Matrix
        /// </summary>
        public virtual int Height
        {
            get
            {
                return this._MatrixData.GetLength(0);//_height;
            }
        }

        public virtual int Rows
        {
            get
            {
                return this._MatrixData.GetLength(0);//_height;
            }
        }

        /// <summary>
        /// Is The Matrix Square
        /// </summary>
        public bool IsSquare
        {
            get
            {
                return this.Width == this.Height;
            }
        }

        /// <summary>
        /// Is The Matrix Symmetric
        /// </summary>
        public bool IsSymmetric
        {
            get
            {
                if (this.IsSquare)
                {
                    int i = 0, I = this.Height, j;

                    for (; i < I; i++)
                    {
                        for (j = 0; j <= i; j++)
                        {
                            // this operation is symmetric! Therefore we can directly access the underlying data
                            if (_MatrixData[i, j] != _MatrixData[j, i])
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Is The Matrix NonSingular
        /// </summary>
        public bool IsNonSingular
        {
            get
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return LUDecomp.IsNonSingular;
            }
        }

        /// <summary>
        /// Is The Matrix Of Full Rank
        /// </summary>
        public bool IsFullRank
        {
            get
            {
                if (QRDecomp == null) QRDecomp = new QRDecomposition(this);
                return QRDecomp.IsFullRank;
            }
        }

        /// <summary>
        /// The Trace Of The Matrix
        /// </summary>
        public double Trace
        {
            get
            {
                double output = 0;

                int i = 0, I = Math.Min(this.Width, this.Height);
                for (; i < I; i++)
                {
                    output += this[i, i];
                }

                return output;
            }
        }

        /// <summary>
        /// Calculate The Norm Of A Matrix
        /// </summary>
        public double Norm1
        {
            get
            {
                double output = 0, tmp;

                int i, j = 0, J = this.Width, I = this.Height;

                for (; j < J; j++)
                {
                    tmp = 0;

                    for (i = 0; i < I; i++)
                    {
                        tmp += Math.Abs(this[i, j]);
                    }

                    output = Math.Max(tmp, output);
                }

                return output;
            }
        }

        /// <summary>
        /// Calculate The L2 Norm Of A Matrix
        /// </summary>
        public double Norm2
        {
            get
            {
                double norm2 = 0;

                for (int j = 0; j < this.Width; j++)
                {
                    for (int i = 0; i < this.Height; i++)
                    {
                        norm2 += this._MatrixData[i, j] * this._MatrixData[i, j];
                    }
                }
                return Math.Sqrt(norm2);
            }
        }

        /// <summary>
        /// Calculate The InfinityNorm
        /// </summary>
        public double InfinityNorm
        {
            get
            {
                double output = 0, tmp;

                int i, j = 0, I = this.Width, J = this.Height;
                for (; j < J; j++)
                {
                    tmp = 0;

                    for (i = 0; i < I; i++)
                    {
                        tmp += Math.Abs(this[j, i]);
                    }

                    output = Math.Max(tmp, output);
                }

                return output;
            }
        }

        /// <summary>
        /// Calculates The FrobeniusNorm
        /// </summary>
        public double FrobeniusNorm
        {
            get
            {
                double f = 0.0;
                int i = 0, j, I = this.Height, J = this.Width;
                for (; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        f = MathLib.Matrix.Hypotenuse(f, this[i, j]);
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// Calculate sum of element squares Of A Matrix
        /// </summary>
        public double SumOfSquares
        {
            get
            {
                double sum = 0;

                for (int j = 0; j < this.Width; j++)
                    for (int i = 0; i < this.Height; i++)
                        sum += this._MatrixData[i, j] * this._MatrixData[i, j];

                return sum;
            }
        }

        /// <summary>
        /// The Transpose Of The Matrix
        /// </summary>
        public Matrix Transpose
        {
            get
            {
                int j, i = 0, I = this.Height, J = this.Width;
                MathLib.Matrix output = new Matrix(J, I);

                for (; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        output[j, i] = this[i, j];
                    }
                }

                return output;
            }
        }

        /* A.C. Shallow transpose relies on indexers which are inefficient.
         * Moreover it creates inconsistency between matrix indexers and actual matrix data,
         * because m[i,j] != m._MatrixData[i,j].
         * */

        /// <summary>
        /// The Transpose Of The Matrix
        /// </summary>
        [Obsolete("use Transpose", true)]
        public virtual Matrix ShallowTranspose
        {
            get
            {
                if (null == (object)_shallowtranspose)
                {
                    _shallowtranspose = new ShallowTransposeMatrix(this);
                }
                return _shallowtranspose;
            }
        }

        /// <summary>
        /// Shorthand notation for shallow transpose of a matrix
        /// UPDATED: Greenwich have broken the shallow transpose by not using the indexing functions in the inverse functions. I have therefore
        /// reverted this to returning an actual transpose matrix. Note this is likely to have a performance hit, but unfortunately there is no
        /// other option.
        /// </summary>
        public Matrix Tr
        {
            get
            {
                return this.Transpose;
                //return this.ShallowTranspose;  SEE COMMENT ABOVE
            }
        }

        /// <summary>
        /// Gets The Inverse Of The Matrix
        /// </summary>
        public Matrix Inverse
        {
            get
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return (this.Solve(MathLib.Matrix.Diagonal(this.Height, this.Width, 1.0)));
            }
        }

        /// <summary>
        /// Atler ego of Inverse property
        /// </summary>
        public Matrix Inv
        {
            get
            {
                return this.Inverse;
            }
        }

        /// <summary>
        /// The Lower Triangular Factor Of The LU Decomp
        /// </summary>
        public Matrix LULowerTriangularFactor
        {
            get
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return LUDecomp.LowerTriangularFactor;
            }
        }

        /// <summary>
        /// The Upper Triangular Factor Of An LU Decomp
        /// </summary>
        public Matrix LUUpperTriangularFactor
        {
            get
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return LUDecomp.UpperTriangularFactor;
            }
        }

        /// <summary>
        /// The Orthogonal Factor Of The QR Decomp
        /// </summary>
        public Matrix QROrthogonalFactor
        {
            get
            {
                if (QRDecomp == null) QRDecomp = new QRDecomposition(this);
                return QRDecomp.OrthogonalFactor;
            }
        }

        /// <summary>
        /// The Upper Triangular Factor Of The QR Decomp
        /// </summary>
        public Matrix QRUpperTriangularFactor
        {
            get
            {
                if (QRDecomp == null) QRDecomp = new QRDecomposition(this);
                return QRDecomp.UpperTriangularFactor;
            }
        }

        public Matrix SVDLeftMatrix
        {
            get
            {
                if (SVDDecomp == null) SVDDecomp = new SVDDecomposition(this);
                return SVDDecomp.U;
            }
        }

        public Matrix SVDRightMatrix
        {
            get
            {
                if (SVDDecomp == null) SVDDecomp = new SVDDecomposition(this);
                return SVDDecomp.V;
            }
        }

        public double[] SVDSingularValues
        {
            get
            {
                if (SVDDecomp == null) SVDDecomp = new SVDDecomposition(this);
                return SVDDecomp.S;
            }
        }

        /// <summary>
        /// Gets The Determinant Of The Matrix
        /// </summary>
        public double Determinant
        {
            get
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return LUDecomp.Determinant;
            }
        }

        /// <summary>
        /// Gets An Array Of The Real Components Of The Eigenvalues
        /// </summary>
        public double[] RealEigenvalues
        {
            get
            {
                if (double.IsNaN(this.Trace)) return null;
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.RealEigenvalues;
            }
        }

        /// <summary>
        /// Gets An Array Of The Imaginary Components Of The Eigenvalues
        /// </summary>
        public double[] ImaginaryEigenvalues
        {
            get
            {
                if (double.IsNaN(this.Trace)) return null;
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.ImaginaryEigenvalues;
            }
        }

        public Matrix RealEigenvector
        {
            get
            {
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.RealEigenvector;
            }
        }

        public Matrix ImaginaryEigenvector
        {
            get
            {
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.ImaginaryEigenvector;
            }
        }

        public Matrix EigenvectorMatrix2
        {
            get
            {
                if (EigenDecomp2 == null) EigenDecomp2 = new EigenvalueDecomposition(this, true);
                Matrix mat = EigenDecomp2.EigenvectorMatrix;
                Matrix matR = new Matrix(mat.Height, mat.Width);

                int k = 0, K = mat.Height;
                int J = mat.Width, j = J - 1, i;
                for (i = 0, j = J - 1; i < J; i++, j--)
                {
                    for (k = 0; k < K; k++)
                    {
                        matR[k, j] = mat[k, i];
                    }
                }
                return matR;
            }
        }

        public double[] RealEigenvalues2
        {
            get
            {
                if (EigenDecomp2 == null) EigenDecomp2 = new EigenvalueDecomposition(this, true);
                double[] vec = (double[])EigenDecomp2.RealEigenvalues;
                Array.Reverse(vec);
                return vec;
            }
        }

        /// <summary>
        /// Gets A Matrix Containing The Eigenvectors
        /// </summary>
        public Matrix EigenvectorMatrix
        {
            get
            {
                if (double.IsNaN(this.Trace)) return null;
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.EigenvectorMatrix;
            }
        }

        /// <summary>
        /// Gets A Diagonal Matrix Of Eigenvalues
        /// </summary>
        public Matrix DiagonalMatrix
        {
            get
            {
                if (EigenDecomp == null) EigenDecomp = new EigenvalueDecomposition(this);
                return EigenDecomp.DiagonalMatrix;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Linear equation solution by Gauss-Jordan elimination.
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public double[] GaussElimination(double[] rhs)
        {
            if (this.Width != this.Height)
                throw new Exception("Matrix has to be square to use Gauss elimination");
            if (this.Width != rhs.Length)
                throw new Exception("Right hand side vector of wrong length");

            double[,] a = (double[,])((double[,])this).Clone();
            double[] b = (double[])((double[])rhs).Clone();

            int n = this.Width;

            int[] indxc, indxr, ipiv;
            int icol = 0;
            int irow = 0;
            double dum, pivinv, temp;

            indxc = new int[n];
            indxr = new int[n];
            ipiv = new int[n];

            for (int i = 0; i < n; i++)
            {
                double big = 0.0;
                for (int j = 0; j < n; j++)
                    if (ipiv[j] != 1)
                        for (int k = 0; k < n; k++)
                        {
                            if (ipiv[k] == 0)
                            {
                                if (Math.Abs(a[j, k]) >= big)
                                {
                                    big = Math.Abs(a[j, k]);
                                    irow = j;
                                    icol = k;
                                }
                            }
                        }
                ++(ipiv[icol]);
                if (irow != icol)
                {
                    for (int l = 0; l < n; l++)
                    {
                        temp = a[irow, l];
                        a[irow, l] = a[icol, l];
                        a[icol, l] = temp;
                    }

                    temp = b[irow];
                    b[irow] = b[icol];
                    b[icol] = temp;
                }
                indxr[i] = irow;
                indxc[i] = icol;
                if (a[icol, icol] == 0.0)
                    throw new Exception("Singular Matrix");

                pivinv = 1.0 / a[icol, icol];
                a[icol, icol] = 1.0;
                for (int l = 0; l < n; l++)
                    a[icol, l] *= pivinv;

                b[icol] *= pivinv;

                for (int ll = 0; ll < n; ll++)
                    if (ll != icol)
                    {
                        dum = a[ll, icol];
                        a[ll, icol] = 0.0;
                        for (int l = 0; l < n; l++)
                            a[ll, l] -= a[icol, l] * dum;
                        b[ll] -= b[icol] * dum;
                    }
            }
            for (int l = n - 1; l >= 0; l--)
            {
                if (indxr[l] != indxc[l])
                    for (int k = 0; k < n; k++)
                    {
                        temp = a[k, indxr[l]];
                        a[k, indxr[l]] = a[k, indxc[l]];
                        a[k, indxc[l]] = temp;
                    }
            }

            return b;
        }

        /// <summary>
        /// Returns a matrix of identical size, to which the function func has been applied to each element of the matrix
        /// </summary>
        /// <param name="func">Function to apply</param>
        /// <returns>A new Matrix to which func has been applied on an element by element basis</returns>
        public Matrix MatrixFunction(DoubleScalar func)
        {
            if (func == null)
                return null;

            Matrix A = new Matrix(this.Height, this.Width);

            int i = 0, I = A.Height;
            int j = 0, J = A.Width;
            for (; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    A[i, j] = func(this[i, j]);
                }
            }
            return A;
        }

        /// <summary>
        /// Gets The Row Of A Matrix As A Vector
        /// </summary>
        /// <param name="row">Index Of The Row</param>
        /// <returns></returns>
        public MathLib.Vector GetRow(int row)
        {
            MathLib.Vector output = new Vector(this.Width);

            int i, I = this.Width;
            for (i = 0; i < I; i++)
            {
                output[i] = this[row, i];
            }

            return output;
        }

        /// <summary>
        /// Gets A Row Of The Matrix as a Vector
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        [Obsolete("Use GetRows instead", false)]
        public Matrix GetRowMat(int row)
        {
            Matrix output = new Matrix(this.Width);
            int i, I = this.Width;
            for (i = 0; i < I; i++)
            {
                output[i] = this[row, i];
            }

            return output;
        }

        /// <summary>
        /// Gets A Column Of The Matrix as a Vector
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public MathLib.Vector GetColumn(int column)
        {
            MathLib.Vector output = new Vector(this.Height);
            double[] vec_data = (double[])output;

            int i = 0, I = this.Height;
            for (i = 0; i < I; i++)
            {
                vec_data[i] = this._MatrixData[i, column];
            }

            return output;
        }

        /// <summary>
        /// Gets A Column Of The Matrix As A Matrix
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        [Obsolete("Use GetColumns instead", false)]
        public MathLib.Matrix GetColumnMat(int column)
        {
            MathLib.Matrix output = new Matrix(this.Height);

            int i = 0, I = this.Height;
            for (i = 0; i < I; i++)
            {
                output[i] = this[i, column];
            }

            return output;
        }

        public MathLib.Matrix GetColumns(params int[] columns)
        {
            return this.Submatrix(0, this.Height - 1, columns);
        }

        public MathLib.Matrix GetRows(params int[] rows)
        {
            return this.Submatrix(rows, 0, this.Width - 1);
        }

        public MathLib.Matrix GetRowsFromMask(params int[] rowMask)
        {
            // returns the rows corresponding to the items in the mask that are non-zero.

            System.Collections.ArrayList al = new System.Collections.ArrayList();
            for (int i = 0; i < rowMask.Length; i++)
            {
                if (rowMask[i] != 0)
                {
                    al.Add(i);
                }
            }

            int[] rows = (int[])al.ToArray(typeof(Int32));
            return GetRows(rows);
        }

        public MathLib.Matrix GetColumnsFromMask(params int[] columnMask)
        {
            // returns the columns corresponding to the items in the mask that are non-zero.

            System.Collections.ArrayList al = new System.Collections.ArrayList();
            for (int i = 0; i < columnMask.Length; i++)
            {
                if (columnMask[i] != 0)
                {
                    al.Add(i);
                }
            }

            int[] columns = (int[])al.ToArray(typeof(Int32));
            return GetColumns(columns);
        }

        /// <summary>
        /// Create A Clone Of The Matrix
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            MathLib.Matrix output = new Matrix((double[,])this);

            return output;
        }

        public Matrix Submatrix(int i0, int i1, int[] columns)
        {
            int XHeight = i1 - i0 + 1;

            // allow null to signify "please return all columns for these rows"
            if (columns == null)
            {
                columns = new int[this.Width];
                for (int j = 0, J = this.Width; j < J; j++)
                    columns[j] = j;
            }

            double[,] X = new double[XHeight, columns.Length];

            int c = 0, i, C = columns.Length;
            int thisHeight = this.Height;

            for (; c < C; c++)
                for (i = i0; i <= i1; i++)
                    X[i - i0, c] = this[i, columns[c]];

            /*would be optimised by use of array copy*/
            return new Matrix(X);
        }

        /// <summary>
        /// Get A SubMatrix
        /// </summary>
        /// <param name="r">1D vector of row number to retrieve</param>
        /// <param name="j0">Starting at column j0</param>
        /// <param name="j1">Ending at column j1</param>
        /// <returns></returns>
        public Matrix Submatrix(int[] rows, int j0, int j1)
        {
            int XWidth = j1 - j0 + 1;

            // allow null to signify "please return all rows for these columns"
            if (rows == null)
            {
                rows = new int[this.Height];
                for (int i = 0, I = this.Height; i < I; i++)
                    rows[i] = i;
            }

            double[,] X = new double[rows.Length, XWidth];

            int r = 0, j, R = rows.Length;
            int thisWidth = this.Width;

            // if it is a shallow transpose matrix then we will have to use a loop
            if (this is ShallowTransposeMatrix)
            {
                for (; r < R; r++)
                    for (j = j0; j <= j1; j++)
                        X[r, j - j0] = this[rows[r], j];
            }
            // otherewise we use Array.Copy, which should be much faster
            else
            {
                for (r = 0; r < R; r++)
                {
                    Array.Copy(this._MatrixData, thisWidth * rows[r] + j0, X, r * XWidth, XWidth);
                }
            }

            return new Matrix(X);
        }

        /// <summary>
        /// Gets A SubMatrix
        /// </summary>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <param name="j0"></param>
        /// <param name="j1"></param>
        /// <returns></returns>
        public Matrix Submatrix(int i0, int i1, int j0, int j1)
        {
            int XWidth = j1 - j0 + 1;
            int XHeight = i1 - i0 + 1;

            double[,] X = new double[XHeight, XWidth];

            int i = 0, j;

            // if it is a shallow transpose matrix then we will have to use a loop
            if (this is ShallowTransposeMatrix)
            {
                for (i = i0; i <= i1; i++)
                    for (j = j0; j <= j1; j++)
                        X[i - i0, j - j0] = this[i, j];
            }
            // otherewise we use Array.Copy, which should be much faster
            else
            {
                int W = this.Width;
                for (i = 0; i < XHeight; i++)
                {
                    Array.Copy(this._MatrixData, (i + i0) * W + j0, X, i * XWidth, XWidth);
                }
            }

            return new Matrix(X);
        }

        /// <summary>
        /// Solve A Linear System
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public Matrix Solve(Matrix rhs)
        {
            if (this.Width != this.Height)
            {
                if (QRDecomp == null) QRDecomp = new QRDecomposition(this);
                return QRDecomp.Solve(rhs);
            }
            else
            {
                if (LUDecomp == null) LUDecomp = new LUDecomposition(this);
                return LUDecomp.Solve(rhs);
            }
        }

        /// <summary>
        /// Returns the maximum number in the matrix
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public double MatrixMax()
        {
            return MatrixMax(double.NegativeInfinity);
        }

        /// <summary>
        /// Returns the maximum number in the matrix greate than dMax
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="dMax"></param>
        /// <returns></returns>
        public double MatrixMax(double dMax)
        {
            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    dMax = Math.Max(Math.Abs(this[i, j]), dMax);
                }
            }
            return dMax;
        }

        /// <summary>
        /// Returns the Moore-Penrose inverse (pseudoinverse) of the matrix.
        /// </summary>
        public Matrix PseudoInverse
        {
            get
            {
                /* Given matrix A, the pseudoinverse matrix B
                 * satisfies the following four equalities:
                 * ABA = A
                 * BAB = B
                 * (AB)' = AB
                 * (BA)' = BA
                */
                const double TOL = 1E-13;
                double smax = 0, thresh;
                Matrix U, V, S;
                double[] s;

                U = this.SVDLeftMatrix;
                V = this.SVDRightMatrix;
                s = this.SVDSingularValues;

                // find the maximum
                for (int i = 0; i < s.Length; i++)
                    smax = Math.Max(s[i], smax);
                thresh = TOL * smax;

                S = new Matrix(s.Length, s.Length);
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] < thresh)
                        S[i, i] = 0;
                    else
                        S[i, i] = 1 / s[i];
                }

                return V * S * U.Transpose;
            }
        }

        #endregion Public Methods

        #region Operator Overrides

        #region ExplicitCasts

        /// <summary>
        /// Explicit cast of a matrix to a double. Will only work if the matrix has only 1 value in it.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double (Matrix value)
        {
            if (value.Width != 1 && value.Height != 1)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
                return value[0, 0];
        }

        #endregion ExplicitCasts

        #region ToString()

        /// <summary>
        /// Override The ToString() Method
        /// </summary>
        /// <returns>Matrix with columns seperated by tabs, rows on new lines</returns>
        public override string ToString()
        {
            string mat_preview = string.Format("Matrix; {0}x{1} ", Height, Width); ;
            for (int i = 0; i < Height && i < 3; i++)
                for (int j = 0; j < Width && j < 3; j++)
                    mat_preview += string.Format(" ({0},{1})={2}", i, j, this[i, j]);

            return mat_preview;
        }

        public string ToString(string name, int dp)
        {
            string mat_preview = string.Format("{2}; {0}x{1} ", Height, Width, name); ;
            for (int i = 0; i < Height && i < 3; i++)
                for (int j = 0; j < Width && j < 3; j++)
                    mat_preview += string.Format(" ({0},{1:0.##})={2:0.##}", i, j, this[i, j]);

            return mat_preview;
        }

        public string ToExcel()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                    sb.Append(string.Format("{0}\t", this[i, j]));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Overload ToString() Method
        /// </summary>
        /// <param name="DecimalPlaces"></param>
        /// <returns>Matrix formatted to the requested number of decimal places</returns>
        public string ToString(int DecimalPlaces)
        {
            string format = string.Format("{0}0, 0:F{1,0:D}{2}", "{", DecimalPlaces, "}\t");
            return ToString(format);
        }

        /// <summary>
        /// Overload ToString() Method
        /// </summary>
        /// <param name="DecimalPlaces"></param>
        /// <param name="delimiter">Delimieted to use (typically a comma)</param>
        /// <returns>Matrix formatted to the requested number of decimal places</returns>
        public string ToString(int DecimalPlaces, string delimiter)
        {
            string format = string.Format("{0}0, 0:F{1,0:D}{2}{3}", "{", DecimalPlaces, "}", delimiter);
            return ToString(format);
        }

        /// <summary>
        /// Overload ToString() method
        /// </summary>
        /// <param name="format">A valid format string e.g. {0:E4}</param>
        /// <returns>Formatted matrix</returns>
        public virtual string ToString(string format)
        {
            string format_ = format;// = format + "\t";

            if (this.Height == 0 && this.Width == 0)
                return "null matrix";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int i = 0, j = 0, I = this.Height, J = this.Width;
            for (; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    // will throw format exception if an invalid format is passed
                    sb.AppendFormat(format_, this._MatrixData[i, j]);
                }

                // remove trailing tab
                sb.Remove(sb.Length - 1, 1);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion ToString()

        #region Scalar Operators

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator *(double lhs, Matrix rhs)
        {
            Matrix output = new Matrix(rhs.Height, rhs.Width);

            int i = 0, I = rhs.Height;
            int j, J = rhs.Width;
            for (; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    output._MatrixData[i, j] = rhs._MatrixData[i, j] * lhs;
                }
            }

            return output;
        }

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator *(Matrix lhs, double rhs)
        {
            return rhs * lhs;
        }

        /// <summary>
        /// /  Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator /(Matrix lhs, double rhs)
        {
            return (1 / rhs) * lhs;
        }

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator +(double lhs, Matrix rhs)
        {
            Matrix output = new Matrix(rhs.Height, rhs.Width);

            int i = 0, I = rhs.Height;
            int j, J = rhs.Width;
            for (; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    output._MatrixData[i, j] = rhs._MatrixData[i, j] + lhs;
                }
            }

            return output;
        }

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator +(Matrix lhs, double rhs)
        {
            return rhs + lhs;
        }

        /// <summary>
        /// - Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator -(double lhs, Matrix rhs)
        {
            //return lhs + -1 * rhs;
            /* Explicitly written out to replace above line and to avoid unnecessary multiplication
             *
             * */

            Matrix output = new Matrix(rhs.Height, rhs.Width);
            int i = 0, I = rhs.Height;
            int j, J = rhs.Width;

            for (; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    output._MatrixData[i, j] = lhs - rhs._MatrixData[i, j];
                }
            }

            return output;
        }

        /// <summary>
        /// - Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator -(Matrix lhs, double rhs)
        {
            return lhs + -1 * rhs;
        }

        #endregion Scalar Operators

        #region Matrix Operators

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator +(Matrix lhs, Matrix rhs)
        {
            if ((lhs.Width != rhs.Width) || (lhs.Height != rhs.Height))
            {
                throw new ArgumentException("lhs Dimensions != rhs Dimensions");
            }
            else
            {
                int i = 0, I = rhs.Height;
                int j, J = rhs.Width;

                MathLib.Matrix output = new Matrix(I, J);

                for (i = 0; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        output._MatrixData[i, j] = lhs._MatrixData[i, j] + rhs._MatrixData[i, j];
                    }
                }

                return output;
            }
        }

        /// <summary>
        /// - Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator -(Matrix lhs, Matrix rhs)
        {
            //return lhs + -1 * rhs;
            /* Explicitly written out (to replace the above line) to improve efficiency and to avoid an unnecessary multiplication
             * */
            if ((lhs.Width != rhs.Width) || (lhs.Height != rhs.Height))
            {
                throw new ArgumentException("lhs Dimensions != rhs Dimensions");
            }
            else
            {
                int i, I = rhs.Height;
                int j, J = rhs.Width;
                MathLib.Matrix output = new Matrix(I, J);

                for (i = 0; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        output._MatrixData[i, j] = lhs._MatrixData[i, j] - rhs._MatrixData[i, j];
                    }
                }

                return output;
            }
        }

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Matrix operator *(Matrix lhs, Matrix rhs)
        {
            if (lhs.Width != rhs.Height)
            {
                throw new ArgumentException("lhs Width != rhs Height");
            }
            else
            {
                int i = 0, I = lhs.Height;
                int j, J = rhs.Width;
                int k, K = lhs.Width;

                MathLib.Matrix output = new Matrix(I, J);

                for (i = 0; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        /* The commented out line is a woefully inefficient way in which to multiply matrices, especially
                         * when they get big! (For a matrix multiplication with dimensions lhs(I,K) rhs (K,J) this involves
                         * 2*I*J calls to GetRow, which copies the data, and then a loop of K for each multiplication!
                         *
                         * //output[i,j]=Vector.DotProduct(lhs.GetRow(i), rhs.GetColumn(j));
                         *
                         * */
                        /*
                         * rh: that virtual method is called n^3 times, and re-assigns null each time
                         * To speed this up I made a non-vitual method that skips the nullifying step.
                         * First call is to regular virtual method to pick up standard side effects of the call.
                         * results 1% speed up
                         */
                        /*
                        if(K>0) {
                            output[i,j] += lhs[i,0]*rhs[0,j];
                        }
                        for (k=1; k<K; k++)
                        {
                            output[i,j,0] += lhs[i,k,0]*rhs[k,j,0];
                        }*/
                        double sum = 0;
                        for (k = 0; k < K; k++)
                        {
                            sum += lhs._MatrixData[i, k] * rhs._MatrixData[k, j];
                        }
                        output._MatrixData[i, j] = sum;
                    }
                }
                return output;
            }
        }

        #endregion Matrix Operators

        #region Power operations

        /// <summary>
        /// Calculates matrix A (square matrix) to the power of n
        /// </summary>
        /// <param name="A">A square matrix</param>
        /// <param name="n">An integer</param>
        /// <returns>A to the power of n</returns>
        public static MathLib.Matrix operator ^(Matrix A, int n /*power*/)
        {
            if (A.IsSquare == false)
                throw new ArgumentException("To calculate the power of a matrix, it must be square");

            Matrix output = A;
            if (n < 5)
            {
                for (int i = 1; i < n; i++)
                {
                    output *= A;
                }
            }
            else
            {
                Matrix P = A;
                int np = n - 1;
                while (np >= 1)
                {
                    if (np % 2d == 0)
                    {
                        np = np / 2;
                    }
                    else
                    {
                        np = (np - 1) / 2;
                        output *= P;
                    }

                    // saves a matrix multiplication
                    if (np == 0)
                        continue;
                    P = P * P;
                }
            }
            return output;
        }

        /// <summary>
        /// Calculates matrix A (square matrix) to the power of n
        /// </summary>
        /// <param name="n">An integer</param>
        /// <returns>A to the power of n</returns>
        public MathLib.Matrix Pow(int n /*power*/)
        {
            return this ^ n;
        }

        #endregion Power operations

        #region Vector Operators

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator *(Matrix lhs, Vector rhs)
        {
            if (lhs.Width != rhs.Dimension)
            {
                throw new ArgumentException("lhs Width != rhs Height");
            }
            else
            {
                MathLib.Vector output = new Vector(lhs.Height);
                int i, I = output.Dimension;
                int j, J = rhs.Dimension;
                double[] vec = (double[])rhs;

                for (i = 0; i < I; i++)
                {
                    double sum = 0;
                    for (j = 0; j < J; j++)
                        sum += lhs._MatrixData[i, j] * vec[j];

                    output[i] = sum;
                }

                return output;
            }
        }

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator *(Vector lhs, Matrix rhs)
        {
            if (lhs.Dimension != rhs.Height)
            {
                throw new ArgumentException("lhs Width != rhs Height");
            }
            else
            {
                MathLib.Vector output = new Vector(rhs.Width);
                int i, I = output.Dimension;
                int j, J = lhs.Dimension;

                double[] vec = (double[])lhs;

                for (i = 0; i < I; i++)
                {
                    double sum = 0;
                    for (j = 0; j < J; j++)
                        sum += vec[j] * rhs._MatrixData[j, i];

                    output[i] = sum;
                }

                return output;
            }
        }

        #endregion Vector Operators

        #region double[] Operators

        /// <summary>
        /// Multiplies a native double vector by a matrix
        /// </summary>
        /// <param name="lhs">Matrix object</param>
        /// <param name="rhs">Native double vector</param>
        /// <returns>lhs*rhs</returns>
        public static double[] operator *(MathLib.Matrix lhs, double[] rhs)
        {
            if (lhs.Width != rhs.Length)
            {
                throw new ArgumentException("lhs Width != rhs Height");
            }
            else
            {
                double[] vec = new double[lhs.Height];
                int i = 0, I = vec.Length;
                int k = 0, K = lhs.Width;
                for (i = 0; i < I; i++)
                {
                    for (k = 0; k < K; k++)
                    {
                        vec[i] += lhs._MatrixData[i, k] * rhs[k];
                    }
                }
                return vec;
            }
        }

        /// <summary>
        /// Multiplication by a native vector without having to use casts. Returns as a double[]
        /// </summary>
        /// <param name="lhs">native double vector</param>
        /// <param name="rhs">Matrix class</param>
        /// <returns>lhs*rhs</returns>
        public static double[] operator *(double[] lhs, MathLib.Matrix rhs)
        {
            if (rhs.Height != lhs.Length)
            {
                throw new ArgumentException("lhs Height != rhs Width");
            }
            else
            {
                double[] vec = new double[rhs.Height];
                int i = 0, I = vec.Length;
                int k = 0, K = rhs.Height;
                for (; i < I; i++)
                {
                    double sum = 0;
                    for (k = 0; k < K; k++)
                    {
                        sum += lhs[k] * rhs._MatrixData[k, i];
                    }
                    vec[i] = sum;
                }
                return vec;
            }
        }

        #endregion double[] Operators

        #endregion Operator Overrides

        #region NewOperators

        /// <summary>
        /// Element by element multiplication
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public Matrix oDot(Matrix rhs)
        {
            if ((this.Width != rhs.Width) || (this.Height != rhs.Height))
            {
                throw new ArgumentException("this Dimensions != rhs Dimensions");
            }
            else
            {
                MathLib.Matrix output = new Matrix(rhs.Height, rhs.Width);

                for (int i = 0; i < rhs.Height; i++)
                {
                    for (int j = 0; j < rhs.Width; j++)
                    {
                        output[i, j] = this[i, j] * rhs[i, j];
                    }
                }

                return output;
            }
        }

        /// <summary>
        /// Element by element division
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public Matrix oDiv(Matrix rhs)
        {
            if ((this.Width != rhs.Width) || (this.Height != rhs.Height))
            {
                throw new ArgumentException("this Dimensions != rhs Dimensions");
            }
            else
            {
                MathLib.Matrix output = new Matrix(rhs.Height, rhs.Width);

                for (int i = 0; i < rhs.Height; i++)
                {
                    for (int j = 0; j < rhs.Width; j++)
                    {
                        output[i, j] = this[i, j] / rhs[i, j];
                    }
                }

                return output;
            }
        }

        #endregion NewOperators

        #region Casts

        /// <summary>
        /// Casts the matrix as a 2D double array
        /// </summary>
        /// <param name="lhs"></param>
        /// <returns>2D double array</returns>
        public static explicit operator double[,] (Matrix lhs)
        {
            return lhs._MatrixData;
        }

        public static explicit operator double[] (Matrix lhs)
        {
            Matrix m;
            if (lhs.Width == 1)
                m = lhs;
            else if (lhs.Height == 1)
                m = lhs.Transpose;
            else
            {
                throw new ApplicationException("Invalid cast of 2-D matrix to 1-D vector");
            }

            double[] vec = new double[m.Height];
            for (int i = 0; i < m.Height; i++)
            {
                vec[i] = m[i];
            }
            return vec;
        }

        public static explicit operator Matrix(double[,] lhs)
        {
            return new Matrix(lhs);
        }

        public static explicit operator Matrix(int[,] lhs)
        {
            return new Matrix(lhs);
        }

        public static explicit operator Matrix(double[] lhs)
        {
            return new Matrix(lhs);
        }

        public static explicit operator Matrix(int[] lhs)
        {
            return new Matrix(lhs);
        }

        #endregion Casts

        #region Static Methods

        /// <summary>
        /// Returns the unit vector
        /// </summary>
        /// <param name="Dimension">Height of the unit vector</param>
        /// <returns>Unit vector</returns>
        public static Matrix UnitVector(int Dimension)
        {
            return Matrix.UnitVector(Dimension, 1);
        }

        /// <summary>
        /// Returns the unit vector
        /// </summary>
        /// <param name="Dimension">Height of the unit vector</param>
        /// <param name="value">The value to which the vector is initialised</param>
        /// <returns>Unit vector</returns>
        public static Matrix UnitVector(int Dimension, double value)
        {
            Matrix output = new Matrix(Dimension, 1);

            for (int i = 0; i < Dimension; i++)
                output._MatrixData[i, 0] = value;
            return output;
        }

        /// <summary>
        /// Create a Diagonal Matrix
        /// </summary>
        /// <param name="Dimension">Height and width of matrix returned</param>
        /// <param name="value">Value placed on the diagonal</param>
        /// <returns></returns>
        public static Matrix Diagonal(int Dimension, double value)
        {
            Matrix output = new Matrix(Dimension, Dimension);

            for (int i = 0; i < Dimension; i++)
                output._MatrixData[i, i] = value;
            return output;
        }

        /// <summary>
        /// Create a rectangular Diagonal matrix.
        /// </summary>
        /// <param name="Height">Height of matrix</param>
        /// <param name="Width">Width of matrix</param>
        /// <param name="value">Value to set on the diagonal</param>
        /// <returns>A matrix with elements set to a_ij = d_ij * c where d_ij is the kronecker delta</returns>
        public static Matrix Diagonal(int Height, int Width, double value)
        {
            Matrix output = new Matrix(Height, Width);
            int MinDimension = Math.Min(Height, Width);
            for (int i = 0; i < MinDimension; i++)
                output._MatrixData[i, i] = value;

            return output;
        }

        public static Matrix Diagonalise(double[] values)
        {
            Matrix S = new Matrix(values.Length, values.Length);
            for (int i = 0; i < values.Length; i++)
                S[i, i] = values[i];
            return S;
        }

        private static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + r * r);
            }

            if (b != 0)
            {
                double r = a / b;
                return Math.Abs(b) * Math.Sqrt(1 + r * r);
            }

            return 0.0;
        }

        #endregion Static Methods

        #region LUDecomposition

        private class LUDecomposition
        {
            #region Class Parameters

            private Matrix LU;
            private int pivotSign;
            private int[] pivotVector;

            #endregion Class Parameters

            #region Constructor

            public LUDecomposition(Matrix input)
            {
                int height, width;
                int i, j, k, p, kmax;
                double s;

                LU = (Matrix)input.Clone();
                height = input.Height;
                width = input.Width;

                //get pointer to matrix data to speed up calculations
                double[,] lu_data = LU._MatrixData;

                pivotVector = new int[height];
                for (i = 0; i < height; i++) pivotVector[i] = i;
                pivotSign = 1;

                // Outer loop.
                for (j = 0; j < width; j++)
                {
                    // Make a copy of the j-th column to localize references.
                    double[] LUcolj = (double[])LU.GetColumn(j);

                    // Apply previous transformations.
                    for (i = 0; i < height; i++)
                    {
                        // Most of the time is spent in the following dot product.
                        kmax = Math.Min(i, j);

                        s = 0.0;
                        for (k = 0; k < kmax; k++) s += lu_data[i, k] * LUcolj[k];

                        lu_data[i, j] = LUcolj[i] -= s;
                    }

                    // Find pivot and exchange if necessary.
                    p = j;
                    for (i = j + 1; i < height; i++)
                        if (Math.Abs(LUcolj[i]) > Math.Abs(LUcolj[p]))
                            p = i;

                    if (p != j)
                    {
                        for (k = 0; k < width; k++)
                        {
                            s = lu_data[p, k];
                            lu_data[p, k] = lu_data[j, k];
                            lu_data[j, k] = s;
                        }

                        k = pivotVector[p];
                        pivotVector[p] = pivotVector[j];
                        pivotVector[j] = k;

                        pivotSign = -pivotSign;
                    }

                    // Compute multipliers.

                    if (j < height & lu_data[j, j] != 0.0)
                    {
                        double temp = lu_data[j, j];
                        for (i = j + 1; i < height; i++)
                        {
                            lu_data[i, j] /= temp;
                        }
                    }
                }
            }

            #endregion Constructor

            #region Public Properties

            public bool IsNonSingular
            {
                get
                {
                    int j, J = LU.Width;

                    for (j = 0; j < J; j++)
                    {
                        if (LU[j, j] == 0) return false;
                    }

                    return true;
                }
            }

            public double Determinant
            {
                get
                {
                    if (LU.Height != LU.Width) throw new ArgumentException("Matrix must be square.");

                    int j, J = LU.Height;
                    double determinant = (double)pivotSign;
                    for (j = 0; j < J; j++)
                    {
                        determinant *= LU[j, j];
                    }

                    return determinant;
                }
            }

            public Matrix LowerTriangularFactor
            {
                get
                {
                    int rows = LU.Height;
                    int columns = LU.Width;

                    Matrix X = new Matrix(rows, columns);

                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < columns; j++)
                            if (i > j)
                                X[i, j] = LU[i, j];
                            else if (i == j)
                                X[i, j] = 1.0;
                            else
                                X[i, j] = 0.0;

                    return X;
                }
            }

            public Matrix UpperTriangularFactor
            {
                get
                {
                    int rows = LU.Height;
                    int columns = LU.Width;

                    Matrix X = new Matrix(rows, columns);

                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < columns; j++)
                            if (i <= j)
                                X[i, j] = LU[i, j];
                            else
                                X[i, j] = 0.0;

                    return X;
                }
            }

            #endregion Public Properties

            #region Solve

            public Matrix Solve(Matrix B)
            {
                if (B.Height != LU.Height) throw new ArgumentException("Invalid matrix dimensions.");
                if (!this.IsNonSingular) throw new InvalidOperationException("Matrix is singular");

                // Copy right hand side with pivoting
                int count = B.Width;
                Matrix Res = B.Submatrix(pivotVector, 0, count - 1);

                //get pointers to matrix data to speed up computations
                double[,] X = Res._MatrixData;
                double[,] lu_data = LU._MatrixData;

                int rows = LU.Height;
                int columns = LU.Width;

                // Solve L*Y = B(piv,:)
                for (int k = 0; k < columns; k++)
                {
                    for (int i = k + 1; i < columns; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            X[i, j] -= X[k, j] * lu_data[i, k];
                        }
                    }
                }

                // Solve U*X = Y;
                for (int k = columns - 1; k >= 0; k--)
                {
                    for (int j = 0; j < count; j++)
                    {
                        X[k, j] /= lu_data[k, k];
                    }

                    for (int i = 0; i < k; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            X[i, j] -= X[k, j] * lu_data[i, k];
                        }
                    }
                }

                return Res;
            }

            #endregion Solve
        }

        #endregion LUDecomposition

        #region QRDecomposition

        private class QRDecomposition
        {
            #region Class Properties

            private Matrix QR;
            private double[] Rdiag;

            #endregion Class Properties

            #region Constructor

            public QRDecomposition(Matrix A)
            {
                int m, n, i, k, j;
                double s;
                double nrm;

                QR = (Matrix)A.Clone();
                m = A.Height;
                n = A.Width;
                Rdiag = new double[n];

                for (k = 0; k < n; k++)
                {
                    // Compute 2-norm of k-th column without under/overflow.
                    nrm = 0;

                    for (i = k; i < m; i++)
                    {
                        nrm = MathLib.Matrix.Hypotenuse(nrm, QR[i, k]);
                    }

                    if (nrm != 0.0)
                    {
                        // Form k-th Householder vector.
                        if (QR[k, k] < 0) nrm = -nrm;

                        for (i = k; i < m; i++)
                        {
                            QR[i, k] /= nrm;
                        }

                        QR[k, k] += 1.0;

                        // Apply transformation to remaining columns.
                        for (j = k + 1; j < n; j++)
                        {
                            s = 0.0;

                            for (i = k; i < m; i++)
                            {
                                s += QR[i, k] * QR[i, j];
                            }

                            s = -s / QR[k, k];

                            for (i = k; i < m; i++)
                            {
                                QR[i, j] += s * QR[i, k];
                            }
                        }
                    }

                    Rdiag[k] = -nrm;
                }
            }

            #endregion Constructor

            #region Public Properties

            public bool IsFullRank
            {
                get
                {
                    int j;
                    int columns = QR.Width;

                    for (j = 0; j < columns; j++)
                    {
                        if (Rdiag[j] == 0) return false;
                    }

                    return true;
                }
            }

            public Matrix UpperTriangularFactor
            {
                get
                {
                    int i, j, n;
                    Matrix X;

                    n = QR.Width;
                    X = new Matrix(n, n);

                    for (i = 0; i < n; i++)
                        for (j = 0; j < n; j++)
                            if (i < j)
                                X[i, j] = QR[i, j];
                            else if (i == j)
                                X[i, j] = Rdiag[i];
                            else
                                X[i, j] = 0.0;

                    return X;
                }
            }

            public Matrix OrthogonalFactor
            {
                get
                {
                    int i, j, k;
                    double s;
                    Matrix X;

                    X = new Matrix(QR.Height, QR.Width);

                    int K = QR.Width;
                    int I = QR.Height;
                    for (k = K - 1; k >= 0; k--)
                    {
                        for (i = 0; i < I; i++)
                        {
                            X[i, k] = 0.0;
                        }

                        X[k, k] = 1.0;
                        for (j = k; j < K; j++)
                        {
                            if (QR[k, k] != 0)
                            {
                                s = 0.0;

                                for (i = k; i < I; i++)
                                {
                                    s += QR[i, k] * X[i, j];
                                }

                                s = -s / QR[k, k];

                                for (i = k; i < I; i++)
                                {
                                    X[i, j] += s * QR[i, k];
                                }
                            }
                        }
                    }
                    return X;
                }
            }

            #endregion Public Properties

            #region Solve

            public Matrix Solve(Matrix rhs)
            {
                int count, m, n, i, j, k;
                double s;
                Matrix X;

                if (rhs.Height != QR.Height) throw new ArgumentException("Matrix row dimensions must agree.");
                if (!this.IsFullRank) throw new InvalidOperationException("Matrix is rank deficient.");

                // Copy right hand side
                count = rhs.Width;
                X = (Matrix)rhs.Clone();
                m = QR.Height;
                n = QR.Width;

                // Compute Y = transpose(Q)*B
                for (k = 0; k < n; k++)
                {
                    for (j = 0; j < count; j++)
                    {
                        s = 0.0;

                        for (i = k; i < m; i++)
                        {
                            s += QR[i, k] * X[i, j];
                        }

                        s = -s / QR[k, k];

                        for (i = k; i < m; i++)
                        {
                            X[i, j] += s * QR[i, k];
                        }
                    }
                }

                // Solve R*X = Y;
                for (k = n - 1; k >= 0; k--)
                {
                    for (j = 0; j < count; j++)
                    {
                        X[k, j] /= Rdiag[k];
                    }

                    for (i = 0; i < k; i++)
                    {
                        for (j = 0; j < count; j++)
                        {
                            X[i, j] -= X[k, j] * QR[i, k];
                        }
                    }
                }

                return X.Submatrix(0, n - 1, 0, count - 1);
            }

            #endregion Solve
        }

        #endregion QRDecomposition

        #region SVD Decomposition

        private class SVDDecomposition
        {
            public SVDDecomposition(Matrix A)
            {
                // do the decomposition
                Matrix v, a;
                a = (Matrix)A.Clone();

                double[] w;
                bool flag;
                int i, its, j, jj, k, l = 0, nm = 0;
                double anorm, c, f, g, h, s, scale, x, y, z;

                int m = a.Height;
                int n = a.Width;

                w = new double[n];
                v = new Matrix(n, n);
                double[] rv1 = new double[n];
                g = scale = anorm = 0.0;
                for (i = 0; i < n; i++)
                {
                    l = i + 2;
                    rv1[i] = scale * g;
                    g = s = scale = 0.0;
                    if (i < m)
                    {
                        for (k = i; k < m; k++)
                            scale += Math.Abs(a[k, i]);
                        if (scale != 0.0)
                        {
                            for (k = i; k < m; k++)
                            {
                                a[k, i] /= scale;
                                s += a[k, i] * a[k, i];
                            }
                            f = a[i, i];
                            g = -SIGN(Math.Sqrt(s), f);
                            h = f * g - s;
                            a[i, i] = f - g;
                            for (j = l - 1; j < n; j++)
                            {
                                for (s = 0.0, k = i; k < m; k++)
                                    s += a[k, i] * a[k, j];
                                f = s / h;
                                for (k = i; k < m; k++)
                                    a[k, j] += f * a[k, i];
                            }
                            for (k = i; k < m; k++)
                                a[k, i] *= scale;
                        }
                    }
                    w[i] = scale * g;
                    g = s = scale = 0.0;
                    if (i + 1 <= m && i != n)
                    {
                        for (k = l - 1; k < n; k++)
                            scale += Math.Abs(a[i, k]);
                        if (scale != 0.0)
                        {
                            for (k = l - 1; k < n; k++)
                            {
                                a[i, k] /= scale;
                                s += a[i, k] * a[i, k];
                            }
                            f = a[i, l - 1];
                            g = -SIGN(Math.Sqrt(s), f);
                            h = f * g - s;
                            a[i, l - 1] = f - g;
                            for (k = l - 1; k < n; k++)
                                rv1[k] = a[i, k] / h;
                            for (j = l - 1; j < m; j++)
                            {
                                for (s = 0.0, k = l - 1; k < n; k++)
                                    s += a[j, k] * a[i, k];
                                for (k = l - 1; k < n; k++)
                                    a[j, k] += s * rv1[k];
                            }
                            for (k = l - 1; k < n; k++)
                                a[i, k] *= scale;
                        }
                    }
                    anorm = Math.Max(anorm, (Math.Abs(w[i]) + Math.Abs(rv1[i])));
                }
                for (i = n - 1; i >= 0; i--)
                {
                    if (i < n - 1)
                    {
                        if (g != 0.0)
                        {
                            for (j = l; j < n; j++)
                                v[j, i] = (a[i, j] / a[i, l]) / g;
                            for (j = l; j < n; j++)
                            {
                                for (s = 0.0, k = l; k < n; k++)
                                    s += a[i, k] * v[k, j];
                                for (k = l; k < n; k++)
                                    v[k, j] += s * v[k, i];
                            }
                        }
                        for (j = l; j < n; j++)
                            v[i, j] = v[j, i] = 0.0;
                    }
                    v[i, i] = 1.0;
                    g = rv1[i];
                    l = i;
                }
                for (i = Math.Min(m, n) - 1; i >= 0; i--)
                {
                    l = i + 1;
                    g = w[i];
                    for (j = l; j < n; j++)
                        a[i, j] = 0.0;
                    if (g != 0.0)
                    {
                        g = 1.0 / g;
                        for (j = l; j < n; j++)
                        {
                            for (s = 0.0, k = l; k < m; k++)
                                s += a[k, i] * a[k, j];
                            f = (s / a[i, i]) * g;
                            for (k = i; k < m; k++)
                                a[k, j] += f * a[k, i];
                        }
                        for (j = i; j < m; j++)
                            a[j, i] *= g;
                    }
                    else
                        for (j = i; j < m; j++)
                            a[j, i] = 0.0;
                    ++a[i, i];
                }
                for (k = n - 1; k >= 0; k--)
                {
                    for (its = 0; its < 30; its++)
                    {
                        flag = true;
                        for (l = k; l >= 0; l--)
                        {
                            nm = l - 1;
                            if (Math.Abs(rv1[l]) + anorm == anorm)
                            {
                                flag = false;
                                break;
                            }
                            if (Math.Abs(w[nm]) + anorm == anorm)
                                break;
                        }
                        if (flag)
                        {
                            c = 0.0;
                            s = 1.0;
                            for (i = l - 1; i < k + 1; i++)
                            {
                                f = s * rv1[i];
                                rv1[i] = c * rv1[i];
                                if (Math.Abs(f) + anorm == anorm)
                                    break;
                                g = w[i];
                                h = pythag(f, g);
                                w[i] = h;
                                h = 1.0 / h;
                                c = g * h;
                                s = -f * h;
                                for (j = 0; j < m; j++)
                                {
                                    y = a[j, nm];
                                    z = a[j, i];
                                    a[j, nm] = y * c + z * s;
                                    a[j, i] = z * c - y * s;
                                }
                            }
                        }
                        z = w[k];
                        if (l == k)
                        {
                            if (z < 0.0)
                            {
                                w[k] = -z;
                                for (j = 0; j < n; j++)
                                    v[j, k] = -v[j, k];
                            }
                            break;
                        }
                        if (its == 29)
                            throw new ApplicationException("no convergence in 30 svdcmp iterations");
                        x = w[l];
                        nm = k - 1;
                        y = w[nm];
                        g = rv1[nm];
                        h = rv1[k];
                        f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
                        g = pythag(f, 1.0);

                        f = ((x - z) * (x + z) + h * ((y / (f + SIGN(g, f))) - h)) / x;

                        c = s = 1.0;
                        for (j = l; j <= nm; j++)
                        {
                            i = j + 1;
                            g = rv1[i];
                            y = w[i];
                            h = s * g;
                            g = c * g;
                            z = pythag(f, h);
                            rv1[j] = z;
                            c = f / z;
                            s = h / z;
                            f = x * c + g * s;
                            g = g * c - x * s;
                            h = y * s;
                            y *= c;
                            for (jj = 0; jj < n; jj++)
                            {
                                x = v[jj, j];
                                z = v[jj, i];
                                v[jj, j] = x * c + z * s;
                                v[jj, i] = z * c - x * s;
                            }
                            z = pythag(f, h);
                            w[j] = z;
                            if (z != 0)
                            {
                                z = 1.0 / z;
                                c = f * z;
                                s = h * z;
                            }
                            f = c * g + s * y;
                            x = c * y - s * g;
                            for (jj = 0; jj < m; jj++)
                            {
                                y = a[jj, j];
                                z = a[jj, i];
                                a[jj, j] = y * c + z * s;
                                a[jj, i] = z * c - y * s;
                            }
                        }
                        rv1[l] = 0.0;
                        rv1[k] = f;
                        w[k] = x;
                    }
                }

                // sort the w into ascending order
                int[] indices = new int[n];
                for (i = 0; i < n; i++)
                    indices[i] = i;

                Array.Sort(w, indices);
                Array.Reverse(w);
                Array.Reverse(indices);

                _U = (Matrix)a.Clone();
                _V = (Matrix)v.Clone();

                // for each singular value ...
                for (i = 0; i < n; i++)
                {
                    // no need to do anything if this condition is not sayisfied
                    if (indices[i] != i)
                    {
                        for (j = 0; j < m; j++)
                            _U[j, i] = a[j, indices[i]];
                        for (j = 0; j < n; j++)
                            //_V[i,j] = v[indices[i],j];
                            _V[j, i] = v[j, indices[i]];
                    }
                }
                _S = w;
            }

            private double pythag(double a, double b)
            {
                double absa, absb;

                absa = Math.Abs(a);
                absb = Math.Abs(b);
                if (absa > absb)
                    return absa * Math.Sqrt(1.0 + (absb / absa) * (absb / absa));
                else
                    return (absb == 0.0 ? 0.0 : absb * Math.Sqrt(1.0 + (absa / absb) * (absa / absb)));
            }

            private double SIGN(double a, double b)
            {
                return b >= 0 ? (a >= 0 ? a : -1) : (a >= 0 ? -a : a);
            }

            private Matrix _V, _U;
            private double[] _S;

            public double[] S
            {
                get
                {
                    return _S;
                }
            }

            public Matrix V
            {
                get
                {
                    return _V;
                }
            }

            public Matrix U
            {
                get
                {
                    return _U;
                }
            }
        }

        #endregion SVD Decomposition

        public Matrix CholeskyLowerTriangularFactor
        {
            get
            {
                if (CholeskyDecomp == null) CholeskyDecomp = new CholeskyDecomposition(this);
                return CholeskyDecomp.L;
            }
        }

        private class CholeskyDecomposition
        {
            private Matrix _cholesky;

            public CholeskyDecomposition(Matrix A)
            {
                int i, j, k;
                double sum;
                A = (Matrix)A.Clone();
                int n = A.Height;
                _cholesky = new Matrix(n, n);

                for (i = 0; i < n; i++)
                {
                    for (j = i; j < n; j++)
                    {
                        sum = A[i, j];
                        for (k = i - 1; k >= 0; k--)
                            sum -= _cholesky[i, k] * _cholesky[j, k];
                        if (i == j)
                        {
                            if (sum <= 0.0)
                                throw new ApplicationException("Cholesky Decomposition Failed (Matrix is not positive definite)");

                            _cholesky[i, i] = Math.Sqrt(sum);
                        }
                        else
                        {
                            //A[j,i]=sum/_cholesky[i,i];
                            _cholesky[j, i] = sum / _cholesky[i, i];
                        }
                    }
                }
            }

            public Matrix L
            {
                get
                {
                    return _cholesky;
                }
            }

            public Matrix U
            {
                get
                {
                    return _cholesky.Transpose;
                }
            }
        }

        #region EigenDecomposition

        private class EigenvalueDecomposition
        {
            private int n;           	// matrix dimension
            private double[] d, e; 		// storage of eigenvalues.
            private Matrix V; 			// storage of eigenvectors.
            private Matrix H;  			// storage of nonsymmetric Hessenberg form.
            private double[] ort;    	// storage for nonsymmetric algorithm.
            private double cdivr, cdivi;
            private bool isSymmetric;

            public EigenvalueDecomposition(Matrix A, bool AssumeSymmetric)
            {
                if (A.Width != A.Height) throw new ArgumentException("Matrix is not a square matrix.");

                n = A.Height;
                V = new Matrix(n, n);
                d = new double[n];
                e = new double[n];

                // Check for symmetry.
                if (AssumeSymmetric == false)
                {
                    throw new ArgumentException("This constructor only takes the case where we assume symmetry");
                }
                isSymmetric = AssumeSymmetric;

                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        V[i, j] = A[i, j];

                // Tridiagonalize.
                tred2();
                // Diagonalize.
                tql2();
            }

            public EigenvalueDecomposition(Matrix A)
            {
                if (A.Width != A.Height) throw new ArgumentException("Matrix is not a square matrix.");

                n = A.Height;
                V = new Matrix(n, n);
                d = new double[n];
                e = new double[n];

                // Check for symmetry.
                isSymmetric = A.IsSymmetric;

                if (isSymmetric)
                {
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                            V[i, j] = A[i, j];

                    // Tridiagonalize.
                    tred2();
                    // Diagonalize.
                    tql2();
                }
                else
                {
                    H = new Matrix(n, n);
                    ort = new double[n];

                    for (int j = 0; j < n; j++)
                        for (int i = 0; i < n; i++)
                            H[i, j] = A[i, j];

                    // Reduce to Hessenberg form.
                    orthes();

                    // Reduce Hessenberg to real Schur form.
                    hqr2();
                }
            }

            private void tred2()
            {
                // Symmetric Householder reduction to tridiagonal form.
                // This is derived from the Algol procedures tred2 by Bowdler, Martin, Reinsch, and Wilkinson,
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutine in EISPACK.

                // NOTE: This algorithm is also similar to the identically named routine in Numerical Recipees 11.2

                // (The idea is to reduce the input matrix to a simpler form, and then find the eigenvalues of the simpler tridiagonal form. Having found
                // them we can transform back using the housholder rotation in reverse)

                for (int j = 0; j < n; j++)
                    d[j] = V[n - 1, j];

                // Householder reduction to tridiagonal form.
                for (int i = n - 1; i > 0; i--)
                {
                    // Scale to avoid under/overflow.
                    double scale = 0.0;
                    double h = 0.0;
                    for (int k = 0; k < i; k++)
                        scale = scale + Math.Abs(d[k]);

                    if (scale == 0.0)
                    {
                        e[i] = d[i - 1];
                        for (int j = 0; j < i; j++)
                        {
                            d[j] = V[i - 1, j];
                            V[i, j] = 0.0;
                            V[j, i] = 0.0;
                        }
                    }
                    else
                    {
                        // Generate Householder vector.
                        for (int k = 0; k < i; k++)
                        {
                            d[k] /= scale;
                            h += d[k] * d[k];
                        }

                        double f = d[i - 1];
                        double g = Math.Sqrt(h);
                        if (f > 0) g = -g;

                        e[i] = scale * g;
                        h = h - f * g;
                        d[i - 1] = f - g;
                        for (int j = 0; j < i; j++)
                            e[j] = 0.0;

                        // Apply similarity transformation to remaining columns.
                        for (int j = 0; j < i; j++)
                        {
                            f = d[j];
                            V[j, i] = f;
                            g = e[j] + V[j, j] * f;
                            for (int k = j + 1; k <= i - 1; k++)
                            {
                                g += V[k, j] * d[k];
                                e[k] += V[k, j] * f;
                            }
                            e[j] = g;
                        }

                        f = 0.0;
                        for (int j = 0; j < i; j++)
                        {
                            e[j] /= h;
                            f += e[j] * d[j];
                        }

                        double hh = f / (h + h);
                        for (int j = 0; j < i; j++)
                            e[j] -= hh * d[j];

                        for (int j = 0; j < i; j++)
                        {
                            f = d[j];
                            g = e[j];
                            for (int k = j; k <= i - 1; k++)
                                V[k, j] -= (f * e[k] + g * d[k]);

                            d[j] = V[i - 1, j];
                            V[i, j] = 0.0;
                        }
                    }
                    d[i] = h;
                }

                // Accumulate transformations.
                for (int i = 0; i < n - 1; i++)
                {
                    V[n - 1, i] = V[i, i];
                    V[i, i] = 1.0;
                    double h = d[i + 1];
                    if (h != 0.0)
                    {
                        for (int k = 0; k <= i; k++)
                            d[k] = V[k, i + 1] / h;

                        for (int j = 0; j <= i; j++)
                        {
                            double g = 0.0;
                            for (int k = 0; k <= i; k++)
                                g += V[k, i + 1] * V[k, j];
                            for (int k = 0; k <= i; k++)
                                V[k, j] -= g * d[k];
                        }
                    }

                    for (int k = 0; k <= i; k++)
                        V[k, i + 1] = 0.0;
                }

                for (int j = 0; j < n; j++)
                {
                    d[j] = V[n - 1, j];
                    V[n - 1, j] = 0.0;
                }

                V[n - 1, n - 1] = 1.0;
                e[0] = 0.0;
            }

            private void tql2()
            {
                // Symmetric tridiagonal QL algorithm.
                // This is derived from the Algol procedures tql2, by Bowdler, Martin, Reinsch, and Wilkinson,
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutine in EISPACK.
                for (int i = 1; i < n; i++)
                    e[i - 1] = e[i];

                e[n - 1] = 0.0;

                double f = 0.0;
                double tst1 = 0.0;
                double eps = Math.Pow(2.0, -52.0);

                for (int l = 0; l < n; l++)
                {
                    // Find small subdiagonal element.
                    tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                    int m = l;
                    while (m < n)
                    {
                        if (Math.Abs(e[m]) <= eps * tst1)
                            break;
                        m++;
                    }

                    // If m == l, d[l] is an eigenvalue, otherwise, iterate.
                    if (m > l)
                    {
                        int iter = 0;
                        do
                        {
                            iter = iter + 1;  // (Could check iteration count here.)

                            // Compute implicit shift
                            double g = d[l];
                            double p = (d[l + 1] - g) / (2.0 * e[l]);
                            double r = MathLib.Matrix.Hypotenuse(p, 1.0);
                            if (p < 0) r = -r;

                            d[l] = e[l] / (p + r);
                            d[l + 1] = e[l] * (p + r);
                            double dl1 = d[l + 1];
                            double h = g - d[l];
                            for (int i = l + 2; i < n; i++)
                                d[i] -= h;
                            f = f + h;

                            // Implicit QL transformation.
                            p = d[m];
                            double c = 1.0;
                            double c2 = c;
                            double c3 = c;
                            double el1 = e[l + 1];
                            double s = 0.0;
                            double s2 = 0.0;
                            for (int i = m - 1; i >= l; i--)
                            {
                                c3 = c2;
                                c2 = c;
                                s2 = s;
                                g = c * e[i];
                                h = c * p;
                                r = MathLib.Matrix.Hypotenuse(p, e[i]);
                                e[i + 1] = s * r;
                                s = e[i] / r;
                                c = p / r;
                                p = c * d[i] - s * g;
                                d[i + 1] = h + s * (c * g + s * d[i]);

                                // Accumulate transformation.
                                for (int k = 0; k < n; k++)
                                {
                                    h = V[k, i + 1];
                                    V[k, i + 1] = s * V[k, i] + c * h;
                                    V[k, i] = c * V[k, i] - s * h;
                                }
                            }

                            p = -s * s2 * c3 * el1 * e[l] / dl1;
                            e[l] = s * p;
                            d[l] = c * p;

                            // Check for convergence.
                        }
                        while (Math.Abs(e[l]) > eps * tst1);
                    }
                    d[l] = d[l] + f;
                    e[l] = 0.0;
                }

                // Sort eigenvalues and corresponding vectors.
                for (int i = 0; i < n - 1; i++)
                {
                    int k = i;
                    double p = d[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        if (d[j] < p)
                        {
                            k = j;
                            p = d[j];
                        }
                    }

                    if (k != i)
                    {
                        d[k] = d[i];
                        d[i] = p;
                        for (int j = 0; j < n; j++)
                        {
                            p = V[j, i];
                            V[j, i] = V[j, k];
                            V[j, k] = p;
                        }
                    }
                }
            }

            private void orthes()
            {
                // Nonsymmetric reduction to Hessenberg form.
                // This is derived from the Algol procedures orthes and ortran, by Martin and Wilkinson,
                // Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding Fortran subroutines in EISPACK.
                int low = 0;
                int high = n - 1;

                for (int m = low + 1; m <= high - 1; m++)
                {
                    // Scale column.

                    double scale = 0.0;
                    for (int i = m; i <= high; i++)
                        scale = scale + Math.Abs(H[i, m - 1]);

                    if (scale != 0.0)
                    {
                        // Compute Householder transformation.
                        double h = 0.0;
                        for (int i = high; i >= m; i--)
                        {
                            ort[i] = H[i, m - 1] / scale;
                            h += ort[i] * ort[i];
                        }

                        double g = Math.Sqrt(h);
                        if (ort[m] > 0) g = -g;

                        h = h - ort[m] * g;
                        ort[m] = ort[m] - g;

                        // Apply Householder similarity transformation
                        // H = (I - u * u' / h) * H * (I - u * u') / h)
                        for (int j = m; j < n; j++)
                        {
                            double f = 0.0;
                            for (int i = high; i >= m; i--)
                                f += ort[i] * H[i, j];

                            f = f / h;
                            for (int i = m; i <= high; i++)
                                H[i, j] -= f * ort[i];
                        }

                        for (int i = 0; i <= high; i++)
                        {
                            double f = 0.0;
                            for (int j = high; j >= m; j--)
                                f += ort[j] * H[i, j];

                            f = f / h;
                            for (int j = m; j <= high; j++)
                                H[i, j] -= f * ort[j];
                        }

                        ort[m] = scale * ort[m];
                        H[m, m - 1] = scale * g;
                    }
                }

                // Accumulate transformations (Algol's ortran).
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        V[i, j] = (i == j ? 1.0 : 0.0);

                for (int m = high - 1; m >= low + 1; m--)
                {
                    if (H[m, m - 1] != 0.0)
                    {
                        for (int i = m + 1; i <= high; i++)
                            ort[i] = H[i, m - 1];

                        for (int j = m; j <= high; j++)
                        {
                            double g = 0.0;
                            for (int i = m; i <= high; i++)
                                g += ort[i] * V[i, j];

                            // Double division avoids possible underflow.
                            g = (g / ort[m]) / H[m, m - 1];
                            for (int i = m; i <= high; i++)
                                V[i, j] += g * ort[i];
                        }
                    }
                }
            }

            private void cdiv(double xr, double xi, double yr, double yi)
            {
                // Complex scalar division.
                double r, d;
                if (Math.Abs(yr) > Math.Abs(yi))
                {
                    r = yi / yr;
                    d = yr + r * yi;
                    cdivr = (xr + r * xi) / d;
                    cdivi = (xi - r * xr) / d;
                }
                else
                {
                    r = yr / yi;
                    d = yi + r * yr;
                    cdivr = (r * xr + xi) / d;
                    cdivi = (r * xi - xr) / d;
                }
            }

            private void hqr2()
            {
                // Nonsymmetric reduction from Hessenberg to real Schur form.
                // This is derived from the Algol procedure hqr2, by Martin and Wilkinson, Handbook for Auto. Comp.,
                // Vol.ii-Linear Algebra, and the corresponding  Fortran subroutine in EISPACK.
                int nn = this.n;
                int n = nn - 1;
                int low = 0;
                int high = nn - 1;
                double eps = Math.Pow(2.0, -52.0);
                double exshift = 0.0;
                double p = 0, q = 0, r = 0, s = 0, z = 0, t, w, x, y;

                // Store roots isolated by balanc and compute matrix norm
                double norm = 0.0;
                for (int i = 0; i < nn; i++)
                {
                    if (i < low | i > high)
                    {
                        d[i] = H[i, i];
                        e[i] = 0.0;
                    }

                    for (int j = Math.Max(i - 1, 0); j < nn; j++)
                        norm = norm + Math.Abs(H[i, j]);
                }

                // Outer loop over eigenvalue index
                int iter = 0;
                while (n >= low)
                {
                    // Look for single small sub-diagonal element
                    int l = n;
                    while (l > low)
                    {
                        s = Math.Abs(H[l - 1, l - 1]) + Math.Abs(H[l, l]);
                        if (s == 0.0) s = norm;
                        if (Math.Abs(H[l, l - 1]) < eps * s)
                            break;

                        l--;
                    }

                    // Check for convergence
                    if (l == n)
                    {
                        // One root found
                        H[n, n] = H[n, n] + exshift;
                        d[n] = H[n, n];
                        e[n] = 0.0;
                        n--;
                        iter = 0;
                    }
                    else if (l == n - 1)
                    {
                        // Two roots found
                        w = H[n, n - 1] * H[n - 1, n];
                        p = (H[n - 1, n - 1] - H[n, n]) / 2.0;
                        q = p * p + w;
                        z = Math.Sqrt(Math.Abs(q));
                        H[n, n] = H[n, n] + exshift;
                        H[n - 1, n - 1] = H[n - 1, n - 1] + exshift;
                        x = H[n, n];

                        if (q >= 0)
                        {
                            // Real pair
                            z = (p >= 0) ? (p + z) : (p - z);
                            d[n - 1] = x + z;
                            d[n] = d[n - 1];
                            if (z != 0.0)
                                d[n] = x - w / z;
                            e[n - 1] = 0.0;
                            e[n] = 0.0;
                            x = H[n, n - 1];
                            s = Math.Abs(x) + Math.Abs(z);
                            p = x / s;
                            q = z / s;
                            r = Math.Sqrt(p * p + q * q);
                            p = p / r;
                            q = q / r;

                            // Row modification
                            for (int j = n - 1; j < nn; j++)
                            {
                                z = H[n - 1, j];
                                H[n - 1, j] = q * z + p * H[n, j];
                                H[n, j] = q * H[n, j] - p * z;
                            }

                            // Column modification
                            for (int i = 0; i <= n; i++)
                            {
                                z = H[i, n - 1];
                                H[i, n - 1] = q * z + p * H[i, n];
                                H[i, n] = q * H[i, n] - p * z;
                            }

                            // Accumulate transformations
                            for (int i = low; i <= high; i++)
                            {
                                z = V[i, n - 1];
                                V[i, n - 1] = q * z + p * V[i, n];
                                V[i, n] = q * V[i, n] - p * z;
                            }
                        }
                        else
                        {
                            // Complex pair
                            d[n - 1] = x + p;
                            d[n] = x + p;
                            e[n - 1] = z;
                            e[n] = -z;
                        }

                        n = n - 2;
                        iter = 0;
                    }
                    else
                    {
                        // No convergence yet

                        // Form shift
                        x = H[n, n];
                        y = 0.0;
                        w = 0.0;
                        if (l < n)
                        {
                            y = H[n - 1, n - 1];
                            w = H[n, n - 1] * H[n - 1, n];
                        }

                        // Wilkinson's original ad hoc shift
                        if (iter == 10)
                        {
                            exshift += x;
                            for (int i = low; i <= n; i++)
                                H[i, i] -= x;

                            s = Math.Abs(H[n, n - 1]) + Math.Abs(H[n - 1, n - 2]);
                            x = y = 0.75 * s;
                            w = -0.4375 * s * s;
                        }

                        // MATLAB's new ad hoc shift
                        if (iter == 30)
                        {
                            s = (y - x) / 2.0;
                            s = s * s + w;
                            if (s > 0)
                            {
                                s = Math.Sqrt(s);
                                if (y < x) s = -s;
                                s = x - w / ((y - x) / 2.0 + s);
                                for (int i = low; i <= n; i++)
                                    H[i, i] -= s;
                                exshift += s;
                                x = y = w = 0.964;
                            }
                        }

                        iter = iter + 1;

                        // Look for two consecutive small sub-diagonal elements
                        int m = n - 2;
                        while (m >= l)
                        {
                            z = H[m, m];
                            r = x - z;
                            s = y - z;
                            p = (r * s - w) / H[m + 1, m] + H[m, m + 1];
                            q = H[m + 1, m + 1] - z - r - s;
                            r = H[m + 2, m + 1];
                            s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            p = p / s;
                            q = q / s;
                            r = r / s;
                            if (m == l)
                                break;
                            if (Math.Abs(H[m, m - 1]) * (Math.Abs(q) + Math.Abs(r)) < eps * (Math.Abs(p) * (Math.Abs(H[m - 1, m - 1]) + Math.Abs(z) + Math.Abs(H[m + 1, m + 1]))))
                                break;
                            m--;
                        }

                        for (int i = m + 2; i <= n; i++)
                        {
                            H[i, i - 2] = 0.0;
                            if (i > m + 2)
                                H[i, i - 3] = 0.0;
                        }

                        // Double QR step involving rows l:n and columns m:n
                        for (int k = m; k <= n - 1; k++)
                        {
                            bool notlast = (k != n - 1);
                            if (k != m)
                            {
                                p = H[k, k - 1];
                                q = H[k + 1, k - 1];
                                r = (notlast ? H[k + 2, k - 1] : 0.0);
                                x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                                if (x != 0.0)
                                {
                                    p = p / x;
                                    q = q / x;
                                    r = r / x;
                                }
                            }

                            if (x == 0.0) break;

                            s = Math.Sqrt(p * p + q * q + r * r);
                            if (p < 0) s = -s;

                            if (s != 0)
                            {
                                if (k != m)
                                    H[k, k - 1] = -s * x;
                                else
                                    if (l != m)
                                    H[k, k - 1] = -H[k, k - 1];

                                p = p + s;
                                x = p / s;
                                y = q / s;
                                z = r / s;
                                q = q / p;
                                r = r / p;

                                // Row modification
                                for (int j = k; j < nn; j++)
                                {
                                    p = H[k, j] + q * H[k + 1, j];
                                    if (notlast)
                                    {
                                        p = p + r * H[k + 2, j];
                                        H[k + 2, j] = H[k + 2, j] - p * z;
                                    }

                                    H[k, j] = H[k, j] - p * x;
                                    H[k + 1, j] = H[k + 1, j] - p * y;
                                }

                                // Column modification
                                for (int i = 0; i <= Math.Min(n, k + 3); i++)
                                {
                                    p = x * H[i, k] + y * H[i, k + 1];
                                    if (notlast)
                                    {
                                        p = p + z * H[i, k + 2];
                                        H[i, k + 2] = H[i, k + 2] - p * r;
                                    }

                                    H[i, k] = H[i, k] - p;
                                    H[i, k + 1] = H[i, k + 1] - p * q;
                                }

                                // Accumulate transformations
                                for (int i = low; i <= high; i++)
                                {
                                    p = x * V[i, k] + y * V[i, k + 1];
                                    if (notlast)
                                    {
                                        p = p + z * V[i, k + 2];
                                        V[i, k + 2] = V[i, k + 2] - p * r;
                                    }

                                    V[i, k] = V[i, k] - p;
                                    V[i, k + 1] = V[i, k + 1] - p * q;
                                }
                            }
                        }
                    }
                }

                // Backsubstitute to find vectors of upper triangular form
                if (norm == 0.0) return;

                for (n = nn - 1; n >= 0; n--)
                {
                    p = d[n];
                    q = e[n];

                    // Real vector
                    if (q == 0)
                    {
                        int l = n;
                        H[n, n] = 1.0;
                        for (int i = n - 1; i >= 0; i--)
                        {
                            w = H[i, i] - p;
                            r = 0.0;
                            for (int j = l; j <= n; j++)
                                r = r + H[i, j] * H[j, n];

                            if (e[i] < 0.0)
                            {
                                z = w;
                                s = r;
                            }
                            else
                            {
                                l = i;
                                if (e[i] == 0.0)
                                {
                                    H[i, n] = (w != 0.0) ? (-r / w) : (-r / (eps * norm));
                                }
                                else
                                {
                                    // Solve real equations
                                    x = H[i, i + 1];
                                    y = H[i + 1, i];
                                    q = (d[i] - p) * (d[i] - p) + e[i] * e[i];
                                    t = (x * s - z * r) / q;
                                    H[i, n] = t;
                                    H[i + 1, n] = (Math.Abs(x) > Math.Abs(z)) ? ((-r - w * t) / x) : ((-s - y * t) / z);
                                }

                                // Overflow control
                                t = Math.Abs(H[i, n]);
                                if ((eps * t) * t > 1)
                                    for (int j = i; j <= n; j++)
                                        H[j, n] = H[j, n] / t;
                            }
                        }
                    }
                    else if (q < 0)
                    {
                        // Complex vector
                        int l = n - 1;

                        // Last vector component imaginary so matrix is triangular
                        if (Math.Abs(H[n, n - 1]) > Math.Abs(H[n - 1, n]))
                        {
                            H[n - 1, n - 1] = q / H[n, n - 1];
                            H[n - 1, n] = -(H[n, n] - p) / H[n, n - 1];
                        }
                        else
                        {
                            cdiv(0.0, -H[n - 1, n], H[n - 1, n - 1] - p, q);
                            H[n - 1, n - 1] = cdivr;
                            H[n - 1, n] = cdivi;
                        }

                        H[n, n - 1] = 0.0;
                        H[n, n] = 1.0;
                        for (int i = n - 2; i >= 0; i--)
                        {
                            double ra, sa, vr, vi;
                            ra = 0.0;
                            sa = 0.0;
                            for (int j = l; j <= n; j++)
                            {
                                ra = ra + H[i, j] * H[j, n - 1];
                                sa = sa + H[i, j] * H[j, n];
                            }

                            w = H[i, i] - p;

                            if (e[i] < 0.0)
                            {
                                z = w;
                                r = ra;
                                s = sa;
                            }
                            else
                            {
                                l = i;
                                if (e[i] == 0)
                                {
                                    cdiv(-ra, -sa, w, q);
                                    H[i, n - 1] = cdivr;
                                    H[i, n] = cdivi;
                                }
                                else
                                {
                                    // Solve complex equations
                                    x = H[i, i + 1];
                                    y = H[i + 1, i];
                                    vr = (d[i] - p) * (d[i] - p) + e[i] * e[i] - q * q;
                                    vi = (d[i] - p) * 2.0 * q;
                                    if (vr == 0.0 & vi == 0.0)
                                        vr = eps * norm * (Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                    cdiv(x * r - z * ra + q * sa, x * s - z * sa - q * ra, vr, vi);
                                    H[i, n - 1] = cdivr;
                                    H[i, n] = cdivi;
                                    if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                    {
                                        H[i + 1, n - 1] = (-ra - w * H[i, n - 1] + q * H[i, n]) / x;
                                        H[i + 1, n] = (-sa - w * H[i, n] - q * H[i, n - 1]) / x;
                                    }
                                    else
                                    {
                                        cdiv(-r - y * H[i, n - 1], -s - y * H[i, n], z, q);
                                        H[i + 1, n - 1] = cdivr;
                                        H[i + 1, n] = cdivi;
                                    }
                                }

                                // Overflow control
                                t = Math.Max(Math.Abs(H[i, n - 1]), Math.Abs(H[i, n]));
                                if ((eps * t) * t > 1)
                                    for (int j = i; j <= n; j++)
                                    {
                                        H[j, n - 1] = H[j, n - 1] / t;
                                        H[j, n] = H[j, n] / t;
                                    }
                            }
                        }
                    }
                }

                // Vectors of isolated roots
                for (int i = 0; i < nn; i++)
                    if (i < low | i > high)
                        for (int j = i; j < nn; j++)
                            V[i, j] = H[i, j];

                // Back transformation to get eigenvectors of original matrix
                for (int j = nn - 1; j >= low; j--)
                    for (int i = low; i <= high; i++)
                    {
                        z = 0.0;
                        for (int k = low; k <= Math.Min(j, high); k++)
                            z = z + V[i, k] * H[k, j];
                        V[i, j] = z;
                    }
            }

            public double[] RealEigenvalues
            {
                get { return d; }
            }

            public double[] ImaginaryEigenvalues
            {
                get { return e; }
            }

            public Matrix EigenvectorMatrix
            {
                get { return V; }
            }

            public Matrix RealEigenvector
            {
                get
                {
                    if (ReV == null) ConstructImReEigenvectors();
                    return ReV;
                }
            }

            private void ConstructImReEigenvectors()
            {
                ReV = (Matrix)V.Clone();
                ImV = new Matrix(n, n);

                if (isSymmetric)
                    return;

                for (int j = 0, i; j < n; j++)
                {
                    if (ImaginaryEigenvalues[j] != 0)
                    {
                        for (i = 0; i < n; i++)
                        {
                            // copy Re_j+1 to Im_j
                            ImV[i, j] = ReV[i, j + 1];
                            // copy -Re_j+1 to Im_j+1
                            ImV[i, j + 1] = -ReV[i, j + 1];
                            // copy Re_j to Re_j+1
                            ReV[i, j + 1] = ReV[i, j];
                        }
                        j += 1; // we can skip the next eigenvalue
                    }
                }
            }

            private Matrix ReV;
            private Matrix ImV;

            public Matrix ImaginaryEigenvector
            {
                get
                {
                    if (ImV == null) ConstructImReEigenvectors();
                    return ImV;
                }
            }

            public Matrix DiagonalMatrix
            {
                get
                {
                    Matrix X = new Matrix(n, n);

                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                            X[i, j] = 0.0;

                        X[i, i] = d[i];
                        if (e[i] > 0)
                        {
                            X[i, i + 1] = e[i];
                        }
                        else if (e[i] < 0)
                        {
                            X[i, i - 1] = e[i];
                        }
                    }

                    return X;
                }
            }
        }

        #endregion EigenDecomposition

        #region Comparison Stuff

        /// <summary>
        /// Comparison Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(Matrix lhs, Matrix rhs)
        {
            if (((object)lhs == null) && ((object)rhs == null)) return true;
            if (((object)lhs == null) && ((object)rhs != null)) return false;
            if (((object)lhs != null) && ((object)rhs == null)) return false;
            if (rhs.Width != lhs.Width) return false;
            if (rhs.Height != lhs.Height) return false;

            int i = 0, I = rhs.Width;
            int j = 0, J = rhs.Height;
            for (i = 0; i < I; i++)
                for (j = 0; j > J; j++)
                    if (Math.Abs(rhs[i, j] - lhs[i, j]) > Double.Epsilon)
                        return false;

            return true;
        }

        /// <summary>
        /// Comparison Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(Matrix lhs, Matrix rhs)
        {
            return !(rhs == lhs);
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion Comparison Stuff

        #region ShallowTransposeMatrix

        /// <summary>
        /// Provides reversed indexing of the matrix so that we can get and use the transpose matrix without
        /// having to copy (even worse - piecewise copy!) the matrix elements.
        /// </summary>
        private class ShallowTransposeMatrix : Matrix
        {
            #region Constructor

            /// <summary>
            /// Ensures that the base matrix object is pointing to the same data
            /// </summary>
            /// <param name="thebase">The matrix for which we want the transpose</param>
            public ShallowTransposeMatrix(Matrix thebase)
                : base(thebase)
            {
                // the transpose of the shallow transpose is the original!

                _shallowtranspose = thebase;
            }

            #endregion Constructor

            #region Public Property Overrides

            /// <summary>
            /// Create A Clone Of The Matrix
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                return ((Matrix)base.Clone()).Transpose;
            }

            public override double this[int i, int j, int ignoreThisIndexer]
            {
                get
                {
                    return this[i, j];
                }
                set
                {
                    this[i, j] = value;
                }
            }

            /// <summary>
            /// Indexer For The Matrix. i is the row number. j is the column number.
            /// </summary>
            public override double this[int i, int j]
            {
                get
                {
                    return this._MatrixData[j, i];
                }
                set
                {
                    base[j, i] = value;
                }
            }

            /// <summary>
            /// Indexer For a Vector Matrix. i is the column number.
            /// </summary>
            public override double this[int j]
            {
                get
                {
                    return this._MatrixData[0, j];
                }
                set
                {
                    this._MatrixData[0, j] = value;
                }
            }

            /// <summary>
            /// The Width Of The Matrix
            /// </summary>
            public override int Width
            {
                get
                {
                    return this._MatrixData.GetLength(0);//_width;
                }
            }

            /// <summary>
            /// The Height Of The Matrix
            /// </summary>
            public override int Height
            {
                get
                {
                    return this._MatrixData.GetLength(1);//_height;
                }
            }

            /// <summary>
            /// Overload ToString() method
            /// </summary>
            /// <param name="format">A valid format string</param>
            /// <returns>Formatted matrix</returns>
            public override string ToString(string format)
            {
                string output = "";
                string format_ = format;// = format + "\t";

                if (this.Height == 0 && this.Width == 0)
                    return "null matrix";

                int i = 0, I = this.Height;
                int j = 0, J = this.Width;

                for (i = 0; i < I; i++)
                {
                    for (j = 0; j < J; j++)
                    {
                        // will throw format exception if an invalid format is passed
                        output += string.Format(format_, this._MatrixData[j, i]);
                    }

                    // remove trailing tab
                    output = output.Remove(output.Length - 1, 1);
                    output += "\n";
                }

                return output;
            }

            #endregion Public Property Overrides

            #region Static Cast overrides

            /// <summary>
            /// Casts the matrix as a 2D double array
            /// </summary>
            /// <param name="lhs"></param>
            /// <returns>2D double array</returns>
            public static explicit operator double[,] (ShallowTransposeMatrix lhs)
            {
                return (double[,])lhs.Transpose;
            }

            #endregion Static Cast overrides
        }

        #endregion ShallowTransposeMatrix

        private class AutoCorrelationComposition : Matrix
        {
            public AutoCorrelationComposition(Matrix theBase)
                : base(theBase)
            {
                Matrix X = this.NormalisedCentredMatrix;
                Matrix C = (X.Transpose * X);
                _C = (1.0 / (X.Height - 1)) * C;
                _vif = null;
            }

            public new Matrix CorrelationMatrix
            {
                get
                {
                    return _C;
                }
            }

            public new Matrix VarianceInflationFactors
            {
                get
                {
                    if (_vif == null)
                    {
                        Matrix CInv;

                        if (_C.IsNonSingular == false)
                            CInv = _C.PseudoInverse;
                        else
                            CInv = _C.Inverse;

                        _vif = new Matrix(CInv.Height);
                        for (int i = 0; i < CInv.Height; i++)
                        {
                            _vif[i] = CInv[i, i];
                        }
                    }
                    return _vif;
                }
            }

            private Matrix _C, _vif;
        }

        public Matrix NormalisedCentredMatrix
        {
            get
            {
                Matrix newMat = new Matrix(this.Height, this.Width);
                double s, m;
                for (int j = 0; j < this.Width; j++)
                {
                    m = this.ColAvg(j);
                    s = this.ColStdev(j);

                    if (s == 0)
                    {
                        for (int i = 0; i < this.Height; i++)
                            newMat[i, j] = 0;
                    }
                    else
                    {
                        for (int i = 0; i < this.Height; i++)
                        {
                            newMat[i, j] = (this[i, j] - m) / s;
                        }
                    }
                }

                return newMat;
            }
        }

        public Matrix CorrelationMatrix
        {
            get
            {
                if (_aCorrelComp == null)
                    _aCorrelComp = new AutoCorrelationComposition(this);

                return _aCorrelComp.CorrelationMatrix;
                //				Matrix X = this.NormalisedCentredMatrix;
                //				Matrix C = (X.Tr * X);
                //				return (1.0/(X.Height-1))*C;
            }
        }

        public Matrix VarianceInflationFactors
        {
            get
            {
                if (_aCorrelComp == null)
                    _aCorrelComp = new AutoCorrelationComposition(this);

                return _aCorrelComp.VarianceInflationFactors;
            }
        }

        public double RowStdev(int i)
        {
            double sum = 0, sum2 = 0, value;
            for (int c = 0; c < this.Width; c++)
            {
                value = this[i, c];
                sum += value;
                sum2 += value * value;
            }

            return Math.Sqrt((sum2 - sum * sum / this.Width) / (this.Width - 1));
        }

        public double ColStdev(int j)
        {
            double sum = 0, sum2 = 0, value;
            for (int r = 0; r < this.Height; r++)
            {
                value = this[r, j];
                sum += value;
                sum2 += value * value;
            }

            return Math.Sqrt((sum2 - sum * sum / this.Height) / (this.Height - 1));
        }

        public double ColAvg(int j)
        {
            return this.ColSum(j) / this.Height;
        }

        public double RowAvg(int i)
        {
            return this.RowSum(i) / this.Width;
        }

        public double ColSum(int j)
        {
            double sum = 0;
            for (int r = 0; r < this.Height; r++)
            {
                sum += this[r, j];
            }
            return sum;
        }

        public double RowSum(int i)
        {
            double sum = 0;
            for (int c = 0; c < this.Width; c++)
            {
                sum += this[i, c];
            }
            return sum;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void DebugPrint()
        {
            System.Diagnostics.Debug.WriteLine(this.ToString());
        }

        public List<string> ColumnNames { get; set; }

        public List<DateTime> RowNames { get; set; }

        public double[,] MatrixData
        {
            get
            {
                return _MatrixData;
            }
        }
    }
}