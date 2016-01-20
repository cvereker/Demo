// BaselineVersion= Post strange Tortoise behaviour  Date=8/9/2005 9:46:51 AM
using System;

namespace MathLib
{
    /// <summary>
    /// Summary description for GammaFunction.
    /// </summary>
    public class SpecialFunctions
    {
        private static double[] _gammCoef = new double[]        {       76.18009172947146,-86.50532032941677,
                                                        24.01409824083091,-1.231739572450155,0.1208650973866179e-2,
                                                        -0.5395239384953e-5};

        /// <summary>
        /// Returns the value ln[Gamma(z)] for z>0. Uses Lanczos formula.
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public static double gammln(double z)
        {
            int j;
            double x, y, tmp, ser;

            y = x = z;
            tmp = x + 5.5;
            tmp -= (x + 0.5) * Math.Log(tmp);
            ser = 1.000000000190015;
            for (j = 0; j < 6; j++) ser += _gammCoef[j] / ++y;
            return -tmp + Math.Log(2.5066282746310005 * ser / x);
        }

        public static double Gamma(double z)
        {
            return Math.Exp(gammln(z));
        }

        public static double Beta(double z, double w)
        {
            return Math.Exp(gammln(z) + gammln(w) - gammln(z + w));
        }

        public static double betai(double a, double b, double x)
        {
            double bt;

            if (x < 0.0 || x > 1.0)
                throw new Exception("Bad x in routine betai");
            if (x == 0.0 || x == 1.0)
                bt = 0.0;
            else
                bt = Math.Exp(gammln(a + b) - gammln(a) - gammln(b) + a * Math.Log(x) + b * Math.Log(1.0 - x));

            if (x < (a + 1.0) / (a + b + 2.0))
                return bt * betacf(a, b, x) / a;
            else
                return 1.0 - bt * betacf(b, a, 1.0 - x) / b;
        }

        public static double betacf(double a, double b, double x)
        {
            const int MAXIT = 100;
            const double EPS = 1E-10; //numeric_limits<DP>::epsilon();
            const double FPMIN = double.MinValue / EPS; //numeric_limits<DP>::min()/EPS;
            int m, m2;
            double aa, c, d, del, h, qab, qam, qap;

            qab = a + b;
            qap = a + 1.0;
            qam = a - 1.0;
            c = 1.0;
            d = 1.0 - qab * x / qap;
            if (Math.Abs(d) < FPMIN) d = FPMIN;
            d = 1.0 / d;
            h = d;
            for (m = 1; m <= MAXIT; m++)
            {
                m2 = 2 * m;
                aa = m * (b - m) * x / ((qam + m2) * (a + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < FPMIN) d = FPMIN;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < FPMIN) c = FPMIN;
                d = 1.0 / d;
                h *= d * c;
                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < FPMIN) d = FPMIN;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < FPMIN) c = FPMIN;
                d = 1.0 / d;
                del = d * c;
                h *= del;
                if (Math.Abs(del - 1.0) <= EPS) break;
            }
            if (m > MAXIT) throw new Exception("a or b too big, or MAXIT too small in betacf");
            return h;
        }
    }
}