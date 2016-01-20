using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemExtensions.Data;
using SystemExtensions.Collections;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable table = new DataTable();
            table.FillTable("Server=myserver;Database=mydatabase;trusted_connection=TRUE;", "web.getSomeData", CommandType.StoredProcedure, 
                new Tuple<string, object>("Latitude",123));

            table.Upsert("", "");

            table.SaveCSV("check.csv");

            // double[,] pivot = table.AsEnumerable().Pivot(r => r["Date"], r => r["Column"], r => (double) r["value"]);

            LinqLinearRegression reg = new LinqLinearRegression(table.AsEnumerable(), "YValue", "XValue", "Date");
            //reg.LastResult.Beta = 1;
        }
    }
}
