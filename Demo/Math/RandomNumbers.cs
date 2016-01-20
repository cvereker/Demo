namespace MathLib
{
    /// <summary>
    /// Computes the Faure low-discrepancy sequence. A QMC simulation will converge with order 1/T as opposed to the 1/sqrt(T) convergence observed with standard random number generators
    /// See: Faure, H. Discrepance de suites associees a un systeme de numeration (en dimensions). Acta Arith. 41, 337-351 1982.
    /// (Discrepancy of sequences associated with a number system (in dimension s))
    /// </summary>
    public class FaureSequence
    {
        /// <summary>
        /// Default constructor - uses base 16
        /// </summary>
        public FaureSequence()
        {
        }

        /// <summary>
        /// Overloaded constructor, which allows you to set the base
        /// </summary>
        /// <param name="SequenceBase"></param>
        public FaureSequence(int SequenceBase)
        {
            _faurebase = SequenceBase;
        }

        /// <summary>
        /// Get the next random number
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            double n = _faurebase++;
            //   Returns the equivalent first Faure sequence number
            double f, sb;
            int i, n1, n2;

            n1 = (int)n;
            f = 0;
            sb = 0.5;

            while (n1 > 0)
            {
                n2 = (int)(n1 / 2D);
                i = n1 - n2 * 2;
                f = f + sb * i;
                sb = sb / 2D;
                n1 = n2;
            }
            return f;
        }

        public double NextNormDeviate()
        {
            return Normals.BeasleySpringer(NextDouble());
            //return Normals.MoroNormSInv(this.NextDouble());
        }

        /// <summary>
        /// Set the base
        /// </summary>
        public double FaureBaseNumber
        {
            set
            {
                _faurebase = value;
            }
            get
            {
                return _faurebase;
            }
        }

        private double _faurebase = 16;
    }
}