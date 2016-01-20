using System;

namespace MathLib
{
    /// <summary>
    /// Abstarct Class Providing Set Of Static Functions For Calculation
    /// </summary>
    public abstract class Utils
    {
        public static int Factorial(int n)
        {
            if (n == 0)
                return 1;
            else
                return n * Factorial(n - 1);
        }

        public static double Bound(double value, double min, double max)
        {
            return (value < min) ? min : ((value > max) ? max : value);
        }

        #region Fix

        /// <summary>
        /// Returns The Integer Part Of The Number
        /// e.g.	5.213		==> 5
        ///			-3.212		==> -3
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static double Fix(double Value)
        {
            return (Value > 0) ? System.Math.Floor(Value) : System.Math.Ceiling(Value);
        }

        #endregion Fix

        #region ExpX2

        // Based Of Code From The
        // Cephes Math Library Release 2.9:  June, 2000
        // Copyright 2000 by Stephen L. Moshier

        /// <summary>
        /// Computes y = exp(x*x) while suppressing error amplification
        /// that would ordinarily arise from the inexactness of the
        /// exponential argument x*x.
        ///
        ///  *                      Relative error:
        ///  * arithmetic    domain     # trials      peak         rms
        ///  *   IEEE      -26.6, 26.6    10^7       3.9e-16     8.9e-17
        /// </summary>
        /// <param name="x"></param>
        /// <param name="sign">If sign &lt; 0, the result is inverted; i.e., y = exp(-x*x)</param>
        /// <returns>exp(Sign(sign) * x * x)</returns>
        public static double ExpX2(double x, int sign)
        {
            double M = 128.0;
            double MInv = 0.0078125;

            double u, u1, m, f;

            x = Math.Abs(x);
            if (sign < 0) x = -x;

            // Represent x as an exact multiple of M plus a residual.
            // M is a power of 2 chosen so that exp(m * m) does not overflow
            // or underflow and so that |x - m| is small.
            m = MInv * Math.Floor(M * x + 0.5);
            f = x - m;

            // x^2 = m^2 + 2mf + f^2
            u = m * m;
            u1 = 2 * m * f + f * f;

            if (sign < 0)
            {
                u = -u;
                u1 = -u1;
            }

            // u is exact, u1 is small.
            u = Math.Exp(u) * Math.Exp(u1);

            return (u);
        }

        #endregion ExpX2

        #region PolyEval

        // Based Of Code From The
        // Cephes Math Library Release 2.1:  December, 1988
        // Copyright 1984, 1987, 1988 by Stephen L. Moshier
        // Direct inquiries to 30 Frost Street, Cambridge, MA 02140

        /// <summary>
        /// Evaluates A Polynomial
        /// </summary>
        /// <param name="x"></param>
        /// <param name="coef">Coefficients Of The Polynomial</param>
        /// <returns>Evaluated Value</returns>
        public static double PolyEval(double x, double[] coef)
        {
            double ans = 0;

            foreach (double val in coef)
            {
                ans = ans * x + val;
            }

            return ans;
        }

        /// <summary>
        /// Evaluates A Polynomial Where X^N Coefficient Is One
        /// </summary>
        /// <param name="x"></param>
        /// <param name="coef">Set Of Coefficients EXCLUDING X^N</param>
        /// <returns></returns>
        public static double Poly1Eval(double x, double[] coef)
        {
            double ans = 1;

            foreach (double val in coef)
            {
                ans = ans * x + val;
            }

            return ans;
        }

        #endregion PolyEval

        #region Thomas Solver

        /// <summary>
        /// Thomas algorithm for solving a tridiagonal system of equations
        ///
        /// Equation ith:
        ///      Li[i]*xv[i-1] + Di[i]*xv[i] + Ui[i]*xv[i+1] = Bi[i]
        /// </summary>
        /// <param name="Li">lower diagonal  n = 0,.. n-1    Li[0]   = 0</param>
        /// <param name="Di">diagonal        n = 0,.. n-1</param>
        /// <param name="Ui">lower diagonal  n = 0,.. n-1    Ui[n-1] = 0</param>
        /// <param name="Bi"></param>
        /// <returns>xv</returns>
        public static Vector Thomas(Vector Li, Vector Di, Vector Ui, Vector Bi)
        {
            if ((Di.Dimension != Li.Dimension) || (Ui.Dimension != Li.Dimension) || (Bi.Dimension != Li.Dimension))
                throw new ArgumentException("Vector's not of consistent dimension.");
            if (Di[0] == 0.0)
                throw new ArithmeticException("Error 1 in Thomas");

            double bet;
            Vector xv = new Vector(Li.Dimension);
            Vector Gi = new Vector(Li.Dimension);

            // Forward substitution
            Gi[0] = Di[0];
            xv[0] = Bi[0];

            for (int i = 1; i < Li.Dimension; i++)
            {
                bet = Li[i] / Gi[i - 1];
                Gi[i] = Di[i] - bet * Ui[i - 1];
                xv[i] = Bi[i] - bet * xv[i - 1];

                if (Gi[i] == 0.0)
                    throw new ArithmeticException("Error 2 in Thomas");
            }

            // Backward substitution
            xv[Li.Dimension - 1] = xv[Li.Dimension - 1] / Gi[Li.Dimension - 1];

            for (int i = Li.Dimension - 2; i >= 0; i--)
                xv[i] = (xv[i] - Ui[i] * xv[i + 1]) / Gi[i];

            return xv;
        }

