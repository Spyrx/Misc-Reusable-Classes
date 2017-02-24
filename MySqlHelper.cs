using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows;

namespace ENTERNAMESPACE
{
    class MySqlHelper
    {
        public void connectToSqlServer(string conString)
        {

            SqlConnection connection;

            connection = new SqlConnection(conString);
            try
            {
                connection.Open();
		//Do something other that show a message box here
                MessageBox.Show("Connection Successful!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot Open Connection to Server!");
                MessageBox.Show(ex.ToString());
            }

        }
        public DataTable getSQLServers()
        {
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            DataTable sqlServers = instance.GetDataSources();
            return sqlServers;
        }

        public DataTable getDatabases(String sqlInstance)
        {
            string connectionString = string.Format("Data Source=" + sqlInstance + ";Integrated Security=SSPI;");

            DataTable databases = null;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                databases = sqlConnection.GetSchema("Databases");
                sqlConnection.Close();
            }
            return databases;
        }
    }
}
