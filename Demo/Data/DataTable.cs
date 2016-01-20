using SystemExtensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SystemExtensions
{
    public static partial class DataTableExtensions
    {
        #region CreateTable (static methods)

        public static void SetPrimaryKey(this DataTable table, string FieldList)
        {
            table.PrimaryKey = table.GetColumns(FieldList);
        }

        public static DataColumn SetAutoIncrementColumn(this DataTable table, string Field, int seed, int step)
        {
            table.Columns[Field].AutoIncrement = true;
            table.Columns[Field].AutoIncrementSeed = seed;
            table.Columns[Field].AutoIncrementStep = step;
            return table.Columns[Field];
        }

        public static DataColumn SetAutoIncrementColumn(this DataTable table, string Field)
        {
            return table.SetAutoIncrementColumn(Field, 1, 1);
        }

        /// <summary>
        /// Creates a new table
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        /// <remarks>
        /// Creates a new table named TableName. The fields (columns) are seperated by commas. Each
        /// field can have one, two, or three parts, seperated by spaces.
        /// The first part is always the field name.
        /// The second part is always the type (case insenstivie)
        /// The third part is an expression. Any ADO.NET acceptable expression is valid, and additionally
        /// the string "required" (case insensitive) which sets the coumn AllowDBNull parameter to false
        /// </remarks>
        //public static DataTable CreateTable(string FieldList)
        //{
        //    DataTable dt = new DataTable();
        //    return dt.CreateTable(FieldList);
        //}
        public static DataTable CreateTable(List<DataColumn> columns)
        {
            DataTable dt = new DataTable();
            foreach (DataColumn srcCol in columns)
            {
                dt.Columns.Add(srcCol.ColumnName, srcCol.DataType);
                dt.Columns[srcCol.ColumnName].Caption = srcCol.Caption;
            }
            return dt;
        }

        /// <summary>
        /// Creates a new table
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        /// <remarks>
        /// Creates a new table named TableName. The fields (columns) are seperated by commas. Each
        /// field can have one, two, or three parts, seperated by spaces.
        /// The first part is always the field name.
        /// The second part is always the type (case insenstivie)
        /// The third part is an expression. Any ADO.NET acceptable expression is valid, and additionally
        /// the string "required" (case insensitive) which sets the coumn AllowDBNull parameter to false
        /// </remarks>

        #region Not sure where this comes from and if it is correct?

        public static DataTable CreateTable(string FieldList)
        {
            DataTable dt = new DataTable();
            DataColumn dc;
            string[] Fields = FieldList.Split(new string[] { ",", " AND " }, StringSplitOptions.None);
            string[] FieldsParts;
            string Expression;
            foreach (string Field in Fields)
            {
                FieldsParts = Field.Trim().Split(" ".ToCharArray(), 3); // allow for spaces in the expression

                if (FieldsParts.Length == 1)
                {
                    // default fields are integer
                    dc = dt.Columns.Add(FieldsParts[0].Trim(), typeof(Int32));
                    dc.AllowDBNull = true;
                }
                // add fieldname and datatype
                else if (FieldsParts.Length == 2)
                {
                    dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
                    dc.AllowDBNull = true;
                }
                // add fieldname, datatype, and expression
                else if (FieldsParts.Length == 3)
                {
                    Expression = FieldsParts[2].Trim();
                    if (FieldsParts[1] == "=")
                    {
                        if (FieldsParts[2].Contains("#"))
                        {
                            // default fields with a date
                            dc = dt.Columns.Add(FieldsParts[0].Trim(), typeof(DateTime));
                            string def = FieldsParts[2];
                            def = def.Replace('#', ' ');
                            def = def.Trim();
                            dc.DefaultValue = DateTime.Parse(def);
                        }
                        else
                        {
                            // default fields are integer with a default value
                            dc = dt.Columns.Add(FieldsParts[0].Trim(), typeof(Int32));
                            dc.DefaultValue = FieldsParts[2];
                        }
                        dc.AllowDBNull = true;
                    }
                    else if (Expression.ToUpper() == "REQUIRED")
                    {
                        dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
                        dc.AllowDBNull = false;
                    }
                    else
                    {
                        dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true), Expression);
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid field definition: '" + Field + "'.");
                }
            }
            return dt;
        }

        #endregion Not sure where this comes from and if it is correct?

        public static DataTable CreateTable(this DataTable SourceTable, string FieldList)
        {
            DataTable dt;
            dt = new DataTable();

            if (FieldList == null || FieldList == "" || FieldList == "*")
            {
                foreach (DataColumn srcCol in SourceTable.Columns)
                {
                    dt.Columns.Add(srcCol.ColumnName, srcCol.DataType);
                    dt.Columns[srcCol.ColumnName].Caption = srcCol.Caption;
                }
                return dt;
            }
            else
            {
                FieldParser fp = new FieldParser();
                fp.ParseFieldList(FieldList, false);

                DataColumn dc;
                foreach (FieldInfo Field in fp.FieldInfo)
                {
                    dc = SourceTable.Columns[Field.FieldName];
                    if (dc == null)
                        dt.Columns.Add(Field.FieldAlias, typeof(Int32));
                    else
                    {
                        dt.Columns.Add(Field.FieldAlias, dc.DataType);
                        dt.Columns[Field.FieldAlias].Caption = dc.Caption;
                    }
                }

                return dt;
            }
        }

        /// <summary>
        /// Creates a new table
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldList"></param>
        /// <param name="KeyFieldList"></param>
        /// <returns></returns>
        public static DataTable CreateTable(string FieldList, string KeyFieldList)
        {
            DataTable table = SystemExtensions.DataTableExtensions.CreateTable(FieldList);
            table.SetPrimaryKey(KeyFieldList);
            return table;
        }

        #endregion CreateTable (static methods)

        public static DataColumn[] GetColumns(this DataTable table, string FieldList)
        {
            string[] Fields = FieldList.Split(new string[] { ",", "AND" }, StringSplitOptions.None);

            DataColumn[] FK = new DataColumn[Fields.Length];

            for (int f = 0; f < Fields.Length; f++)
            {
                string Field = Fields[f].Trim();
                if (Field.Contains("="))
                {
                    Field = Field.Split('=')[0].Trim();
                }
                FK[f] = table.Columns[Field];
            }
            return FK;
        }

        public static void SetDefaults(this DataTable table, string FieldsAndValues)
        {
            // this makes it easy to pass a filter into the function
            // i.e. TimeSeriesID = 1 AND NameID = 98 goes to TimeSeriesID = 1, NameID = 98
            if (FieldsAndValues.Contains(" AND "))
                FieldsAndValues = FieldsAndValues.Replace("AND", ",");

            // split by
            string[] Fields = FieldsAndValues.Split(',');
            string[] FieldsParts;
            string Expression;
            foreach (string Field in Fields)
            {
                FieldsParts = Field.Trim().Split(" ".ToCharArray(), 3);

                if (FieldsParts.Length == 3)
                {
                    Expression = FieldsParts[2].Trim();
                    object value;
                    if (FieldsParts[1] == "=")
                    {
                        if (Expression.Contains("#"))
                        {
                            // default fields with a date
                            Expression = Expression.Replace('#', ' ');
                            Expression = Expression.Trim();
                            value = DateTime.Parse(Expression);
                        }
                        else
                        {
                            // default fields are integer with a default value
                            int iValue; double dValue;
                            if (int.TryParse(Expression, out iValue))
                                value = iValue;
                            else if (double.TryParse(Expression, out dValue))
                                value = dValue;
                            else
                                value = Expression;
                        }
                        if (table.Columns.Contains(FieldsParts[0].Trim()))
                            table.Columns[FieldsParts[0].Trim()].DefaultValue = value;
                    }
                }
            }
        }

        public static void SetDefault(this DataTable table, string defaultField, int defaultValue)
        {
            table.SetDefaults(defaultField + " = " + defaultValue);
        }

        public static object GetValue(this DataTable table, int row, string column)
        {
            object value = table.Rows[row][column];

            if (value is short)
                return (int)((short)value);

            return value;
        }

        public static bool ContainsMatchingRow(this DataTable table, string filter)
        {
            DataView dv = new DataView(table);
            dv.RowFilter = filter;
            return (dv.Count > 0);
        }

        #region Static Helper methods

        public static T GetValue<T>(DataRow dr, string column)
        {
            object value = dr[column];

            if (value == DBNull.Value)
                return default(T);
            if (value is short)
                //return (int)((short)value);
                value = (int)((short)value);
            if (value is byte)
                value = (int)((byte)value);

            return (T)value;
        }

        public static T GetValue<T>(DataRowView drv, string column)
        {
            object value = drv[column];

            if (value == DBNull.Value)
                return default(T);
            if (value is short)
                //return (int)((short)value);
                value = (int)((short)value);
            if (value is byte)
                value = (int)((byte)value);

            return (T)value;
        }

        #endregion Static Helper methods

        #region Print/Table->String functions

        public static string ToString(this DataTable table)
        {
            return ToString(table, true);
        }

        public static string ToString(this DataTable table, bool includeHeaders)
        {
            return ToString(table, ",", includeHeaders);
        }

        public static string ToString(this DataTable table, string sep, bool includeHeaders)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string sz = "";

            if (includeHeaders == true)
                sb.AppendFormat("{0}{1}", ColumnHeaders(table, sep), Environment.NewLine);

            foreach (DataRowView drv in table.DefaultView)
            {
                sz = "";
                for (int i = 0; i < table.Columns.Count - 1; i++)
                {
                    sz += ObjToString(drv[i]).Replace(sep, " ");
                    sz += sep;
                }
                sz += ObjToString(drv[table.Columns.Count - 1]).Replace(sep, " ");
                //sz += ObjToString(drv[table.Columns.Count - 1]);
                sb.AppendFormat("{0}{1}", sz, Environment.NewLine);
            }
            return sb.ToString();
        }

        public static string ToString(this DataTable table, int stringLength, bool includeHeaders)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string sz = "";

            if (includeHeaders == true)
                sb.AppendFormat("{0}{1}", ColumnHeaders(table, stringLength), Environment.NewLine);

            foreach (DataRowView drv in table.DefaultView)
            {
                sz = "";
                for (int i = 0; i < table.Columns.Count - 1; i++)
                {
                    sz += string.Format("{0,-" + stringLength + "} ", ObjToString(drv[i]));
                }
                sz += ObjToString(drv[table.Columns.Count - 1]);
                //sz += ObjToString(drv[table.Columns.Count - 1]);
                sb.AppendFormat("{0}{1}", sz, Environment.NewLine);
            }
            return sb.ToString();
        }

        private static string ObjToString(Object obj)
        {
            switch (obj.GetType().Name)
            {
                case "DateTime":
                    return ((DateTime)obj).ToString(C_SQLDateFormat);

                case "Double":
                    if (double.IsNaN((double)obj)) return ""; // return null as a proxy for NaN
                    return string.Format("{0:0.##########}", (double)obj);

                case "Int32":
                    return string.Format("{0}", (int)obj);

                case "Boolean":
                    return ((bool)obj) == true ? "1" : "0";

                default:
                    return string.Format("{0}", obj.ToString());
            }
        }

        public static string ColumnHeaders(this DataTable table)
        {
            return table.ColumnHeaders(",");
        }

        public static string ColumnHeaders(this DataTable table, string sep)
        {
            string sz = "";
            string header;
            for (int i = 0; i < table.Columns.Count - 1; i++)
            {
                // replace commas in the headers (as they cause problems!!)
                header = table.Columns[i].ColumnName.Replace(',', ' ');
                sz += string.Format("{0}{1}", header, sep);
            }

            header = table.Columns[table.Columns.Count - 1].ColumnName.Replace(',', ' ');
            sz += string.Format("{0}", header);
            return sz;
        }

        public static string ColumnHeaders(this DataTable table, int stringLength)
        {
            string sz = "";
            string header;
            for (int i = 0; i < table.Columns.Count - 1; i++)
            {
                // replace commas in the headers (as they cause problems!!)
                header = table.Columns[i].ColumnName.Replace(',', ' ');
                sz += string.Format("{0,-" + stringLength + "} ", header);
            }

            header = table.Columns[table.Columns.Count - 1].ColumnName.Replace(',', ' ');
            sz += string.Format("{0,-" + stringLength + "} ", header);
            return sz;
        }

        public static string ColumnHeadersWithType(this DataTable table)
        {
            string sz = "";
            for (int i = 0; i < table.Columns.Count - 1; i++)
                sz += string.Format("{0} {1},", table.Columns[i].ColumnName, table.Columns[i].DataType.Name);

            sz += string.Format("{0} {1}", table.Columns[table.Columns.Count - 1].ColumnName, table.Columns[table.Columns.Count - 1].DataType.Name);
            return sz;
        }

        public static string ColumnFilterString(this DataTable table, string fields)
        {
            string[] Fields = fields.Split(',');
            for (int k = 0; k < Fields.Length; k++)
                Fields[k] = Fields[k].Trim();

            DataColumn column;
            string sz = "";
            int i = 0;
            for (; i < Fields.Length - 1; i++)
            {
                column = table.Columns[Fields[i]];
                if (column.DataType == typeof(DateTime))
                    sz += string.Format("{0} = #{{{1}:d}}# AND ", column.ColumnName, i);
                else if (column.DataType == typeof(string))
                    sz += string.Format("{0} = '{{{1}}}' AND ", column.ColumnName, i);
                else
                    sz += string.Format("{0} = {{{1}}} AND ", column.ColumnName, i);
            }

            i = Fields.Length - 1;
            column = table.Columns[Fields[i]];
            if (column.DataType == typeof(DateTime))
                sz += string.Format("{0} = #{{{1}:d}}#", column.ColumnName, i);
            else if (column.DataType == typeof(string))
                sz += string.Format("{0} = '{{{1}}}'", column.ColumnName, i);
            else
                sz += string.Format("{0} = {{{1}}}", column.ColumnName, i);
            return sz;
        }

        private const string C_SQLDateFormat = "yyyy-MM-dd";// HH:mm:ss.ffff";

        public static string ToString(this DataTable table, int rowCount)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string sz = "";
            for (int i = 0; i < table.Columns.Count - 1; i++)
                sz += string.Format("{0},", table.Columns[i].ColumnName);
            sz += string.Format("{0}", table.Columns[table.Columns.Count - 1].ColumnName);

            sb.AppendFormat("{0}\n", sz);

            DataRowView drv;

            rowCount = Math.Min(rowCount, table.DefaultView.Count);

            for (int i, j = 0; j < rowCount; j++)
            {
                drv = table.DefaultView[j];
                sz = "";

                for (i = 0; i < table.Columns.Count - 1; i++)
                {
                    sz += string.Format("{0},", (drv[i] is DateTime) ? ((DateTime)drv[i]).ToString(C_SQLDateFormat) : drv[i].ToString());
                }
                i = table.Columns.Count - 1;

                sz += string.Format("{0},", (drv[i] is DateTime) ? ((DateTime)drv[i]).ToString(C_SQLDateFormat) : drv[i].ToString());
                sb.AppendFormat("{0}\n", sz);
            }

            return sb.ToString();
        }

        public static void DebugPrint(this DataTable table)
        {
            System.Diagnostics.Debug.WriteLine(ToString(table));
        }

        public static void DebugPrint(this DataTable table, int rowCount)
        {
            System.Diagnostics.Debug.WriteLine(ToString(table, rowCount));
        }

        public static void Print(this DataTable table, System.IO.TextWriter stream)
        {
            stream.Write(ToString(table));
            stream.Flush();
        }

        public static void Print(this DataTable table, System.IO.TextWriter stream, bool includeHeaders)
        {
            stream.Write(ToString(table, includeHeaders));
            stream.Flush();
        }

        #endregion Print/Table->String functions

        #region Private methods

        private static FieldInfo LocateFieldInfoByName(System.Collections.ArrayList FieldList, string Name)
        {
            //Looks up a FieldInfo record based on FieldName
            foreach (FieldInfo Field in FieldList)
            {
                if (Field.FieldName == Name)
                    return Field;
            }
            return null;
        }

        /// <summary>
        /// Compares two values to see if they are equal. Also compares DBNULL.Value.
        /// </summary>
        /// <param name="a">First object</param>
        /// <param name="b">Second object</param>
        /// <returns>True, if the two objects are equal</returns>
        /// <remarks>Note: If your DataTable2 contains object fields, you must extend this function to handle them in a meaningful way if you intend to group on them.</remarks>
        private static bool ColumnEqual(object a, object b)
        {
            if ((a is DBNull) && (b is DBNull))
                return true;    //both are null
            if ((a is DBNull) || (b is DBNull))
                return false;    //only one is null
            if (a == null || b == null)
                return false;
            bool bRet = a.Equals(b);    //value type standard comparison
            return bRet;
        }

        /// <summary>
        /// Returns MIN of two values - DBNull is less than all others
        /// </summary>
        /// <param name="a">First object</param>
        /// <param name="b">Second object</param>
        /// <returns>The minium of the objects</returns>
        private static object Min(object a, object b)
        {
            if ((a is DBNull) || (b is DBNull))
                return DBNull.Value;
            if (((IComparable)a).CompareTo(b) == -1)
                return a;
            else
                return b;
        }

        /// <summary>
        /// Returns Max of two values - DBNull is less than all others
        /// </summary>
        /// <param name="a">First object</param>
        /// <param name="b">Second object</param>
        /// <returns>The maximum of the objects</returns>
        private static object Max(object a, object b)
        {
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;
            if (((IComparable)a).CompareTo(b) == 1)
                return a;
            else
                return b;
        }

        /// <summary>
        /// Adds two values - if one is DBNull, then returns the other
        /// </summary>
        /// <param name="a">First object</param>
        /// <param name="b">Second object</param>
        /// <returns>The sum of the objects</returns>
        private static object Add(object a, object b)
        {
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;

            if (a is double)
                return ((double)a + (double)b);
            else
                return ((decimal)a + (decimal)b);
        }

        private static object Mult(object a, object b)
        {
            if (a is DBNull)
                return DBNull.Value;
            if (b is DBNull)
                return DBNull.Value;

            if (a is double)
                return ((double)a * (double)b);
            else
                return ((decimal)a * (decimal)b);
        }

        #endregion Private methods

        #region Internal Classes

        internal class FieldInfo
        {
            public string RelationName;
            public string FieldName;	// source table field name
            public string FieldAlias;	// destination table field name
            public string Aggregate;
        }

        public class FieldParser
        {
            public FieldParser()
            {
            }

            private ArrayList _groupByFieldInfo;

            public ArrayList GroupByFieldInfo
            {
                get
                {
                    return this._groupByFieldInfo;
                }
                set
                {
                    this._groupByFieldInfo = value;
                }
            }

            private string GroupByFieldList;

            private ArrayList m_FieldInfo = new ArrayList();

            public ArrayList FieldInfo
            {
                get
                {
                    return this.m_FieldInfo;
                }
                set
                {
                    this.m_FieldInfo = value;
                }
            }

            private string m_FieldList;

            public string FieldList
            {
                get
                {
                    return this.m_FieldList;
                }
                set
                {
                    this.m_FieldList = value;
                }
            }

            #region Public methods

            /// <summary>
            /// This code parses FieldList into FieldInfo objects and then adds them to the m_FieldInfo private member
            /// </summary>
            /// <param name="FieldList">List of fields</param>
            /// <param name="AllowRelation">Allow relations</param>
            /// <remarks>To be filled in!
            /// </remarks>
            public void ParseFieldList(string FieldList, bool AllowRelation)
            {
                if (m_FieldList == FieldList)
                    return;

                m_FieldInfo = new System.Collections.ArrayList();
                m_FieldList = FieldList;
                FieldInfo Field;
                string[] FieldParts;
                string[] Fields = FieldList.Split(',');
                for (int i = 0; i <= Fields.Length - 1; i++)
                {
                    Field = new FieldInfo();

                    // Parse FieldAlias
                    FieldParts = Fields[i].Trim().Split(' ');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            // set to be at the end of the loop
                            break;

                        case 2:
                            Field.FieldAlias = FieldParts[1];
                            break;

                        default:
                            throw new Exception("Too many spaces in field definition: '" + Fields[i] + "'.");
                    }

                    // Parse FieldName and Relation Name
                    FieldParts = FieldParts[0].Split('.');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            Field.FieldName = FieldParts[0];
                            break;

                        case 2:
                            if (AllowRelation == false)
                                throw new Exception("Relation specifiers not permitted in field list: '" + Fields[i] + "'.");
                            Field.RelationName = FieldParts[0].Trim();
                            Field.FieldName = FieldParts[1].Trim();
                            break;

                        default:
                            throw new Exception("Invalid field definition: '" + Fields[i] + "'.");
                    }

                    if (Field.FieldAlias == null)
                        Field.FieldAlias = Field.FieldName;

                    m_FieldInfo.Add(Field);
                }
            }

            public bool Contains(string Field)
            {
                foreach (FieldInfo field in m_FieldInfo)
                {
                    if (field.FieldName == Field)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Parses FieldList into FieldInfo objects and adds them to the GroupByFieldInfo private member FieldList syntax: fieldname[ alias]|operatorname(fieldname)[ alias],...
            /// </summary>
            /// <param name="FieldList">The field list</param>
            /// <remarks>Supported Operators: count,sum,max,min,first,last </remarks>
            public void ParseGroupByFieldList(string FieldList)
            {
                if (GroupByFieldList == FieldList)
                    return;

                GroupByFieldInfo = new System.Collections.ArrayList();
                FieldInfo Field;

                // split the fields on the comma
                string[] FieldParts; string[] Fields = FieldList.Split(',');

                for (int i = 0; i <= Fields.Length - 1; i++)
                {
                    Field = new FieldInfo();

                    //Parse FieldAlias
                    FieldParts = Fields[i].Trim().Split(' ');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            //to be set at the end of the loop
                            break;

                        case 2:
                            Field.FieldAlias = FieldParts[1];
                            break;

                        default:
                            throw new ArgumentException("Too many spaces in field definition: '" + Fields[i] + "'.");
                    }

                    //Parse FieldName and Aggregate
                    FieldParts = FieldParts[0].Split('(');
                    switch (FieldParts.Length)
                    {
                        case 1:
                            Field.FieldName = FieldParts[0];
                            break;

                        case 2:
                            Field.Aggregate = FieldParts[0].Trim().ToLower();    //we're doing a case-sensitive comparison later
                            Field.FieldName = FieldParts[1].Trim(' ', ')');
                            break;

                        default:
                            throw new ArgumentException("Invalid field definition: '" + Fields[i] + "'.");
                    }
                    if (Field.FieldAlias == null)
                    {
                        if (Field.Aggregate == null)
                            Field.FieldAlias = Field.FieldName;
                        else
                            Field.FieldAlias = Field.Aggregate + "of" + Field.FieldName;
                    }
                    GroupByFieldInfo.Add(Field);
                }
                GroupByFieldList = FieldList;
            }

            #endregion Public methods
        }

        #endregion Internal Classes

        #region Select Into

        /// <summary>
        /// This code copies the selected rows and columns from SourceTable and
        /// inserts them into DestTable.
        /// </summary>
        /// <param name="DestTable"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="Sort"></param>
        public static DataTable InsertInto(this DataTable table, DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            FieldParser fieldParser = new FieldParser();
            if (FieldList != null && FieldList != "" && FieldList != "*")
                fieldParser.ParseFieldList(FieldList, false);
            DataRow[] drs = SourceTable.Select(RowFilter, Sort == null ? FieldList : Sort);
            DataRow DestRow;
            foreach (DataRow SourceRow in drs)
            {
                DestRow = table.NewRow();
                if (FieldList == null || FieldList == "" || FieldList == "*")
                {
                    foreach (DataColumn dc in DestRow.Table.Columns)
                    {
                        if (dc.Expression == "")
                            DestRow[dc] = SourceRow[dc.ColumnName];
                    }
                }
                else
                {
                    foreach (FieldInfo Field in fieldParser.FieldInfo)
                    {
                        DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                    }
                }
                table.Rows.Add(DestRow);
            }
            return table;
        }

        public static DataTable InsertInto(this DataTable table, DataTable SourceTable, List<DataColumn> columnsList, string RowFilter, string Sort)
        {
            DataRow[] drs = SourceTable.Select(RowFilter, Sort);
            DataRow DestRow;
            foreach (DataRow SourceRow in drs)
            {
                DestRow = table.NewRow();

                foreach (DataColumn column in columnsList)
                {
                    DestRow[column.ColumnName] = SourceRow[column.ColumnName];
                }

                table.Rows.Add(DestRow);
            }
            return table;
        }

        public static DataTable SelectInto(this DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            DataTable dt = CreateTable(SourceTable, FieldList);
            dt.InsertInto(SourceTable, FieldList, RowFilter, Sort);
            return dt;
        }

        public static DataTable SelectInto(this DataTable SourceTable, List<DataColumn> columns)
        {
            return SourceTable.SelectInto(columns, null, null);
        }

        public static DataTable SelectInto(this DataTable SourceTable, List<DataColumn> columns, string RowFilter, string Sort)
        {
            DataTable dt = DataTableExtensions.CreateTable(columns);
            dt.InsertInto(SourceTable, columns, RowFilter, Sort);
            return dt;
        }

        #endregion Select Into

        #region Groupings

        /// <summary>
        /// Copies the selected rows and columns from SourceTable and inserts them into DestTable
        /// FieldList has same format as CreateGroupByTable
        /// </summary>
        /// <example>
        /// Here is an example
        /// <code>
        /// dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");
        /// dsHelper.InsertGroupByInto(ds.Tables["OrderSummary"], ds.Tables["Orders"],	"EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max", "EmployeeID<5", "EmployeeID");
        /// </code>
        /// </example>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="GroupBy"></param>
        private static DataTable InsertGroupByFrom(this DataTable table, DataTable SourceTable, string FieldList, string RowFilter, string GroupBy)
        {
            FieldParser fieldParser = new FieldParser();
            if (FieldList == "" || FieldList == null)
            {
                FieldList = GroupBy;
            }

            //Exception<ArgumentException>.AssertTrue("You must specify at least one field in the field list.", FieldList != null);

            fieldParser.ParseGroupByFieldList(FieldList);	//parse field list
            fieldParser.ParseFieldList(GroupBy, false);			//parse field names to Group By into an arraylist

            double value;
            Hashtable sums = null, sums2 = null;

            DataRow[] drs = SourceTable.Select(RowFilter, GroupBy);
            DataRow LastSourceRow = null, DestRow = null; bool SameRow; int RowCount = 0, ZeroCount = 0;
            foreach (DataRow SourceRow in drs)
            {
                SameRow = false;
                if (LastSourceRow != null)
                {
                    SameRow = true;
                    foreach (FieldInfo Field in fieldParser.FieldInfo)
                    {
                        if (!ColumnEqual(LastSourceRow[Field.FieldName], SourceRow[Field.FieldName]))
                        {
                            SameRow = false;
                            break;
                        }
                    }
                    if (!SameRow)
                        table.Rows.Add(DestRow);
                }

                if (!SameRow)
                {
                    DestRow = table.NewRow();
                    RowCount = 0;
                    sums = new Hashtable();
                    sums2 = new Hashtable();
                }

                RowCount += 1;

                foreach (FieldInfo Field in fieldParser.GroupByFieldInfo)
                {
                    if (SourceRow[Field.FieldName] == DBNull.Value)
                    {
                        // ignore nulls
                        continue;
                    }
                    switch (Field.Aggregate)    //this test is case-sensitive
                    {
                        case null:        //implicit last
                        case "":        //implicit last
                        case "last":
                            DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            break;

                        case "first":
                            if (RowCount == 1)
                                DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            break;

                        case "count":
                            DestRow[Field.FieldAlias] = RowCount;
                            break;

                        case "countnonzero":
                            if (SourceRow[Field.FieldName] is int && (int)SourceRow[Field.FieldName] == 0)
                                ZeroCount++;
                            DestRow[Field.FieldAlias] = RowCount - ZeroCount;
                            break;

                        case "sum":
                            DestRow[Field.FieldAlias] = Add(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;

                        case "max":
                            DestRow[Field.FieldAlias] = Max(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;

                        case "min":
                            if (RowCount == 1)
                                DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            else
                                DestRow[Field.FieldAlias] = Min(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;

                        case "avg":
                            value = (double)SourceRow[Field.FieldName];
                            if (sums[Field.FieldAlias] == null)
                                sums[Field.FieldAlias] = 0.0;
                            value = (double)Add(sums[Field.FieldAlias], value);
                            sums[Field.FieldAlias] = value;
                            DestRow[Field.FieldAlias] = (double)sums[Field.FieldAlias] / RowCount;
                            break;

                        case "stdev":
                            value = (double)SourceRow[Field.FieldName];

                            if (sums[Field.FieldAlias] == null)
                                sums[Field.FieldAlias] = 0.0;
                            if (sums2[Field.FieldAlias] == null)
                                sums2[Field.FieldAlias] = 0.0;

                            sums2[Field.FieldAlias] = Add(sums2[Field.FieldAlias], Mult(value, value));
                            sums[Field.FieldAlias] = Add(sums[Field.FieldAlias], value);

                            value = (double)Mult(sums[Field.FieldAlias], sums[Field.FieldAlias]);
                            if (RowCount > 1)
                            {
                                value = (double)sums2[Field.FieldAlias] - value / RowCount;
                                if (value < 0.0000000001)
                                    value = 0;
                                DestRow[Field.FieldAlias] = (double)Math.Sqrt(value / (RowCount - 1));
                            }

                            if (DestRow[Field.FieldAlias] != DBNull.Value && double.IsNaN((double)DestRow[Field.FieldAlias]))
                                value = -1;
                            break;
                    }
                }
                LastSourceRow = SourceRow;
            }
            if (DestRow != null)
                table.Rows.Add(DestRow);

            return table;
        }

        /// <summary>
        ///  Creates a table based on aggregates of fields of another table
        ///  RowFilter affects rows before GroupBy operation. No "Having" support
        ///  though this can be emulated by subsequent filtering of the table that results
        ///  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public static DataTable CreateGroupByTable(this DataTable SourceTable, string FieldList)
        {
            DataTable groupByTable = new DataTable();

            if (FieldList == "" || FieldList == null)
            {
                return SourceTable.CreateGroupByTable(SourceTable.ColumnHeaders());
            }

            //Exception<ArgumentException>.AssertTrue("You must specify at least one field in the field list.", FieldList != null);

            FieldParser fieldParser = new FieldParser();
            fieldParser.ParseGroupByFieldList(FieldList);
            foreach (FieldInfo Field in fieldParser.GroupByFieldInfo)
            {
                DataColumn dc = SourceTable.Columns[Field.FieldName];
                if (Field.Aggregate == null)
                    groupByTable.Columns.Add(Field.FieldAlias, dc.DataType, dc.Expression);
                else if (Field.Aggregate == "count")
                    groupByTable.Columns.Add(Field.FieldAlias, typeof(Int32));
                else if (Field.Aggregate == "avg" || Field.Aggregate == "stdev")
                    groupByTable.Columns.Add(Field.FieldAlias, typeof(double));
                else
                    groupByTable.Columns.Add(Field.FieldAlias, dc.DataType);
            }

            return groupByTable;
        }

        public static DataTable SelectGroupByFrom(this DataTable SourceTable, string FieldList, string RowFilter, string GroupBy)
        {
            if (FieldList == null || FieldList == "")
                FieldList = GroupBy;

            if (FieldList == "*")
                FieldList = SourceTable.ColumnHeaders();
            DataTable dt = SourceTable.CreateGroupByTable(FieldList);

            dt.InsertGroupByFrom(SourceTable, FieldList, RowFilter, GroupBy);
            return dt;
        }

        #endregion Groupings

        #region Select Distinct

        /// <summary>
        /// Selects distinct values of the field name into a seperate table
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public static DataTable SelectDistinct(this DataTable table, string FieldName)
        {
            if (FieldName == "" || FieldName == null)
                return table;

            DataTable dt = new DataTable();
            dt.Columns.Add(FieldName, table.Columns[FieldName].DataType);

            object LastValue = null;
            DataView dv = new DataView(table);
            dv.Sort = FieldName;

            foreach (DataRowView drv in dv)
            {
                bool bt1 = ColumnEqual(LastValue, drv[FieldName]);
                if (LastValue == null || !bt1)
                {
                    LastValue = drv[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            return dt;
        }

        public static DataTable SelectDistinct(this DataTable table, string FieldName, string SelectFields)
        {
            DataTable dt = new DataTable();

            //SelectFields = string.Format("{0},{1}", FieldName, SelectFields);
            FieldParser fieldParser = new FieldParser();
            fieldParser.ParseFieldList(SelectFields, false);

            if (fieldParser.Contains(FieldName) == false)
            {
                SelectFields = string.Format("{0},{1}", FieldName, SelectFields);
            }

            dt = table.CreateTable(SelectFields);

            object LastValue = null;
            DataView dv = new DataView(table);
            dv.Sort = FieldName;

            foreach (DataRowView drv in dv)
            {
                bool bt1 = ColumnEqual(LastValue, drv[FieldName]);
                if (LastValue == null || !bt1)
                {
                    LastValue = drv[FieldName];
                    DataRow newrow = dt.NewRow();
                    foreach (FieldInfo feld in fieldParser.FieldInfo)
                    {
                        newrow[feld.FieldName] = drv[feld.FieldName];
                    }
                    dt.Rows.Add(newrow);
                }
            }
            return dt;
        }

        public static DataTable SelectDistinct(this DataTable table, string FieldName, string SelectFields, string SortField)
        {
            if (SelectFields == null)
                SelectFields = SortField;

            FieldParser fieldParser = new FieldParser();
            fieldParser.ParseFieldList(SelectFields, false);

            // add the sort field to the list, if it is not already there

            if (fieldParser.Contains(SortField) == false)
            {
                SelectFields = SelectFields + ", " + SortField;
            }

            // select the disctinct columns
            DataTable dt = table.SelectDistinct(FieldName, SelectFields);

            // for each row, copy and insert at the front of the row list, removing the original
            foreach (DataRow row in dt.Select("", SortField))
            {
                DataRow newrow = dt.NewRow();
                newrow.ItemArray = (object[])row.ItemArray.Clone();
                dt.Rows.Remove(row);
                dt.Rows.InsertAt(newrow, dt.Rows.Count);
            }
            return dt;
        }

        #endregion Select Distinct

        #region Merge

        public static DataTable MergeInto(DataTable A, DataTable B, params string[] JoinColumnNames)
        {
            if (B == null)
                return A;

            int N = JoinColumnNames.Length;

            // Set the Primary Keys
            DataColumn[] A_PK = new DataColumn[N];
            DataColumn[] B_PK = new DataColumn[N];
            for (int i = 0; i < N; i++)
            {
                A_PK[i] = A.Columns[JoinColumnNames[i]];
                B_PK[i] = B.Columns[JoinColumnNames[i]];

                //Exception<InvalidConstraintException>.AssertTrue(
                //    string.Format("Table A ({0}) does not contain a column named {1}.", A.TableName, JoinColumnNames[i]),
                //    A_PK[i] != null);

                //Exception<InvalidConstraintException>.AssertTrue(
                //    string.Format("Table B ({0}) does not contain a column named {1}.", B.TableName, JoinColumnNames[i]),
                //    B_PK[i] != null);
            }
            A.PrimaryKey = A_PK;
            B.PrimaryKey = B_PK;

            Hashtable ColumnMap = new Hashtable();
            foreach (DataColumn B_col in B.Columns)
            {
                // we don't want to copy over the PK columns!
                if (Array.IndexOf(B.PrimaryKey, (object)B_col) >= 0)
                    continue;

                // add the new column to table A
                DataColumn A_col = A.Columns.Add(B_col.ColumnName, B_col.DataType);
                A_col.DefaultValue = DBNull.Value;

                // keep a map of B column indices to A column indices
                ColumnMap.Add(B.Columns.IndexOf(B_col), A.Columns.IndexOf(A_col));
            }

            DataRelation relation = new DataRelation(null, A.PrimaryKey, B.PrimaryKey);

            foreach (DataRow B_row in B.Rows)
            {
                DataRow A_row = B_row.GetParentRow(relation);

                // if the row in B does not exist in A then create a blank row
                if (A_row == null)
                {
                    A_row = A.NewRow();
                }

                // copy over the data
                IDictionaryEnumerator iter = ColumnMap.GetEnumerator();
                while (iter.MoveNext() != false)
                {
                    int nB = (int)iter.Key;
                    int nA = (int)iter.Value;
                    A_row[nA] = B_row[nB];
                }
            }

            return A;
        }

        #endregion Merge

        #region Joins

        /// <summary>
        /// Creates a table based on fields of another table and related parent tables
        /// FieldList syntax: [relationname.]fieldname[ alias][,[relationname.]fieldname[ alias]]...
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public static DataTable CreateJoinTable(this DataTable SourceTable, string FieldList)
        {
            //Exception<ArgumentException>.AssertTrue("You must specify at least one field in the field list.", FieldList != null);

            DataTable dt = new DataTable();
            FieldParser fp = new FieldParser();
            fp.ParseFieldList(FieldList, true);
            foreach (FieldInfo Field in fp.FieldInfo)
            {
                if (Field.RelationName == null)
                {
                    DataColumn dc = SourceTable.Columns[Field.FieldName];
                    dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                }
                else
                {
                    DataColumn dc = SourceTable.ParentRelations[Field.RelationName].ParentTable.Columns[Field.FieldName];
                    dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                }
            }
            return dt;
        }

        /// <summary>
        /// Copies the selected rows and columns from SourceTable and inserts them into this table
        /// FieldList has same format as CreatejoinTable
        /// </summary>
        /// <param name="DestTable"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="Sort"></param>
        public static DataTable InsertJoinInto(this DataTable table, DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            //Exception<ArgumentException>.AssertTrue("You must specify at least one field in the field list.", FieldList != null);

            FieldParser fieldParser = new FieldParser();
            fieldParser.ParseFieldList(FieldList, true);

            foreach (DataRow SourceRow in SourceTable.Select(RowFilter, Sort))
            {
                DataRow DestRow = table.NewRow();
                foreach (FieldInfo Field in fieldParser.FieldInfo)
                {
                    if (Field.RelationName == null)
                    {
                        DestRow[Field.FieldName] = SourceRow[Field.FieldName];
                    }
                    else
                    {
                        DataRow ParentRow = SourceRow.GetParentRow(Field.RelationName);
                        DestRow[Field.FieldName] = ParentRow[Field.FieldName];
                    }
                }
                table.Rows.Add(DestRow);
            }
            return table;
        }

        /// <summary>
        /// Selects sorted, filtered values from one DataTable2 to another.
        /// Allows you to specify relationname.fieldname in the FieldList to include fields from
        /// a parent table. The Sort and Filter only apply to the base table and not to related tables.
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="Sort"></param>
        /// <returns></returns>
        /// <remarks>
        /// See the original <a href="http://support.microsoft.com/default.aspx?scid=kb;en-us;326009#0">MSDN article</a>.
        /// <see href="http://support.microsoft.com/default.aspx?scid=kb;en-us;326009#0"/>
        /// <seealso href="http://support.microsoft.com/default.aspx?scid=kb;en-us;326009#0"/>
        /// </remarks>
        public static DataTable SelectJoinInto(this DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            DataTable dt = SourceTable.CreateJoinTable(FieldList);
            return dt.InsertJoinInto(SourceTable, FieldList, RowFilter, Sort);
        }

        #endregion Joins

        #region CrossTabs

        public static DataTable CrossTabQuery(this DataTable table, string RowHeaderFieldName, string ColumnHeaderFieldName, string ValueFieldName, string ColumnOrderFieldName, string RowOrderFieldName)//, int IntersectionCount, bool PureCrossTab)
        {
            if (ColumnOrderFieldName == null || ColumnOrderFieldName == "")
                ColumnOrderFieldName = ColumnHeaderFieldName;

            int RowFieldIndex = table.Columns.IndexOf(RowHeaderFieldName);
            int ColumnFieldIndex = table.Columns.IndexOf(ColumnHeaderFieldName);
            int ValueFieldIndex = table.Columns.IndexOf(ValueFieldName);

            // select all the distinct values for the ColumnHeader columns (e.g. SecIDs)
            DataTable distinctdatatable;

            if (ColumnHeaderFieldName == ColumnOrderFieldName)
            {
                distinctdatatable = table.SelectDistinct(ColumnHeaderFieldName, ColumnHeaderFieldName, ColumnOrderFieldName);
                distinctdatatable = distinctdatatable.SelectInto(ColumnHeaderFieldName, "", ColumnHeaderFieldName);
            }
            else
                distinctdatatable = table.SelectDistinct(ColumnHeaderFieldName, null, ColumnOrderFieldName);

            // create the table and add the date RowHeader column (e.g. date)
            DataTable crosstabtable = table.CreateTable(RowHeaderFieldName);

            // we will hold a hash table to aid quickly looking up the column numbers
            Hashtable hashtable = new Hashtable();
            string columnheader;
            int columnnumber;

            // add the ColumnHeader columns (e.g. SecIDs). The columns are ordered by he OrderFieldName id
            DataView dv = new DataView(distinctdatatable);
            dv.Sort = ColumnOrderFieldName + " ASC";
            foreach (DataRowView drv in dv)
            {
                columnheader = drv[ColumnHeaderFieldName].ToString();
                crosstabtable.Columns.Add(columnheader, table.Columns[ValueFieldIndex].DataType);
                columnnumber = crosstabtable.Columns.IndexOf(columnheader);
                crosstabtable.Columns[columnnumber].Caption = drv[ColumnOrderFieldName].ToString();
                hashtable.Add(columnheader, columnnumber);
            }

            // now we have built the schema for the pivot table  we navigate through
            // the source table to extract the data

            object LastValue = null;
            DataRow newrow = null;
            //			if (IntersectionCount==0)
            //				IntersectionCount = distinctdatatable.Rows.Count;

            dv = new DataView(table);
            dv.Sort = RowOrderFieldName;
            foreach (DataRowView drv in dv)
            {
                bool rowexists = ColumnEqual(LastValue, drv[RowFieldIndex]);
                if (LastValue == null		// first time through loop
                    || rowexists == false)	// there is already a good row
                {
                    // only add the row if it has enough non-null data in it!
                    if (newrow != null)
                        crosstabtable.Rows.Add(newrow);

                    // create a new row and add it to the table
                    newrow = crosstabtable.NewRow();
                    LastValue = drv[RowFieldIndex];

                    // set up the RowHeader column(e.g. the date)
                    newrow[0] = LastValue;
                }

                // get the columnheader id
                object column = drv[ColumnFieldIndex];	// the column name
                object destcol = hashtable[column.ToString()];			// the column index in the new DataRow
                newrow[(int)destcol] = drv[ValueFieldIndex];
            }

            // must make sure we add the last row!
            if (newrow != null)
                crosstabtable.Rows.Add(newrow);

            return crosstabtable;
        }

        public static DataTable Pivot(this DataTable table, string RowHeaderFieldName, string ColumnHeaderFieldName, string ValueFieldName, string RowOrderFieldName, string ColumnOrderFieldName)
        {
            if (ColumnOrderFieldName.IsEmpty())
                ColumnOrderFieldName = ColumnHeaderFieldName;

            if (RowOrderFieldName.IsEmpty())
                RowOrderFieldName = RowHeaderFieldName;

            List<string> rowHeaderFieldsList = RowHeaderFieldName.Split(',').Select(s => s.Trim()).ToList();

            //int RowFieldIndex = table.Columns.IndexOf(RowHeaderFieldName);
            int ColumnFieldIndex = table.Columns.IndexOf(ColumnHeaderFieldName);
            int ValueFieldIndex = table.Columns.IndexOf(ValueFieldName);

            // select all the distinct values for the ColumnHeader columns (e.g. SecIDs)
            SortedList<object, string> listColumnNames = new SortedList<object, string>();
            SortedList<object, string> listRowNames = new SortedList<object, string>();

            Dictionary<string[], double> listValues = new Dictionary<string[], double>();

            foreach (DataRow dr in table.Rows)
            {
                string objColumnName = Convert.ToString(dr[ColumnFieldIndex]);
                object objColumnOrder = dr[ColumnOrderFieldName];
                if (listColumnNames.ContainsValue(objColumnName) == false)
                    listColumnNames.Add(objColumnOrder, objColumnName);

                foreach (string a_column in rowHeaderFieldsList)
                {
                    Convert.ToString(dr[a_column]);
                }
                string objRowName = Convert.ToString(dr[RowHeaderFieldName]);
                object objRowOrder = dr[RowOrderFieldName];
                if (listRowNames.ContainsValue(objRowName) == false)
                    listRowNames.Add(objRowOrder, objRowName);

                string[] valueKey = new string[] { objRowName, objColumnName };
                double value = Convert.ToDouble(dr[ValueFieldIndex]);
                if (listValues.ContainsKey(valueKey) == false)
                    listValues.Add(valueKey, value);
            }

            // create the table and add the date RowHeader column (e.g. date)

            #region Create the output table

            DataTable crosstabtable = table.CreateTable(RowHeaderFieldName);
            Type typeOfValue = table.Columns[ValueFieldIndex].DataType;
            for (int i = 0; i < listColumnNames.Count; i++)
            {
                string columnHeader = listColumnNames.Values[i];
                string columnCaption = Convert.ToString(listColumnNames.Keys[i]);
                crosstabtable.Columns.Add(columnHeader, typeOfValue);
                crosstabtable.Columns[columnHeader].Caption = columnCaption;
            }

            #endregion Create the output table

            for (int r = 0; r < listRowNames.Count; r++)
            {
                string rowValue = listRowNames.Values[r];
                DataRow newRow = crosstabtable.NewRow();
                newRow[RowHeaderFieldName] = rowValue;
                foreach (string columnName in listColumnNames.Values)
                {
                    double value;
                    if (listValues.TryGetValue(new string[] { rowValue, columnName }, out value))
                        newRow[columnName] = value;
                }
                crosstabtable.Rows.Add(newRow);
            }

            // now we have built the schema for the pivot table  we navigate through
            // the source table to extract the data

            return crosstabtable;
        }

        #endregion CrossTabs

        public static T[] ColumnToArray<T>(this DataTable table, string columnName, string filter)
        {
            DataRow[] dr = table.Select(filter);
            T[] r = new T[dr.Length];
            int iCol = table.Columns.IndexOf(columnName);

            //Exception<System.Data.DataException>.AssertFalse("Column not known", iCol < 0);

            for (int i = 0; i < dr.Length; i++)
            {
                r.SetValue(dr[i][iCol], i);
            }
            return r;
        }

        public static bool IsEmpty(this DataTable table)
        {
            return table.Rows.Count == 0;
        }

        public static DataRow[] SelectFormat(this DataTable table, string format, params object[] args)
        {
            return table.Select(string.Format(format, args));
        }

        #region Column Operations

        //public static DataTable AddColumns(this DataTable table, string FieldList)
        //{
        //    DataTable dt = table;
        //    DataColumn dc;
        //    string[] Fields = FieldList.Split(',');
        //    string[] FieldsParts;
        //    string Expression;
        //    foreach (string Field in Fields)
        //    {
        //        FieldsParts = Field.Trim().Split(" ".ToCharArray(), 3); // allow for spaces in the expression

        //        // add fieldname and datatype
        //        if (FieldsParts.Length == 2)
        //        {
        //            dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
        //            dc.AllowDBNull = true;
        //            continue;
        //        }
        //        // add fieldname, datatype, and expression
        //        Expression = FieldsParts[2].Trim();
        //        if (Expression.ToUpper() == "REQUIRED")
        //        {
        //            dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
        //            dc.AllowDBNull = false;
        //        }
        //        else
        //        {
        //            dc = dt.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true), Expression);
        //        }
        //    }
        //    return table;
        //}
        public static void DropColumns(this DataTable table, string szFieldsString)
        {
            string[] szFields = szFieldsString.Split(',');
            foreach (string colName in szFields)
            {
                if (table.Columns.IndexOf(colName.Trim()) > -1)
                    table.Columns.Remove(colName.Trim());
            }
        }

        #endregion Column Operations

        #region Row Operations

        public static DataTable DeleteRows(this DataTable table, string rowFilter)
        {
            DataRow[] drs = table.Select(rowFilter);

            for (int r = 0; r < drs.Length; r++)
            {
                drs[r].Delete();
            }
            table.AcceptChanges();
            return table;
        }

        public static DataRow InsertRowList(this DataTable table, IList values)
        {
            DataRow dr = table.NewRow();
            for (int i = 0; i < values.Count; i++)
            {
                dr[i] = values[i];
            }
            table.Rows.Add(dr);
            return dr;
        }

        public static DataRow InsertRowArray(this DataTable table, object value, Array values)
        {
            DataRow dr = table.NewRow();
            dr[0] = value;
            for (int i = 0; i < values.Length; i++)
            {
                dr[i + 1] = values.GetValue(i);
            }
            table.Rows.Add(dr);
            return dr;
        }

        //[Obsolete("Moved to Episteme.SystemExtensions.Data", false)]
        //public static DataRow InsertRow(this DataTable table, params object[] values)
        //{
        //    return table.Rows.Add(values);
        //}
        public static DataRow AddRow(this DataTable table)
        {
            var row = table.NewRow();
            table.Rows.Add(row);
            return row;
        }

        #endregion Row Operations

        public static Array ToArrayWithHeadings(this DataTable table)
        {
            int height = table.Rows.Count;
            int width = table.Columns.Count;

            Array array = Array.CreateInstance(typeof(object), height + 1, width);

            for (int j = 0; j < width; j++)
            {
                array.SetValue(table.Columns[j].ColumnName, 0, j);
                for (int i = 0; i < height; i++)
                {
                    array.SetValue(table.Rows[i][j], i + 1, j);
                }
            }

            return array;
        }

        public static Array ToTypedArray<T>(this DataTable table)
        {
            int height = table.Rows.Count;
            int width = table.Columns.Count;

            Array array = Array.CreateInstance(typeof(T), height, width);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    array.SetValue(table.Rows[i][j], i, j);
                }
            }

            return array;
        }

        public static Array ToTypedArray<T>(this DataTable table, int column)
        {
            int height = table.Rows.Count;
            Array array = Array.CreateInstance(typeof(T), height);

            for (int i = 0; i < height; i++)
            {
                array.SetValue(table.Rows[i][column], i);
            }

            return array;
        }

        public static Array ToArray(this DataTable table)
        {
            int height = table.Rows.Count;
            int width = table.Columns.Count;

            Array array = Array.CreateInstance(typeof(object), height, width);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    array.SetValue(table.Rows[i][j], i, j);
                }
            }
            return array;
        }

        public static Array ToArray(this DataTable table, int column)
        {
            int height = table.Rows.Count;
            Type elementType = table.Columns[column].DataType;

            Array array = Array.CreateInstance(elementType, height);

            for (int i = 0; i < height; i++)
            {
                array.SetValue(table.Rows[i][column], i);
            }

            return array;
        }

        public static bool ContainsColumnAllOf(this DataTable table, string Fields)
        {
            int iFieldCount = 0;
            string[] fields = Fields.Split(',');
            foreach (string field in fields)
            {
                if (table.ContainsColumn(field.Trim()))
                    iFieldCount++;
            }
            return (iFieldCount == fields.Length);
        }

        public static string ContainsColumnOneOf(this DataTable table, string Fields)
        {
            string[] fields = Fields.Split(',');
            foreach (string field in fields)
            {
                if (table.ContainsColumn(field.Trim()))
                    return field.Trim();
            }
            return "";
        }

        public static bool ContainsColumn(this DataTable table, string columnName)
        {
            Type dataType;
            return table.ContainsColumn(columnName, out dataType);
        }

        public static bool ContainsColumn(this DataTable table, string columnName, out Type dataType)
        {
            int iCol = table.Columns.IndexOf(columnName);
            if (iCol < 0)
            {
                dataType = null;
                return false;
            }

            dataType = table.Columns[iCol].DataType;
            return true;
        }

        public static Dictionary<DataRow, DataTable> GetType2TypeMemberDictionary(this DataTable table, string TypeFields, string TypeMemberFields)
        {
            Dictionary<DataRow, DataTable> retVal = new Dictionary<DataRow, DataTable>();

            DataView dvThis = new DataView(table);

            DataTable dtDistinctTypes = table.SelectGroupByFrom(TypeFields, "", TypeFields);
            foreach (DataRow drType in dtDistinctTypes.Rows)
            {
                dvThis.RowFilter = DataTableExtensions.RowToFilter(drType);

                DataTable dtMemberFields = table.SelectInto(TypeMemberFields, DataTableExtensions.RowToFilter(drType), "");

                retVal.Add(drType, dtMemberFields);
            }

            return retVal;
        }

        /// <summary>
        /// Returns the first integer column as an integer list
        /// (Currently only examines the first two columns)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<int> ToList(this DataTable table)
        {
            List<int> list = new List<int>();

            if (table.Columns.Count == 1 && table.Columns[0].DataType == typeof(int))
            {
                foreach (DataRow dr in table.Rows)
                    list.Add((int)dr[0]);
            }
            else if (table.Columns.Count == 2 && table.Columns[1].DataType == typeof(int))
            {
                foreach (DataRow dr in table.Rows)
                    list.Add((int)dr[1]);
            }
            return list;
        }

        public static List<double> GetSeries(this DataTable table, int columnNumber)
        {
            List<double> list = new List<double>();
            if (table.Columns[columnNumber].DataType == typeof(int))
            {
                foreach (DataRow dr in table.Rows)
                    list.Add((double)dr[columnNumber]);
            }

            return list;
        }

        public static List<DateTime> ToDateTimeList(this DataTable table)
        {
            List<DateTime> list = new List<DateTime>();
            foreach (DataColumn column in table.Columns)
            {
                if (column.DataType == typeof(DateTime))
                {
                    foreach (DataRow dr in table.Rows)
                        list.Add((DateTime)dr[column]);
                }
            }
            return list;
        }

        public static List<DatedValue> GetDatedValues(this DataTable table, string column)
        {
            return table.GetDatedValues(table.Columns[column].Ordinal);
        }

        public static List<DatedValue> GetDatedValues(this DataTable table, int column)
        {
            // find the column with dates
            int iDates = -1;
            for (int c = 0; c < table.Columns.Count; c++)
            {
                if (table.Columns[c].DataType == typeof(DateTime))
                {
                    iDates = c;
                    break;
                }
            }

            List<DatedValue> lDatedValues = new List<DatedValue>();
            double value;
            DateTime date;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][column] != DBNull.Value)
                {
                    value = (double)table.Rows[i][column];
                    date = (DateTime)table.Rows[i][iDates];
                    lDatedValues.Add(new DatedValue(date, value));
                }
            }

            return lDatedValues;
        }

        public static void Cumulative(this DataTable me, string TimeField, string AggregateFields, string ValueField, string CumulativeValueField)
        {
            /*Check that the column has been defined
             e.g. me.Columns.Add(OutputField, typeof(double));
             */
            if (me.ContainsColumn(CumulativeValueField) == false)
                throw new NotSupportedException("Table does not contain column {1}".FormatString(CumulativeValueField));

            /*select the distinct aggregate values and calulate the cumulative sum of the value field over the time field*/
            foreach (DataRow row in me.SelectDistinct(AggregateFields).Rows)
            {
                string filter = row.RowToFilter();
                DataView dv = new DataView(me);
                dv.RowFilter = filter;
                dv.Sort = TimeField + " ASC";
                double sum = 0;
                foreach (DataRowView drv in dv)
                {
                    sum += drv.TryGetDouble(ValueField, 0);
                    drv[CumulativeValueField] = sum;
                }
            }
        }

        public static string ColumnDefinitionString(this DataTable table)
        {
            string sz = "";
            for (int c = 0; c < table.Columns.Count; c++)
            {
                sz = string.Format("{0} {1} {2},", sz, table.Columns[c].ColumnName, table.Columns[c].DataType.Name);
            }
            return sz.Trim(',');
        }

        public static string RowToFilter(this DataTable table, int i)
        {
            //return DataTableExtensions.RowToFilter(table.Rows[i]);
            return table.Rows[i].RowToFilter();
        }

        public static double TryGetDouble(this DataRowView me, string field, double alternative_value)
        {
            var v = me[field];
            if (v == DBNull.Value)
                return alternative_value;
            else
                return Convert.ToDouble(v);
        }

        public static string RowToFilter(this DataRow dr)
        {
            // returns the row as a ready made string to use as a filter
            string temp = "TRUE ";
            object val;
            foreach (DataColumn col in dr.Table.Columns)
            {
                val = dr[col];

                if (val == DBNull.Value)
                    continue;

                if (val.GetType() == typeof(string))
                    temp = temp + string.Format("AND {0} = '{1}' ", col.ColumnName, dr[col]);
                else if (val.GetType() == typeof(DateTime))
                    temp = temp + string.Format("AND {0} = #{1}# ", col.ColumnName, (DateTime)dr[col]);
                else
                    temp = temp + string.Format("AND {0} = {1} ", col.ColumnName, dr[col]);
            }
            return temp.Replace("TRUE AND", "");
        }

        public static string RowToFilter(this DataRow dr, params string[] columns)
        {
            // returns the row as a ready made string to use as a filter
            string temp = "TRUE ";
            object val;
            //var columns = column_names.Split(',').AsEnumerable().Select(c=> c.Trim());

            foreach (var col in columns)
            {
                val = dr[col];

                if (val == DBNull.Value)
                    continue;

                if (val.GetType() == typeof(string))
                    temp = temp + string.Format("AND {0} = '{1}' ", col, dr[col]);
                else if (val.GetType() == typeof(DateTime))
                    temp = temp + string.Format("AND {0} = #{1}# ", col, (DateTime)dr[col]);
                else
                    temp = temp + string.Format("AND {0} = {1} ", col, dr[col]);
            }
            return temp.Replace("TRUE AND", "");
        }

        public static bool IsEmptyString(string fields)
        {
            if (fields == "" || fields == null || fields == default(string))
                return true;
            return false;
        }

        //public static string RowToFilter(DataRow dr, string fields)
        //{
        //    if (IsEmptyString(fields))
        //        return "true";
        //    // returns the row as a ready made string to use as a filter
        //    string temp = "";
        //    object val;
        //    string[] columns = fields.Split(',');
        //    foreach (string column in columns)
        //    {
        //        string col = column.Trim();
        //        val = dr[col];

        //        if (val.GetType() == typeof(string))
        //            temp = temp + string.Format("AND {0} = '{1}' ", col, dr[col]);
        //        else if (val.GetType() == typeof(DateTime))
        //            temp = temp + string.Format("AND {0} = #{1}# ", col, (DateTime)dr[col]);
        //        else
        //            temp = temp + string.Format("AND {0} = {1} ", col, dr[col]);
        //    }
        //    temp = temp.TrimStart('A', 'N', 'D');
        //    return temp;
        //}
        public static string RowToValueString(DataRow dr, string fields)
        {
            // returns the row as a ready made string to use as a filter
            string temp = "";
            object val;
            string[] columns = fields.Split(',');
            foreach (string column in columns)
            {
                string col = column.Trim();
                val = dr[col];

                if (val.GetType() == typeof(string))
                    temp = temp + string.Format(",{0}", dr[col]);
                else if (val.GetType() == typeof(DateTime))
                    temp = temp + string.Format(",#{0}#", (DateTime)dr[col]);
                else
                    temp = temp + string.Format(",{0}", dr[col]);
            }
            temp = temp.TrimStart(',');
            return temp;
        }

        public static object[] RowToValueArray(DataRow dr, string fields)
        {
            string[] columns = fields.Split(',');
            Object[] retVal = new Object[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                retVal[i] = dr[columns[i].Trim()];
            }
            return retVal;
        }

        public static DataTable UnRelabelColumnNames(this DataTable table)
        {
            DataColumn dc;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                dc = table.Columns[i];
                dc.ColumnName = (string)dc.ExtendedProperties["ColumnName"];
            }
            return table;
        }

        public static DataTable RelabelColumnNames(this DataTable table)
        {
            DataColumn dc;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                dc = table.Columns[i];

                // swap caption and column name
                dc.ExtendedProperties["ColumnName"] = dc.ColumnName;

                dc.ColumnName = string.Format("Series{0}", i);
            }
            return table;
        }

        public static DataTable SwapColumnNamesWithCaption(this DataTable table)
        {
            DataColumn dc;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                dc = table.Columns[i];

                string szColumnName = dc.ColumnName;
                dc.ColumnName = dc.Caption;
                dc.Caption = szColumnName;
            }
            return table;
        }

        public static DataTable NewDataTable(DataView underlyingView)
        {
            DataTable table = new DataTable();
            foreach (DataColumn srcCol in underlyingView.Table.Columns)
                table.Columns.Add(srcCol.ColumnName, srcCol.DataType);

            table.InsertInto(underlyingView.Table, "", underlyingView.RowFilter, underlyingView.Sort);
            return table;
        }

        public static void SetTag(this DataTable table, object tag)
        {
            table.ExtendedProperties.Add("DATATABLE_EXTENSION_TAG", tag);
        }

        public static object GetTag(this DataTable table)
        {
            if (table.ExtendedProperties.ContainsKey("DATATABLE_EXTENSION_TAG"))
                return table.ExtendedProperties["DATATABLE_EXTENSION_TAG"];
            else
                return null;
        }
    }

    public struct DatedValue
    {
        public DateTime Date;
        public double Value;

        public DatedValue(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
    }
}