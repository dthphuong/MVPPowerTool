using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic;

namespace Ultility
{
    class SqlScript
    {
        public static string ConvertObject(object obj, string objType)
        {
            string result = "";

            switch (objType)
            {
                case "char":
                case "nchar":
                case "nvarchar":
                    result = "N'" + obj.ToString() + "'";
                    break;
                case "bit":
                case "int":
                case "float":
                    result = obj.ToString();
                    break;
                case "datetime":
                    result = "'" + obj.ToString() + "'";
                    break;
                case "image":
                    
                    break;
            }

            return result;
        }




    }
}
