using System;

// Code Based Off .NET Mapack

namespace MathLib
{
    /// <summary>
    /// Simple Class For Handling Vectors.
    /// Implements:
    ///		Basic Operators
    ///		Length
    ///		Normalised Vector
    ///		Dot Product
    ///		Cross Product
    /// </summary>
    public class Vector
    {
        #region Class Parameters

        private double[] _VectorData;

        #endregion Class Parameters

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Dimension"></param>
        public Vector(int Dimension)
        {
            this._VectorData = new double[Dimension];
        }

        /// <summary>
        /// Construct a Vector From A 1D Array
        /// </summary>
        /// <param name="data"></param>
        public Vector(double[] data)
        {
            this._VectorData = (double[])data.Clone();
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// Indexor
        /// </summary>
        public double this[int i]
        {
            get { return this._VectorData[i]; }
            set { this._VectorData[i] = value; }
        }

        /// <summary>
        /// Get The Dimension Of The Vector
        /// </summary>
        public int Dimension
        {
            get { return this._VectorData.Length; }
        }

        /// <summary>
        /// Length Of The Vector
        /// </summary>
        public double Norm
        {
            get { return Math.Sqrt(DotProduct(this, this)); }
        }

        /// <summary>
        /// Creates A Noramilised (i.e. Length 1) Vector
        /// </summary>
        public Vector Normalised
        {
            get { return this / this.Norm; }
        }

        #endregion Public Properties

        #region Operator Overrides

        #region ExplicitCasts

        /// <summary>
        /// Explicit cast of a 1 dimensional vector to a double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator double (Vector value)
        {
            if (value.Dimension != 1)
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            else
                return value[0];
        }

        #endregion ExplicitCasts

        #region ToString()

        /// <summary>
        /// The ToString() Method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "";
            for (int i = 0; i < this.Dimension; i++)
                output += (i == 0 ? "" : "\t") + this[i].ToString();
            return output + "\n";
        }

        #endregion ToString()

        #region Scalar Operators

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator *(double lhs, Vector rhs)
        {
            int i;

            Vector output = new Vector(rhs.Dimension);

            for (i = 0; i < rhs.Dimension; i++)
            {
                output[i] = rhs[i] * lhs;
            }

            return output;
        }

        /// <summary>
        /// * Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator *(Vector lhs, double rhs)
        {
            return rhs * lhs;
        }

        /// <summary>
        /// / Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator /(Vector lhs, double rhs)
        {
            return (1 / rhs) * lhs;
        }

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator +(double lhs, Vector rhs)
        {
            int i;

            Vector output = new Vector(rhs.Dimension);

            for (i = 0; i < rhs.Dimension; i++)
            {
                output[i] = rhs[i] + lhs;
            }

            return output;
        }

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator +(Vector lhs, double rhs)
        {
            return rhs + lhs;
        }

        /// <summary>
        /// - Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator -(double lhs, Vector rhs)
        {
            return lhs + -1 * rhs;
        }

        /// <summary>
        /// - Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator -(Vector lhs, double rhs)
        {
            return lhs + -1 * rhs;
        }

        #endregion Scalar Operators

        #region Vector Operators

        /// <summary>
        /// + Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator +(Vector lhs, Vector rhs)
        {
            if (lhs.Dimension != rhs.Dimension)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
            {
                int i;
                Vector output = new MathLib.Vector(lhs.Dimension);

                for (i = 0; i < lhs.Dimension; i++)
                {
                    output[i] = lhs[i] + rhs[i];
                }

                return output;
            }
        }

