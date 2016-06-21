using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic;

namespace Ultility
{
    public class SqlProvider
    {
        #region Variable

        private string dataSource;
        private string databaseName;
        private string username;
        private string password;
        private SqlConnection connection;

        #endregion

        #region Properties

        public string DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string ConnectionString
        {
            get
            {
                if (username == "")
                {
                    return string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", DataSource, DatabaseName);
                }
                else
                {
                    return string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};Integrated Security=False", DataSource, DatabaseName, Username, Password);
                }
                
            }
        }

        #endregion

        #region Constructor

        public SqlProvider(string dataSource, string dbName = "master", string uid = "", string pwd = "")
        {
            this.dataSource = dataSource;
            this.databaseName = dbName;
            this.username = uid;
            this.password = pwd;
        }

        #endregion

        #region Method

        public async Task<SqlDataReader> ExcuteQuery(string tsql)
        {
            connection = new SqlConnection(ConnectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(tsql, connection);
            cmd.CommandType = CommandType.Text;
            return await cmd.ExecuteReaderAsync();
        }

        /// <summary>
        /// Get all Databases Information as Name, Id, Create Time
        /// </summary>
        /// <returns>List of Databases</returns>
        public List<SQLDatabase> GetDatabases()
        {
            List<SQLDatabase> lstTables = new List<SQLDatabase>();

            connection = new SqlConnection(ConnectionString);
            connection.Open();

            DataTable databaseTable = connection.GetSchema("Databases");
            foreach (DataRow row in databaseTable.Rows)
            {
                lstTables.Add(new SQLDatabase(row.ItemArray[0].ToString(), Convert.ToInt32(row.ItemArray[1]), (DateTime)row.ItemArray[2]));
            }

            connection.Close();

            return lstTables;
        }

        /// <summary>
        /// Get all tables detail: TABLE_NAME, COLUMN_NAME and DATA_TYPE
        /// </summary>
        /// <returns>List of TABLE_NAME, COLUMN_NAME and DATA_TYPE</returns>
        public async Task<Dictionary<string, List<Columns>>> GetTables()
        {
            Dictionary<string, List<Columns>> lstTable = new Dictionary<string, List<Columns>>();
            List<Columns> lstColumn = new List<Columns>();
            List<string> lstPK = new List<string>();
            string tblName = "", tmpName = "NULL";

            var tableDetail = await ExcuteQuery(MVPPowerToolv2.Properties.Resources.GetTablesDetailScript);

            while (await tableDetail.ReadAsync())
            {
                tblName = tableDetail["TABLE_NAME"].ToString();

                if (tblName != tmpName)
                {
                    lstColumn = new List<Columns>();
                    lstPK = new List<string>();
                    tmpName = tblName;
                    var tablePrimaryKey = await ExcuteQuery(MVPPowerToolv2.Properties.Resources.GetTablesPrimaryKeyScript.Replace("---TABLE_NAME---",tblName));
                    while (await tablePrimaryKey.ReadAsync())
                    {
                        lstPK.Add(tablePrimaryKey["COLUMN_NAME"].ToString());
                    }
                }

                lstColumn.Add(new Columns(tableDetail["COLUMN_NAME"].ToString(), tableDetail["DATA_TYPE"].ToString(), lstPK.Exists(e=>e.Equals(tableDetail["COLUMN_NAME"].ToString()))));
                lstTable[tblName] = lstColumn;
            }

            return lstTable;
        }

        #endregion
    }
}
