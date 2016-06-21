using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    public class Columns
    {
        #region Variable

        private string name;
        private string dataType;
        private bool isPrimaryKey;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set {  isPrimaryKey = value; }
        }

        #endregion

        #region Constructor

        public Columns(string name, string dataType, bool isPk = false)
        {
            this.name = name;
            this.dataType = dataType;
            this.isPrimaryKey = isPk;
        }

        #endregion
    }
}
