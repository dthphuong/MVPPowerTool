using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Basic
{
    public class SQLTable
    {
        #region Variable

        private string tableName;
        private List<Columns> lstColumns;

        #endregion

        #region Properties

        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public List<Columns> LstColumns
        {
            get { return lstColumns; }
            set { lstColumns = value; }
        }

        #endregion

        #region Constructor

        public SQLTable(string name, List<Columns> lstColumns)
        {
            this.tableName = name;
            this.lstColumns = lstColumns;
        }

        #endregion
    }
}
