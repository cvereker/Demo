using System;
using System.Data;
using System.Data.SqlClient;

namespace SystemExtensions.Data
{
    public static class EpistemeDataSetExtension
    {
        public static DataSet FillTable(this DataSet data_set,
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
                da.Fill(data_set);
            }
            return data_set;
        }
    }
}