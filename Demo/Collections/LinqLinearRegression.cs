using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SystemExtensions.Primitives;

namespace SystemExtensions.Collections
{
    public class LinqLinearRegression
    {
        public LinqLinearRegression(IEnumerable<System.Data.DataRow> datasource, 
            string yValueMember,string xValueMember, string dateValueMember)
            : this()
        {
            this.XValueMember = xValueMember;
            this.YValueMember = yValueMember;
            this.DateMember = dateValueMember;
            this.DataSource = datasource;
        }
        public LinqLinearRegression(DataTable table) : this()
        {
            this.DataSource = table.AsEnumerable();
        }
        public LinqLinearRegression()
        {
            /*Defaults*/
            this.XValueMember = "x";
            this.YValueMember = "y";
            this.DateMember = "Date";

            this.CheckSign = false;
            this.ExpectedSign = 0;

            this.ResultsSet = new List<LinqLinearRegressionResults>();
        }
       public IEnumerable<System.Data.DataRow> DataSource { get; set; }

       public string YValueMember { get; set; }
       public string XValueMember { get; set; }
       public string DateMember { get; set; }

       public int MaxWindowYears { get; set; }
       public int MinWindowYears { get; set; }

       public double XValMin { get; private set; }
       public double XValMax { get; private set; }
       public double YValMin { get; private set; }
       public double YValMax { get; private set; }

       public List<LinqLinearRegressionResults> ResultsSet { get; private set; }
        public LinqLinearRegressionResults LastResult {get;private set;}

        public bool HasResults
        {
            get
            {
                return ResultsSet.Count > 0;
            }
        }
        public int ExpectedSign { get; set; }
       private  void Regress(DateTime dateMin, DateTime dateMax)
       {
           var timeSeries = this.DataSource.AsEnumerable()
                   .Select(f => new
                   {
                       Date = Convert.ToDateTime(f[this.DateMember]),
                       YValue = Convert.ToDouble(f[this.YValueMember]),
                       XValue = Convert.ToDouble(f[this.XValueMember])
                   })
                       .Where(f => f.Date >= dateMin)
                       .Where(f => f.Date <= dateMax)
                       //.Where(f => Math.Sign(f.YValue) == Math.Sign(f.XValue * ExpectedSign) || ExpectedSign==0)
                   .OrderBy(g => g.Date);
                    
           int countNonZero = timeSeries.Where(f => f.XValue != 0).Count();
           if (countNonZero < this.MinNonZeroRegressands)
               return;

           dateMin = timeSeries.Min(points => points.Date);
           dateMax = timeSeries.Max(points => points.Date);

           /*useful for any charting*/
           this.XValMin = timeSeries.Min(points => points.XValue);
           this.XValMax = timeSeries.Max(points => points.XValue);
           this.YValMin = timeSeries.Min(points => points.YValue);
           this.YValMax = timeSeries.Max(points => points.YValue);

           double T = timeSeries.Count();
           var xMean = timeSeries.Average(points => points.XValue);
           var yMean = timeSeries.Average(points => points.YValue);
           var xVariance = timeSeries.Sum(points => Math.Pow(points.XValue - xMean, 2.0)) / (T - 1);
           var yVariance = timeSeries.Sum(points => Math.Pow(points.YValue - yMean, 2.0)) / (T - 1);
           double Correlation = timeSeries.Sum(points => (points.XValue - xMean) * (points.YValue - yMean))
               / (T - 1) / Math.Sqrt(xVariance * yVariance);

           double alpha, beta, RSS;

           beta = Correlation * Math.Sqrt(yVariance / xVariance);
           alpha = yMean - beta * xMean;

           var yForecast = timeSeries.Select(points => new { Date = points.Date, YHat = alpha + beta * points.XValue });
           var errors = timeSeries.Join(yForecast, yObs => yObs.Date, f => f.Date, (yObs, yHat) => new { Date = yHat.Date, Err = yObs.YValue - yHat.YHat });
           RSS = errors.Sum(err => err.Err * err.Err);

           var beta_tstat = beta * Math.Sqrt((T - 2) / (RSS / (xVariance * (T - 1))));
           var alpha_tstat = beta_tstat * alpha / beta * Math.Sqrt(T / timeSeries.Sum(points => points.XValue * points.XValue));

           this.LastResult = new LinqLinearRegressionResults();

           LastResult.LastXValue = timeSeries.Where(r => r.Date == dateMax)
                                            .Select(r => r.XValue)
                                            .Single();

           LastResult.Alpha = alpha;
           LastResult.Beta = beta;
           LastResult.Correlation = Correlation;
           LastResult.T = T;
           LastResult.RSS = RSS;
           LastResult.TStatAlpha = alpha_tstat;
           LastResult.TStatBeta = beta_tstat;
           LastResult.RegressionStart = dateMin;
           LastResult.RegressionEnd = dateMax;
           LastResult.XStdDev = Math.Sqrt(xVariance);
           LastResult.YStdDev = Math.Sqrt(yVariance);

           this.ResultsSet.Add(LastResult);
       }
       public int MinNonZeroRegressands { get; set; }
       public bool CheckSign { get; set; }
       public void Regress(eRegressionWindowStyle eRegressionWindowStyle)
       {
           DateTime[] dates = this.DataSource.AsEnumerable()
            .Select(rows => Convert.ToDateTime(rows[this.DateMember]))
            .OrderBy(f => f.Date)
            .ToArray();
           
           if (dates.Count() == 0)
               return;

           DateTime minDate = dates.Min();
           this.ResultsSet = new List<LinqLinearRegressionResults>();
           DateTime lastDate = DateTime.MinValue;

           // we don't have any pre-perception on the expected sign of beta.
           ExpectedSign = 0;

           if (CheckSign)
           {
               // regress over the full sample
               Regress(minDate, DateTime.MaxValue);

               // if the beta is significant(ly signed) then set the expected sign.
               // any observations that don't match the expected sign will be discarded
               if (this.LastResult != null && this.LastResult.ProbBeta < 0.1)
                   ExpectedSign = Math.Sign(this.LastResult.Beta);

               this.ResultsSet.Clear();
           }

           switch (eRegressionWindowStyle)
           {

               case eRegressionWindowStyle.None:
                   Regress(minDate, DateTime.MaxValue);
                   break;
               case eRegressionWindowStyle.Expanding:

                   
                   foreach (var date in dates.Where(d => d.Date > minDate.AddYears(this.MinWindowYears)))
                   {
                       if (date > lastDate)
                           this.Regress(minDate, date);

                       /*output the results*/
                   }
                         break;
               case eRegressionWindowStyle.Rolling:
                         
                         foreach (var date in dates.Where(d => d.Date > minDate.AddYears(this.MinWindowYears)).OrderBy(d=>d.Date))
                         {
                             if(date>lastDate)
                             // MaxWindow years + 1 month
                             this.Regress(date.AddMonths(-(this.MaxWindowYears*12+1)), date);

                             /*output the results*/
                         }
                   break;
           }
       }
       public enum eRegressionWindowStyle
       {
           None, Expanding, Rolling
       }

