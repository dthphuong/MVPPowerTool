using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    public class SQLDatabase
    {
        #region Variable

        private string name;
        private int index;
        private DateTime createTime;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        #endregion

        #region Constructor

        public SQLDatabase(string name, int index, DateTime createTime)
        {
            this.name = name;
            this.index = index;
            this.createTime = createTime;
        }

        #endregion
    }
}
