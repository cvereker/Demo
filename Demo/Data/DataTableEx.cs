using SystemExtensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SystemExtensions.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataTablePropMappingAttribute : Attribute
    {
        public string ColumnName { get; set; }
    }

    public class StoredProcParameter : Tuple<string, object>
    {
        public StoredProcParameter(string ParameterName, object ParameterValue)
            : base(ParameterName, ParameterValue)
        {
        }

        public static StoredProcParameter New(string ParameterName, object ParameterValue)
        {
            return new StoredProcParameter(ParameterName, ParameterValue);
        }
    }

    /// <summary>
    /// Extensions to System.Data.DataTable
    /// </summary>
    public static class EpistemeDataTableExtension
    {
        /// <summary>
        /// Extension method for filling  table from a SQL Server database
        /// </summary>
        /// <param name="table">The table to be filled</param>
        /// <param name="ConnectionString">Properly formed SQL Connection String e.g.:<code>"Server=[SERVER];Database=[DATABASE];trusted_connection=[TRUE/FALSE];")</code></param>
        /// <param name="CommandText">Stored procedure or SQL Text</param>
        /// <param name="CommandType">Type of the command</param>
        /// <param name="parameters">name,value pairs of parameters e.g. <code>new Tuple<![CDATA["<String,Object>"]]>("TimeSeriesID", 143)</code></param>
        /// <returns></returns>
        public static DataTable FillTable(this DataTable table,
        string ConnectionString,
        string CommandText,
        CommandType CommandType = CommandType.Text,
        params Tuple<string, object>[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = CommandText,
                CommandType = CommandType
            })
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                foreach (var param in parameters)
                    cmd.Parameters.AddWithValue(param.Item1, param.Item2);
                conn.Open();

                try
                {
                    da.Fill(table);
                }
                catch (EntryPointNotFoundException e)
                {
                    // ignore
                }
            }
            return table;
        }

        /// <summary>
        /// Casts datatable to an object, matching column names to the object public properties
        /// </summary>
        /// <typeparam name="T">Object (class or struct) that has a public default constructor</typeparam>
        /// <param name="table">Table of data to be used as the data source</param>
        /// <param name="EnforceTypeEquivalence">If true then the types of the DataTable columns and the class columns must match exactly. Otherwise the code will cast (including downcasting).</param>
        /// <returns>A list of newly instantiated <typeparamref name="T"/> objects with public properties initialised with values from the DataTable</returns>
        public static List<T> Cast<T>(this DataTable table, bool EnforceTypeEquivalence = false) where T : class, new()
        {
            // build a map of property to datatable column
            Type type = typeof(T);
            var mapper = new Dictionary<System.Reflection.PropertyInfo, DataColumn>();
            foreach (var property in type.GetProperties().Where(p => p.PropertyType.IsValueType
                || p.PropertyType.FullName == "System.String"
                || p.PropertyType.FullName == "System.DateTime"))
            {
                var attr = property.GetCustomAttributes(typeof(DataTablePropMappingAttribute), true).FirstOrDefault() as DataTablePropMappingAttribute;
                if (attr != null && table.Columns.Contains(attr.ColumnName))
                    mapper.Add(property, table.Columns[attr.ColumnName]);
                else if (table.Columns.Contains(property.Name))
                    mapper.Add(property, table.Columns[property.Name]);
            }

            //use the mapper to set the properties on each object and add to the return list
            List<T> converted_rows = new List<T>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                T value = new T();
                foreach (var prop in mapper)
                    if (row[prop.Value] != DBNull.Value)
                    {
                        object propValue = row[prop.Value];
                        // do any possible casts (e.g. double to int) without warning)

                        // can't handle enums yet
                        if (prop.Key.PropertyType.IsEnum)
                            continue;

                        if (!EnforceTypeEquivalence)
                            propValue = Convert.ChangeType(propValue, prop.Key.PropertyType);
                        prop.Key.SetValue(value, propValue, null);
                    }

                converted_rows.Add(value);
            }
            return converted_rows;
        }

        public static DataTable CastToDataTable<T>(this IEnumerable<T> list, bool EnforceTypeEquivalence = false) where T : class, new()
        {
            DataTable table = new DataTable();

            // build a map of property to datatable column
            Type type = typeof(T);
            var mapper = new Dictionary<System.Reflection.PropertyInfo, DataColumn>();
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsGenericType || property.PropertyType.IsClass)
                    continue;

                var attr = property.GetCustomAttributes(typeof(DataTablePropMappingAttribute), true).FirstOrDefault() as DataTablePropMappingAttribute;
                if (attr != null)
                {
                    table.Columns.Add(attr.ColumnName, property.PropertyType);
                    mapper.Add(property, table.Columns[attr.ColumnName]);
                }
                else
                {
                    table.Columns.Add(property.Name, property.PropertyType);
                    mapper.Add(property, table.Columns[property.Name]);
                }
            }

            //use the mapper to set the properties on each object and add to the return list

            foreach (var item in list)
            {
                var row = table.NewRow();
                foreach (var prop in mapper)
                {
                    var value = prop.Key.GetValue(item, null);
                    if (value is double)
                        value = ((double)value).ToDBFriendlyDouble();
                    row[prop.Value] = value;
                }

                table.Rows.Add(row);
            }
            return table;
        }

        public static string ScriptTableIfNotExists(this DataTable table, string schema, string tableName)
        {
            var simpleCreateTable = table.ScriptTable(schema + "." + tableName, false);
            return @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND  TABLE_NAME = '{1}') {2}".FormatString(schema, tableName, simpleCreateTable);
        }

        public static Int32 ExecuteCreateTableIfNotExists(this DataTable table, String connectionString, string schema, string tableName)
        {
            Int32 iRet = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = table.ScriptTableIfNotExists(schema, tableName);
                    iRet = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return iRet;
        }

        //public static string ScriptDelAppend(this DataTable table, string sourceSchema, string destinationSchema)
        //{
        //    StringBuilder sb = new StringBuilder();
        //}
        public static string ScriptTable(this DataTable table, string tableName, Boolean enforceTempTable = true)
        {
            if (enforceTempTable && !tableName.StartsWith("tmp.") && !tableName.StartsWith("#"))
                throw new NotSupportedException("The table name MUST be on a schema called tmp or #, and the table name must be passed in with this schema (e.g. tmp.Values)");

            StringBuilder sb = new StringBuilder();

            // only drop temp tables
            if (enforceTempTable)
            {
                sb.AppendLine("IF  EXISTS (	SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'" + tableName + "') AND type in (N'U')) DROP TABLE " + tableName);
                sb.AppendLine("");
            }

            sb.AppendFormat("create table {0}{1}", tableName, Environment.NewLine);
            sb.AppendLine("(");
            foreach (DataColumn col in table.Columns)
            {
                var name = col.ColumnName;
                var type = col.DataType.Name;
                string sqlType;
                switch (type)
                {
                    case "DateTime":
                        sqlType = "DateTime";
                        break;

                    case "TimeSpan":
                        sqlType = "time(7)";
                        break;

                    case "Int32":
                        sqlType = "Int";
                        break;

                    case "Int64":
                        sqlType = "bigint";
                        break;

                    case "Int16":
                        sqlType = "smallint";
                        break;

                    case "Double":
                        sqlType = "float";
                        break;

                    case "Single":
                        sqlType = "float(53)";
                        break;

                    case "String":

                        if (col.MaxLength > 0)
                            sqlType = "varchar({0})".FormatString(col.MaxLength);
                        else if (table.PrimaryKey.Contains(col))
                            sqlType = "varchar(255)";
                        else
                            sqlType = "varchar(max)";
                        break;

                    case "SqlGeography":
                        //Desktop applications
                        //For desktop applications, add the following line of code to run before any spatial operations are performed:
                        //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

                        sqlType = "geography";

                        break;

                    default:
                        throw new NotImplementedException("Type casting not yet implemented");
                }

                sb.AppendFormat("   {0} {1},{2}", name, sqlType, Environment.NewLine);
            }
            sb.Length--; // remove last comma

            if (table.PrimaryKey != null && table.PrimaryKey.Count() > 0)
            {
                var pk = table.PrimaryKey.Select(p => p.ColumnName).Aggregate((p1, p2) => p1 + "," + p2);
                sb.AppendFormat("primary key ({0})", pk);
            }

            sb.AppendLine(")");

            return sb.ToString();
        }

        /// <summary>
        /// Casts DataTable to object T (which can be an anonymous type)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<T> Cast<T>(this DataTable table, Func<DataRow, T> func)
        {
            List<T> converted_rows = new List<T>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                T value = func(row);
                converted_rows.Add(value);
            }
            return converted_rows;
        }

        public static void Delete(this DataTable table, string filter)
        {
            IEnumerable<DataRow> rowsToDelete = table.Select(filter);
            rowsToDelete.Delete();
        }

        public static void Delete(this IEnumerable<DataRow> rowsToDelete)
        {
            foreach (var row in rowsToDelete)
                row.Delete();
        }

        public static DataTable GetEmptyTable(this DataTable table, string connectionString, string tableName)
        {
            return table.FillTable(connectionString, "select top 0 * from {0}".FormatString(tableName));
        }

        public static void Upsert(this DataTable table, string connectionString, string destination)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // open connection and keep open until the end (to preserve the temp table)
                connection.Open();

                // create a temp table with the correct columns
                SqlCommand cmdCreateTempTable = new SqlCommand("select top 0 * into #temp from " + destination, connection);
                cmdCreateTempTable.CommandType = CommandType.Text;
                cmdCreateTempTable.ExecuteNonQuery();

                // bulk copy to the temp table
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#temp";

                    // ensure that the order of DataColumns does not matter
                    foreach (DataColumn column in table.Columns)
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    bulkCopy.WriteToServer(table);
                }

                // get the schema infor for the target table (including primary key, etc)
                SqlCommand cmdSchema = new SqlCommand("select top 0 * from {0}".FormatString(destination), connection);
                cmdSchema.CommandType = CommandType.Text;

                // extract the information that we need and then close it - the SqlReader hogs the connection and will otherwise block
                // any further commands on this connection
                var schemaReader = cmdSchema.ExecuteReader(CommandBehavior.KeyInfo);
                var schemaTable = schemaReader.GetSchemaTable().AsEnumerable().Select(r => new
                {
                    ColName = (String)r["ColumnName"],
                    IsKey = (bool)r["IsKey"],
                    IsExpression = (bool)r["IsExpression"],
                    IsAutoIncrement = (bool)r["IsAutoIncrement"],
                    IsReadOnly = (bool)r["IsReadOnly"]
                }).ToList();
                schemaReader.Close();

                String target = destination;
                String source = "#temp";

                // build the merge statement
                StringBuilder sbPK = new StringBuilder();
                foreach (var pk_column in schemaTable.Where(c => c.IsKey).Select(r => r.ColName))
                    sbPK.AppendFormat(" and target.{0} = source.{0}", pk_column);

                // remove the initial comma
                if (sbPK.Length == 0)
                    throw new NotSupportedException("Destination table does not have a primary key");
                sbPK.Remove(0, 4);
                String joinSQL = sbPK.ToString();

                StringBuilder sbUpdate = new StringBuilder();
                StringBuilder sbInsert = new StringBuilder();
                StringBuilder sbValues = new StringBuilder();
                var updateableColumns = schemaTable.Where(c => !c.IsKey && !c.IsExpression && !c.IsAutoIncrement && !c.IsReadOnly).Select(r => r.ColName);
                var insertableColumns = schemaTable.Where(c => !c.IsAutoIncrement && !c.IsExpression && !c.IsReadOnly).Select(r => r.ColName);

                foreach (var updt_column in insertableColumns)
                {
                    sbInsert.AppendFormat(",{0}", updt_column);
                    sbValues.AppendFormat(",source.{0}", updt_column);
                }
                foreach (var updt_column in updateableColumns)
                    sbUpdate.AppendFormat(",target.{0} = source.{0}", updt_column);

                // remove the initial comma
                if (updateableColumns.Count() > 0) { sbInsert.Remove(0, 1); sbUpdate.Remove(0, 1); sbValues.Remove(0, 1); }
                String updateSQL = sbUpdate.ToString();
                String insertSQL = sbInsert.ToString();
                String valuesSQL = sbValues.ToString();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("merge into {0} as target using {1} as source on {2} ", target, source, joinSQL);
                sb.AppendFormat("when matched then update set {0} ", updateSQL);
                sb.AppendFormat("when not matched by target then insert ({0}) values({1});", insertSQL, valuesSQL);

                // execute the merge statement
                SqlCommand cmd = new SqlCommand(sb.ToString(), connection);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                // ... and we're done. The temp table will be droped as soon as we close the connection
            }
        }

        public static void BulkCopy(this DataTable table, string connectionString, string destination, string preImportQueryText = null, string postImportQueryText = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (!string.IsNullOrEmpty(preImportQueryText))
                {
                    SqlCommand cmdPre = new SqlCommand(preImportQueryText, connection);
                    cmdPre.CommandType = CommandType.Text;
                    cmdPre.ExecuteNonQuery();
                }

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = destination;

                    // ensure that the order of DataColumns does not matter
                    foreach (DataColumn column in table.Columns)
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    bulkCopy.WriteToServer(table);
                }

                if (!string.IsNullOrEmpty(postImportQueryText))
                {
                    SqlCommand cmdPost = new SqlCommand(postImportQueryText, connection);
                    cmdPost.CommandType = CommandType.Text;
                    cmdPost.ExecuteNonQuery();
                }
            }
        }

        public static DataTable AddColumns(this DataTable table, string FieldList)
        {
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
                    dc = table.Columns.Add(FieldsParts[0].Trim(), typeof(Int32));
                    dc.AllowDBNull = true;
                }
                // add fieldname and datatype
                else if (FieldsParts.Length == 2)
                {
                    dc = table.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
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
                            dc = table.Columns.Add(FieldsParts[0].Trim(), typeof(DateTime));
                            string def = FieldsParts[2];
                            def = def.Replace('#', ' ');
                            def = def.Trim();
                            dc.DefaultValue = DateTime.Parse(def);
                        }
                        else
                        {
                            // default fields are integer with a default value
                            dc = table.Columns.Add(FieldsParts[0].Trim(), typeof(Int32));
                            dc.DefaultValue = FieldsParts[2];
                        }
                        dc.AllowDBNull = true;
                    }
                    else if (Expression.ToUpper() == "REQUIRED")
                    {
                        dc = table.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
                        dc.AllowDBNull = false;
                    }
                    else
                    {
                        dc = table.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true), Expression);
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid field definition: '" + Field + "'.");
                }
            }
            return table;
        }

        public static void SetPrimaryKey(this DataTable table, string FieldList)
        {
            var columns = FieldList.Split(',').Select(c => c.Trim());
            if (columns.All(c => table.Columns.Contains(c)))
            {
                var pk = columns.Select(c => table.Columns[c]).ToArray();
                table.PrimaryKey = pk;
            }
            else
            {
                throw new IndexOutOfRangeException("unmatched columns specified for primary key");
            }
        }

        public static DataRow InsertRow(this DataTable table, params object[] values)
        {
            return table.Rows.Add(values);
        }

        public static object DBDate(this DateTime value)
        {
            return value == default(DateTime) ? System.DBNull.Value : (object)value;
        }

        public static void SaveCSV(this DataTable table, string filepath, string sep = ",")
        {
            var headers = table.ColumnHeaders(sep);

            var rows = table.AsEnumerable().Select(r => r.ItemArray.Aggregate((rowValues, next) => rowValues + sep + next)).ToList();

            var fInfo = new System.IO.FileInfo(filepath);
            if (!fInfo.Directory.Exists)
                fInfo.Directory.Create();

            using (var writer = new System.IO.StreamWriter(filepath, false))
            {
                writer.WriteLine(headers);
                foreach (var row in rows)
                    writer.WriteLine(row);
                writer.Close();
            }
        }
    }
}