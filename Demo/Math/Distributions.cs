// BaselineVersion= Post strange Tortoise behaviour  Date=8/9/2005 9:46:51 AM
using System;

namespace MathLib
{
    /// <summary>
    /// Summary description for Distributions.
    /// </summary>
    public class Distributions
    {
        public static double tcdf(double z, double n)
        {
            return 1 - SpecialFunctions.betai(n / 2, 0.5, n / (n + z * z));
        }

        /// <summary>
        /// Student's t probability density function (pdf)
        /// </summary>
        /// <param name="z"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double tpdf(double z, double n)
        {
            double p = Math.Exp(SpecialFunctions.gammln((n + 1) / 2) - SpecialFunctions.gammln(n / 2)) /
                (Math.Sqrt(n * Math.PI) * Math.Pow(1 + z * z / n, +(n + 1) * 0.5));

            return p;
        }

        public static double npdf(double z, double mu, double sig)
        {
            double p = Math.Exp(-(z - mu) * (z - mu) / (2 * sig * sig)) / (sig * Math.Sqrt(2 * Math.PI));
            return p;
        }

        public static double FDistcdf(double x, int d1, int d2)
        {
            double prob = MathLib.SpecialFunctions.betai(0.5 * d1, 0.5 * d2, d1 / (d1 + d2 * x));
            return prob;
        }

        /// <summary>
        /// Chi-square probability density function (pdf)
        /// </summary>
        /// <param name="z"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double chi2pdf(double z, double n)
        {
            return Math.Pow(z, (n - 2) / 2) * Math.Exp(-z / 2 - SpecialFunctions.gammln(n / 2)) / Math.Pow(2, n / 2);
        }

        /// <summary>
        /// Rayleigh probability density function
        /// </summary>
        /// <param name="z"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double raylpdf(double z, double b)
        {
            b *= b;
            return z / b * Math.Exp(-z * z / (2 * b));
        }

        /// <summary>
        /// Class for calculating the normal distribution function to 15 d.p. (and it's inverse)
        /// </summary>
        public class Normals
        {
            #region Paramaters

            private static double PI = 3.1415926535897932384626433832795028841971693993751058209749445923078164062;

            #endregion Paramaters

            #region ctor

            public Normals()
            {
                //
                // TODO: Add constructor logic here
                //
            }

            #endregion ctor

            #region Methods

            /// <summary>
            /// Computes the normal distribution using a number of methods, mainly reported by Genz and Hart
            /// </summary>
            /// <param name="z"></param>
            /// <returns></returns>
            public static double NormalDist(double z)
            {
                double retVal;

                if (z >= 5)
                    retVal = algHartGenz(z);
                if (z >= 7)
                    retVal = algHart(z);
                else
                    retVal = algHartGenz(z);

                //Fast Strecock(z)

                return retVal;
            }

            /// <summary>
            /// Replaces NormSInv for quasi-random sequences (eg Faure) - See Moro (1995)
            ///
            /// </summary>
            /// <remarks>Coded by Mike Staunton</remarks>
            /// <param name="z"></param>
            /// <returns>NormSInv(z)</returns>
            public static double MoroNormSInv(double z)
            {
                double p;
                double[] a = new double[] { 2.50662823884, -18.61500062529, 41.39119773534, -25.44106049637 };
                double[] b = new double[] { -8.4735109309, 23.08336743743, -21.06224101826, 3.13082909833 };
                double[] c = new double[] { 0.337475482272615, 0.976169019091719, 0.160797971491821, 2.76438810333863E-02, 3.8405729373609E-03, 3.951896511919E-04, 3.21767881768E-05, 2.888167364E-07, 3.960315187E-07 };

                double y = z - 0.5;
                if (Math.Abs(y) < 0.42)
                {
                    p = y * y;
                    p = y * (((a[3] * p + a[3]) * p + a[1]) * p + a[0]) / ((((b[3] * p + b[2]) * p + b[1]) * p + b[0]) * p + 1);
                }
                else
                {
                    if (y > 0)
                        p = Math.Log(-Math.Log(1 - z));
                    else
                        p = Math.Log(-Math.Log(z));

                    p = c[0] + p * (c[1] + p * (c[2] + p * (c[3] + p * (c[4] + p * (c[5] + p * (c[6] + p * (c[7] + p * c[8])))))));

                    if (y <= 0)
                        p = -p;
                }
                return p;
            }