        /// <summary>
        /// / Operator: does pointwise division
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MathLib.Vector operator /(Vector lhs, Vector rhs)
        {
            if (lhs.Dimension != rhs.Dimension)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
            {
                int i;
                Vector output = new MathLib.Vector(lhs.Dimension);

                for (i = 0; i < lhs.Dimension; i++)
                {
                    output[i] = lhs[i] / rhs[i];
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
        public static MathLib.Vector operator -(Vector lhs, Vector rhs)
        {
            return lhs + -1 * rhs;
        }

        #endregion Vector Operators

        public Vector getRange(int[] idx)
        {
            Vector X = new Vector(idx.Length);
            for (int j = 0; j < idx.Length; j++)
            {
                X[j] = this[idx[j]];
            }
            return X;
        }

        public Vector getRange(int a, int b)
        {
            Vector X = new Vector(b - a);
            int k = 0;
            for (int j = a; j <= b; j++)
            {
                X[k] = this[j];
                k++;
            }
            return X;
        }

        public double sum()
        {
            double s = 0;
            for (int i = 0; i < this.Dimension; i++)
            {
                s += this[i];
            }
            return s;
        }

        public static Vector min(double s, Vector V)
        {
            Vector X = new Vector(V.Dimension);
            for (int j = 0; j < V.Dimension; j++)
            {
                X[j] = Math.Min(s, V[j]);
            }
            return X;
        }

        public static Vector min(Vector V, double s)
        {
            return min(s, V);
        }

        #region Comparison Operators

        public static bool[] operator >(Vector lhs, Vector rhs)
        {
            if (lhs.Dimension != rhs.Dimension)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
            {
                int i;
                bool[] output = new bool[lhs.Dimension];

                for (i = 0; i < lhs.Dimension; i++)
                {
                    output[i] = lhs[i] > rhs[i];
                }

                return output;
            }
        }

        public static bool[] operator >(Vector lhs, double rhs)
        {
            int i;
            bool[] output = new bool[lhs.Dimension];

            for (i = 0; i < lhs.Dimension; i++)
            {
                output[i] = (lhs[i] > rhs);
            }

            return output;
        }

        public static bool[] operator >=(Vector lhs, double rhs)
        {
            int i;
            bool[] output = new bool[lhs.Dimension];

            for (i = 0; i < lhs.Dimension; i++)
            {
                output[i] = (lhs[i] > rhs) || (lhs[i] == rhs);
            }

            return output;
        }

        public static bool[] operator <(Vector lhs, Vector rhs)
        {
            return rhs > lhs;
        }

        public static bool[] operator <(Vector lhs, double rhs)
        {
            int i;
            bool[] output = new bool[lhs.Dimension];

            for (i = 0; i < lhs.Dimension; i++)
            {
                output[i] = (lhs[i] < rhs);
            }

            return output;
        }

        public static bool[] operator <=(Vector lhs, double rhs)
        {
            int i;
            bool[] output = new bool[lhs.Dimension];

            for (i = 0; i < lhs.Dimension; i++)
            {
                output[i] = (lhs[i] < rhs) || (lhs[i] == rhs);
            }

            return output;
        }

        public static bool[] operator >(double lhs, Vector rhs)
        {
            return rhs < lhs;
        }

        public static bool[] operator >=(double lhs, Vector rhs)
        {
            return rhs <= lhs;
        }

        public static bool[] operator <(double lhs, Vector rhs)
        {
            return rhs > lhs;
        }

        public static bool[] operator <=(double lhs, Vector rhs)
        {
            return rhs >= lhs;
        }

        public static bool[] operator >=(Vector lhs, Vector rhs)
        {
            if (lhs.Dimension != rhs.Dimension)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
            {
                int i;
                bool[] output = new bool[lhs.Dimension];

                for (i = 0; i < lhs.Dimension; i++)
                {
                    output[i] = (lhs[i] > rhs[i]) || (lhs[i] == rhs[i]);
                }

                return output;
            }
        }

        public static bool[] operator <=(Vector lhs, Vector rhs)
        {
            return rhs >= lhs;
        }

        #endregion Comparison Operators

        #endregion Operator Overrides

        #region Casts

        /// <summary>
        /// Casts the vector as a 1D double array
        /// </summary>
        /// <param name="lhs"></param>
        /// <returns>1D double array</returns>
        public static explicit operator double[] (Vector lhs)
        {
            return lhs._VectorData;
        }

        #endregion Casts

        #region Vector Functions

        /// <summary>
        /// Calcuates The DotProduct Of A Pair Of Vectors
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static double DotProduct(Vector lhs, Vector rhs)
        {
            if (lhs.Dimension != rhs.Dimension)
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different");
            }
            else
            {
                int i;
                double output = 0;

                for (i = 0; i < lhs.Dimension; i++)
                {
                    output += lhs[i] * rhs[i];
                }

                return output;
            }
        }

        /// <summary>
        /// Calculates The CrossProduct Of A Pair Of Vectors
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Vector CrossProduct(Vector lhs, Vector rhs)
        {
            if ((lhs.Dimension != rhs.Dimension) || (lhs.Dimension != 3))
            {
                throw new System.ArgumentException("lhs & rhs Dimensions are different or not equal to 3");
            }
            else
            {
                Vector output = new MathLib.Vector(3);

                output[0] = lhs[2] * rhs[3] - lhs[3] * rhs[2];
                output[1] = lhs[3] * rhs[1] - lhs[1] * rhs[3];
                output[2] = lhs[1] * rhs[2] - lhs[2] * rhs[1];

                return output;
            }
        }

        public void Multiply(double lhs)
        {
            for (int i = 0; i < this._VectorData.Length; i++)
            {
                this._VectorData[i] *= lhs;
            }

            return;
        }

        /// <summary>
        /// Obtain a copy of the vector data represented as a double array.
        /// </summary>
        /// <returns></returns>
        public double[] ToArray()
        {
            double[] retVal = new double[this._VectorData.Length];
            Array.Copy(_VectorData, retVal, _VectorData.Length);
            return retVal;
        }

        #endregion Vector Functions
    }
}