       public class LinqLinearRegressionResults
       {
           public DateTime RegressionStart { get; internal set; }
           public DateTime RegressionEnd { get; internal set; }
           public double T { get; internal set; }
           public double LastXValue { get; internal set; }
           public double Correlation { get; internal set; }
           public double Alpha { get; internal set; }
           public double Beta { get; internal set; }
           public double RSS { get; internal set; }
           public double YStdDev { get; internal set; }
           public double XStdDev { get; internal set; }
           public double TStatAlpha { get; internal set; }
           public double TStatBeta { get; internal set; }
           public double ProbAlpha
           {
               get
               {
                   if (!TStatAlpha.IsNumeric())
                       return double.NaN;
                   return 1.0 - MathLib.Distributions.tcdf(TStatAlpha, T - 2);
               }
           }
           public double ProbBeta 
           { 
               get 
               {
                   if (!TStatBeta.IsNumeric())
                       return double.NaN;

                   return 1.0 - MathLib.Distributions.tcdf(TStatBeta, T - 2); 
               } 
           }
       }

       internal string ResultsAsText()
       {
           StringBuilder sb = new StringBuilder();
           try
           {
               if (ResultsSet.Count == 0)
                   sb.AppendFormat("There is not enough data to run the full sample regression");
               else
               {
                   sb.AppendFormat("Regression from {0:d} to {1:d}", ResultsSet[0].RegressionStart, ResultsSet[0].RegressionEnd);
                   sb.AppendLine();
                   sb.AppendFormat("{0} observations. RSS {1:f3}. Correlation {2:f3}", LastResult.T, LastResult.RSS, LastResult.Correlation);
                   sb.AppendLine();
                   sb.AppendLine("y = α + βx");
                   sb.AppendFormat("α: {0:g4},  t-stat: {1:g4} ({2:f3})", LastResult.Alpha, LastResult.TStatAlpha, LastResult.ProbAlpha);
                   sb.AppendLine();
                   sb.AppendFormat("Raw regressand:   β: {0:g4},  t-stat: {1:g4} ({2:f3})", LastResult.Beta, LastResult.TStatBeta, LastResult.ProbBeta);
                   sb.AppendLine();
                   sb.AppendFormat("Normalised regressand: β: {0:g4},  t-stat: {1:g4} ({2:f3})", LastResult.Beta * LastResult.XStdDev, LastResult.TStatBeta, LastResult.ProbBeta);
                   sb.AppendLine();
                   sb.AppendFormat("ecostat standard deviation: {0:g4}",  LastResult.XStdDev);
               }
           }
           catch(Exception e)
           {
               sb.AppendLine("Error: " + e.Message);
           }

           return sb.ToString();
       }
    }
}