            #endregion Methods

            #region Private Normal Routines

            /// <summary>
            /// Hart's Algorithm
            /// </summary>
            /// <param name="z"></param>
            /// <returns>Normal Distribution for z /<.1 </returns>
            private static double algHart(double z)
            {
                double P0, a, b, c;
                P0 = Math.Sqrt(PI / 2);
                a = (1 + Math.Sqrt(1 - 2 * PI * PI + 6 * PI)) / (2 * PI);
                b = 2 * PI * a * a;
                c = Math.Sqrt(1 + b * z * z) / (1 + a * z * z);

                return 1 - Math.Exp(-z * z / 2) / Math.Sqrt(2 * PI) / z * (1 - c / (P0 * z + Math.Sqrt(Math.Pow(P0 * z, 2) + Math.Exp(-z * z / 2) * c)));
            }

            private static double algHartGenz(double z)
            {
                double P0, P1, P2, P3, P4, P5, P6, Q0, Q1, Q2, Q3, Q4, Q5, Q6, Q7;
                double P, EXPNTL, CUTOFF, ROOTPI, ZABS;

                P0 = 220.206867912376;
                P1 = 221.213596169931;
                P2 = 112.079291497871;
                P3 = 33.912866078383;
                P4 = 6.37396220353165;
                P5 = 0.700383064443688;
                P6 = 3.52624965998911E-02;

                Q0 = 440.413735824752;
                Q1 = 793.826512519948;
                Q2 = 637.333633378831;
                Q3 = 296.564248779674;
                Q4 = 86.7807322029461;
                Q5 = 16.064177579207;
                Q6 = 1.75566716318264;
                Q7 = 8.83883476483184E-02;
                ROOTPI = 2.506628274631;
                CUTOFF = 7.07106781186547;

                ZABS = Math.Abs(z);

                if (ZABS > 37)
                    P = 0;
                else
                {
                    EXPNTL = Math.Exp(-ZABS * ZABS / 2);

                    if (ZABS < CUTOFF)
                        P = EXPNTL * ((((((P6 * ZABS + P5) * ZABS + P4) * ZABS + P3) * ZABS + P2) * ZABS + P1) * ZABS + P0) / (((((((Q7 * ZABS + Q6) * ZABS
                            + Q5) * ZABS + Q4) * ZABS + Q3) * ZABS + Q2) * ZABS + Q1) * ZABS + Q0);
                    else
                        P = EXPNTL / (ZABS + 1 / (ZABS + 2 / (ZABS + 3 / (ZABS + 4 / (ZABS + 0.65))))) / ROOTPI;
                }

                if (z > 0) P = 1 - P;
                return P;
            }

            public static double BeasleySpringer(double z)
            {
                double a0, a1, a2, A3, b1, b2, b3, b4, c0, c1, c2, C3, d1, d2, q, R, retVal;
                a0 = 2.50662823884;
                a1 = -18.61500062529;
                a2 = 41.39119773534;
                A3 = -25.44106049637;
                b1 = -8.4735109309;
                b2 = 23.08336743743;
                b3 = -21.06224101826;
                b4 = 3.13082909833;
                c0 = -2.78718931138;
                c1 = -2.29796479134;
                c2 = 4.85014127135;
                C3 = 2.32121276858;
                d1 = 3.54388924762;
                d2 = 1.63709781897;

                q = z - 0.5;
                if (Math.Abs(q) > 0.4)
                {
                    R = q * q;
                    return q * (((A3 * R + a2) * R + a1) * R + a0) / ((((b4 * R + b3) * R + b2) * R + b1) * R + 1);
                }

                R = z;
                if (q > 0) R = 1 - z;
                if (R <= 0)
                {
                    // error
                    return -1;
                }

                //	double retVal;
                R = Math.Sqrt(-Math.Log(R));
                retVal = (((C3 * R + c2) * R + c1) * R + c0) / ((d2 * R + d1) * R + 1);
                if (q < 0) retVal = -retVal;
                return retVal;
            }