        #endregion Thomas Solver

        #region Fibonacci

        /// <summary>
        /// Get The nth Fibonacci Number
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static int Fibonacci(int idx)
        {
            return Fibonacci(idx, 1);
        }

        /// <summary>
        /// Get The nth Fibonacci Number
        /// Step allows getting intermediate points
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int Fibonacci(int idx, int step)
        {
            if (step < 1) throw new ArgumentException("Invalid Step Size");

            int baseIdx = idx / step;
            double baseValue = (Math.Pow(1 + Math.Sqrt(5), baseIdx) - Math.Pow(1 - Math.Sqrt(5), baseIdx)) / (Math.Pow(2, baseIdx) * Math.Sqrt(5));
            if (idx != baseIdx * step)
            {
                int nextIdx = baseIdx + 1;
                double grad = (Math.Pow(1 + Math.Sqrt(5), nextIdx) - Math.Pow(1 - Math.Sqrt(5), nextIdx)) / (Math.Pow(2, nextIdx) * Math.Sqrt(5)) - baseValue;
                baseValue = baseValue + ((double)idx - baseIdx * step) / step * grad;
            }

            return (int)Math.Floor(baseValue);
        }

        #endregion Fibonacci

        #region QRoot

        /// <summary>
        /// Solves a Quadratic (or Linear) Equation
        /// </summary>
        /// <param name="quadratic"></param>
        /// <param name="linear"></param>
        /// <param name="constant"></param>
        /// <returns></returns>
        public static double[] QRoot(double quadratic, double linear, double constant)
        {
            if (quadratic == 0.0)
                return new double[] { -constant / linear };

            double det = linear * linear - 4 * quadratic * constant;

            if (det < 0)
                return new double[0];
            else if (det == 0)
                return new double[] { -linear / (2 * quadratic) };
            else
            {
                det = Math.Sqrt(det);
                return new double[] { (-linear + det) / (2 * quadratic), (-linear - det) / (2 * quadratic) };
            }
        }

        #endregion QRoot

        #region CRoot

        /// <summary>
        /// Finds the Real Roots Of A Cubic
        /// http://mathworld.wolfram.com/CubicFormula.html
        /// </summary>
        /// <returns></returns>
        public static double[] CRoot(double cubic, double quadratic, double linear, double constant)
        {
            if (cubic == 0) return QRoot(quadratic, linear, constant);

            double a2 = quadratic / cubic;
            double a1 = linear / cubic;
            double a0 = constant / cubic;
            double Q = (a2 * a2 - 3 * a1) / 9.0;
            double R = (2 * Math.Pow(a2, 3) - 9 * a1 * a2 + 27 * a0) / 54;

            double D = R * R - Math.Pow(Q, 3);
            if (D < 0)
            {
                double theta = Math.Acos(R / Math.Pow(Q, 1.5));

                double[] output = new double[3];
                output[0] = -2 * Math.Sqrt(Q) * Math.Cos(theta / 3.0) - a2 / 3.0;
                output[1] = -2 * Math.Sqrt(Q) * Math.Cos((theta + 2 * Math.PI) / 3.0) - a2 / 3.0;
                output[2] = -2 * Math.Sqrt(Q) * Math.Cos((theta - 2 * Math.PI) / 3.0) - a2 / 3.0;
                return output;
            }
            else
            {
                double capA = (double.IsNaN(R) ? double.NaN : -Math.Sign(R) * Math.Pow(Math.Abs(R) + Math.Sqrt(D), 1.0 / 3.0));
                double capB = (capA == 0 ? 0.0 : Q / capA);
                return new double[] { capA + capB - a2 / 3.0 };
            }
        }

        #endregion CRoot

        public static double Max(params double[] values)
        {
            if (values == null)
                return double.NaN;

            double max = values[0];

            foreach (double value in values)
            {
                max = System.Math.Max(value, max);
            }
            return max;
        }

        public static double Min(params double[] values)
        {
            if (values == null)
                return double.NaN;

            double max = values[0];

            foreach (double value in values)
            {
                max = System.Math.Min(value, max);
            }
            return max;
        }

        public static int Max(params int[] values)
        {
            int max = values[0];

            foreach (int value in values)
            {
                max = System.Math.Max(value, max);
            }
            return max;
        }

        public static int Min(params int[] values)
        {
            int max = values[0];

            foreach (int value in values)
            {
                max = System.Math.Min(value, max);
            }
            return max;
        }

        /// <summary>
        /// Computes mean and variance by using roundoff stable algorithm
        /// </summary>
        public static void MeanAndVar(double[] x, out double mean, out double var)
        {
            int count = 0;
            double alpha = x[0];
            double beta = 0;
            double temp;

            for (int i = 1; i < x.Length; i++)
            {
                count = i + 1;
                temp = (x[i] - alpha);
                alpha += temp / count;
                beta += i * temp * temp / count;
            }
            mean = alpha;
            var = beta / (count - 1);
        }
    }
}