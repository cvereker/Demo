using System;

namespace MathLib
{
    public class MatrixMath
    {
        public static Matrix VarianceCoVariance(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            double[] μ = new double[J];
            for (int j = 0; j < J; j++)
            {
                for (int t = 0; t < T; t++)
                {
                    μ[j] += m[t, j];
                }
                μ[j] /= T;
            }

            Matrix Σ = new Matrix(J, J);
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    for (int t = 0; t < T; t++)
                    {
                        // logic explaining the appearance of the saxpy algorithm
                        // tr(M) * M
                        // tr(m)[i,k] * m[k,j];
                        Σ[i, j] += m[t, i] * m[t, j];
                    }
                    Σ[i, j] /= T;
                    Σ[i, j] -= μ[i] * μ[j];
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_Tw(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            double t0 = 0; // for completeness (i.e. we could put real times in here)

            double acc;
            double[] μ = new double[J];
            double[] w = new double[T];
            for (int t = 0; t < T; t++)
            {
                w[t] = (t - t0) / (double)(T - 1 - t0);
            }

            for (int j = 0; j < J; j++)
            {
                acc = 0;
                for (int t = 0; t < T; t++)
                {
                    μ[j] += w[t] * m[t, j];
                    acc += w[t];
                }
                μ[j] /= acc;
            }

            Matrix Σ = new Matrix(J, J);
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    acc = 0;
                    for (int t = 0; t < T; t++)
                    {
                        acc += w[t];
                        Σ[i, j] += w[t] * m[t, i] * m[t, j];
                    }
                    Σ[i, j] /= acc;
                    Σ[i, j] -= μ[i] * μ[j];
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_EW(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            double acc;
            double[] μ = new double[J];
            double[] w = new double[T];
            double λ = 0.97;
            for (int t = 0; t < T; t++)
            {
                w[t] = Math.Pow(λ, T - t - 1);
            }

            for (int j = 0; j < J; j++)
            {
                acc = 0;
                for (int t = 0; t < T; t++)
                {
                    μ[j] += w[t] * m[t, j];
                    acc += w[t];
                }
                μ[j] /= acc;
            }

            Matrix Σ = new Matrix(J, J);
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    acc = 0;
                    for (int t = 0; t < T; t++)
                    {
                        acc += w[t];
                        Σ[i, j] += w[t] * m[t, i] * m[t, j];
                    }
                    Σ[i, j] /= acc;
                    Σ[i, j] -= μ[i] * μ[j];
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_NaN(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            int cnt = 0;
            double[] μ = new double[J];
            for (int j = 0; j < J; j++)
            {
                cnt++;
                for (int t = 0; t < T; t++)
                {
                    if (double.IsNaN(m[t, j]) == false)
                    {
                        μ[j] += m[t, j];
                        cnt++;
                    }
                }
                μ[j] /= cnt;
            }

            Matrix Σ = new Matrix(J, J);
            double c;
            cnt = 0;
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    cnt++;
                    for (int t = 0; t < T; t++)
                    {
                        c = m[t, i] * m[t, j];
                        if (double.IsNaN(c) == false)
                        {
                            Σ[i, j] += m[t, i] * m[t, j];
                            cnt++;
                        }
                    }
                    Σ[i, j] /= cnt;
                    Σ[i, j] -= μ[i] * μ[j];
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_EW_Weekly(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            double acc;
            double[] μ = new double[J];
            double[] w = new double[T];
            double λ = 0.97;
            for (int t = 0; t < T; t++)
            {
                w[t] = Math.Pow(λ, T - t - 1);
            }

            for (int j = 0; j < J; j++)
            {
                acc = 0;
                for (int t = 0; t < T; t++)
                {
                    μ[j] += w[t] * m[t, j];
                    acc += w[t];
                }
                μ[j] /= acc;
            }

            Matrix Σ = new Matrix(J, J);
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    acc = 0;
                    for (int t = 0; t < T; t++)
                    {
                        acc += w[t];
                        Σ[i, j] += w[t] * m[t, i] * m[t, j];
                    }
                    Σ[i, j] /= acc;
                    Σ[i, j] -= μ[i] * μ[j];
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_EW_WeeklyTwoDecay(Matrix m)
        {
            int T, J;
            T = m.Height;
            J = m.Width;

            double accShort, accLong;
            double[,] μShort = new double[J, 2];
            double[] wShort = new double[T];
            double[] wLong = new double[T];
            double[,] μLong = new double[J, 2];
            double λShort = 0.97;
            double λLong = 0.997;
            for (int t = 0; t < T; t++)
            {
                wShort[t] = Math.Pow(λShort, T - t - 1);
                wLong[t] = Math.Pow(λLong, T - t - 1);
            }

            for (int j = 0; j < J; j++)
            {
                accShort = accLong = 0;
                for (int t = 0; t < T; t++)
                {
                    μShort[j, 0] += wShort[t] * m[t, j];
                    μShort[j, 1] += wShort[t] * m[t, j] * m[t, j];
                    accShort += wShort[t];

                    μLong[j, 0] += wLong[t] * m[t, j];
                    μLong[j, 1] += wLong[t] * m[t, j] * m[t, j];
                    accLong += wLong[t];
                }

                μShort[j, 0] /= accShort; // mean
                // assume zero mean
                μShort[j, 1] = μShort[j, 1] / accShort; // zero mean
                //μShort[j, 1] = μShort[j, 1] / accShort - μShort[j, 0] * μShort[j, 0]; // variance

                μLong[j, 0] /= accLong;  // mean
                // assume zero mean
                μLong[j, 1] = μLong[j, 1] / accLong; // zero mean
                //μLong[j, 1] = μLong[j, 1] / accLong - μLong[j, 0] * μLong[j, 0];    // variance
            }

            Matrix Σ = new Matrix(J, J);
            for (int i = 0; i < J; i++)
            {
                for (int j = 0; j < J; j++)
                {
                    accLong = 0;
                    for (int t = 0; t < T; t++)
                    {
                        accLong += wLong[t];
                        Σ[i, j] += wLong[t] * m[t, i] * m[t, j];
                    }

                    // standard variance co-variance for the Long decay factor
                    Σ[i, j] /= accLong;
                    //Σ[i, j] -= μLong[i,0] * μLong[j,0]; // Assume zero mean

                    // re-scale by the ratio of standard deviations
                    Σ[i, j] *= Math.Sqrt(μShort[i, 1] * μShort[j, 1] / (μLong[i, 1] * μLong[j, 1]));
                }
            }
            // for checking. Note, if comparing with excel note that this is the mle of vcv - i.e. has divided by T, not (T-1)
            //            System.Diagnostics.Debug.WriteLine(m.ToString());
            //            System.Diagnostics.Debug.WriteLine(Σ.ToString());
            return Σ;
        }

        public static Matrix VarianceCoVariance_ShrinkageTarget(Matrix X)
        {
            Matrix S;

            S = VarianceCoVariance_EW_WeeklyTwoDecay(X);

            Matrix S1 = X * S.EigenvectorMatrix2.Submatrix(null, 0, 1);

            return null;
        }

        public static Matrix InsertConstantColumn(Matrix m)
        {
            Matrix mCopy = new Matrix(m.Height, 1 + m.Width);
            int K = m.Width;

            int sourceIndex, destinationIndex;
            for (int t = 0; t < m.Height; t++)
            {
                sourceIndex = t * K;
                destinationIndex = 1 + t * (K + 1);
                Array.Copy(m.MatrixData, sourceIndex, mCopy.MatrixData, destinationIndex, K);
                mCopy[t, 0] = 1;
            }

            return mCopy;
        }

        public static Matrix DropColumns(Matrix mat, params int[] columns)
        {
            System.Collections.Generic.List<int> li = new System.Collections.Generic.List<int>(columns);

            Matrix m = new Matrix(mat.Height, mat.Width - li.Count);
            for (int jj = 0, j = 0; j < mat.Width; j++)
            {
                if (li.Contains(j))
                    continue;

                for (int i = 0; i < mat.Height; i++)
                {
                    m[i, jj] = mat[i, j];
                }
                jj++;
            }
            return m;
        }

        public static Matrix NormaliseMatrix(Matrix mat, out double[] mean, out double[] stdev)
        {
            Matrix newMat = new Matrix(mat.Height, mat.Width);
            mean = new double[mat.Width];
            stdev = new double[mat.Width];
            double s, m;
            for (int j = 0; j < mat.Width; j++)
            {
                m = mat.ColAvg(j);
                s = mat.ColStdev(j);

                mean[j] = m;
                stdev[j] = s;

                if (s == 0)
                {
                    for (int i = 0; i < mat.Height; i++)
                        newMat[i, j] = 0;
                }
                else
                {
                    for (int i = 0; i < mat.Height; i++)
                    {
                        newMat[i, j] = (mat[i, j] - m) / s;
                    }
                }
            }

            return newMat;
        }
    }
}