            private static double gammln(double xx)
            {
                int j;
                double x, y, tmp, ser;
                /*static const*/
                double[] cof = { 76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5 };
                y = x = xx;
                tmp = x + 5.5;
                tmp -= (x + 0.5) * Math.Log(tmp);
                ser = 1.000000000190015;
                for (j = 0; j < 6; j++) ser += cof[j] / ++y;
                return -tmp + Math.Log(2.5066282746310005 * ser / x);
            }

            private static double NormalExact(double z)
            {
                //Using double factorial
                // (2n-1)!! = Gamma(n+0.5).2^n / sqrt(PI)

                // fairly accurate to
                double ln2 = Math.Log(2);
                double sum = 0;
                for (double k = 0; k < 250; k++)
                {
                    sum += Math.Exp(-z * z / 2 + (2 * k + 1) * Math.Log(z) + (k + 1) * ln2 + gammln(k + 2) - gammln(2 * k + 3));
                }
                return 0.5 - sum / Math.Sqrt(2 * PI);
            }

            #endregion Private Normal Routines
        }

        /// <summary>
        /// Class for calculating the chi squared distribution function (and inverse)
        /// </summary>
        public class Chi2Dist
        {
            /*	FUNCTION ChiDist: probability of chi sqaure value
                ALGORITHM Compute probability of chi square value.
                Adapted from:
                    Hill, I. D. and Pike, M. C.  Algorithm 299
                    Collected Algorithms for the CACM 1967 p. 243
                Updated for rounding errors based on remark in
                    ACM TOMS June 1985, page 185
            */

            public static double Dist(double x, int deg_freedom)
            {
                double LOG_SQRT_PI = 0.5723649429247000870717135; /* log (sqrt (pi)) */
                double I_SQRT_PI = 0.5641895835477562869480795; /* 1 / sqrt (pi) */
                double BIGX = 20.0; /* max value to represent exp (x) */

                double a, y = 0, s;
                double e, c, z;
                bool even;     /* true if deg_freedom is an even number */

                if (x <= 0.0 || deg_freedom < 1)
                    return (1.0);

                a = 0.5 * x;
                even = (bool)(deg_freedom % 2d == 0);
                if (deg_freedom > 1)
                    y = Math.Exp(-a);
                s = (even ? y : (2.0 * Normals.NormalDist(-Math.Sqrt(x))));
                if (deg_freedom > 2)
                {
                    x = 0.5 * (deg_freedom - 1.0);
                    z = (even ? 1.0 : 0.5);
                    if (a > BIGX)
                    {
                        e = (even) ? 0.0 : LOG_SQRT_PI;
                        c = Math.Log(a);
                        while (z <= x)
                        {
                            e = Math.Log(z) + e;
                            s += Math.Exp(c * z - a - e);
                            z += 1.0;
                        }
                        return (s);
                    }
                    else
                    {
                        e = (even ? 1.0 : (I_SQRT_PI / Math.Sqrt(a)));
                        c = 0.0;
                        while (z <= x)
                        {
                            e = e * (a / z);
                            c = c + e;
                            z += 1.0;
                        }
                        return (c * y + s);
                    }
                }
                else
                    return (s);
            }

            /*	FUNCTION critchi: compute critical chi square value to produce given p
                ALGORITHM compute critical chi square value to produce given p
                Adapted from:
                    Hill, I. D. and Pike, M. C.  Algorithm 299
                    Collected Algorithms for the CACM 1967 p. 243
            */

            public static double Inv(double prob, int deg_freedom)
            {
                const double CHI_EPSILON = 0.000001;		/* accuracy of critchi approximation */
                const double CHI_MAX = 99999.0;		/* maximum chi square value */

                double minchisq = 0.0;
                double maxchisq = CHI_MAX;
                double chisqval;

                if (prob <= 0.0)
                    return (maxchisq);
                else if (prob >= 1.0)
                    return (0.0);

                chisqval = deg_freedom / Math.Sqrt(prob);    /* fair first value */
                while (maxchisq - minchisq > CHI_EPSILON)
                {
                    if (Chi2Dist.Dist(chisqval, deg_freedom) < prob)
                        maxchisq = chisqval;
                    else
                        minchisq = chisqval;
                    chisqval = (maxchisq + minchisq) * 0.5;
                }
                return (chisqval);
            }
        }
    }
}