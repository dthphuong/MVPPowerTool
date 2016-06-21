using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Basic;
using System.Globalization;

namespace Ultility
{
    public class MvpGenerateCore
    {
        #region Variable

        private string path;
        private string projectName;
        private string connectionString;
        private Dictionary<string, List<Columns>> tables;
        private string basicclassPath, modelPath, viewPath, presenterPath;

        #endregion

        #region Properties

        public string ProjectName
        {
            get { return projectName;  }
            set {  projectName = value;}
        }

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public Dictionary<string, List<Columns>> Tables
        {
            get { return tables;}
            set { tables = value;}
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        #endregion

        #region Constructor

        public MvpGenerateCore(string projName, string conString, Dictionary<string, List<Columns>> tbl, string path)
        {
            this.projectName = projName;
            this.connectionString = conString;
            this.tables = tbl;
            this.path = path;
            basicclassPath = "";
            modelPath = "";
            viewPath = "";
            presenterPath = "";
        }

        #endregion

        #region Method

        #region Convert Based method

        /// <summary>
        /// Convert SQL Data types to C# Data types
        /// </summary>
        /// <param name="sqlType">SQL Data type</param>
        /// <returns>C# Type</returns>
        public string ToCSharpConvertType(string sqlType)
        {
            string t = "";

            switch (sqlType.ToLower())
            {
                case "nvarchar":
                case "varchar":
                case "char":
                case "text":
                    t = "Convert.ToString(";
                    break;
                case "bit":
                    t = "Convert.ToBoolean(";
                    break;
                case "int":
                    t = "Convert.ToInt32(";
                    break;
                case "float":
                case "real":
                    t = "Convert.ToDouble(";
                    break;
                case "datetime":
                    t = "Convert.ToDateTime(";
                    break;
                case "image":
                    t = "ConvertTool.ToByte(";
                    break;

            }

            return t;
        }

        public string ToCSharpType(string sqlType)
        {
            string t = "";

            switch (sqlType.ToLower())
            {
                case "nvarchar":
                case "varchar":
                case "char":
                case "text":
                    t = "string";
                    break;
                case "bit":
                    t = "bool";
                    break;
                case "int":
                    t = "int";
                    break;
                case "float":
                case "real":
                    t = "double";
                    break;
                case "datetime":
                case "date":
                    t = "DateTime";
                    break;
                case "image":
                    t = "byte[]";
                    break;

            }

            return t;
        }

        /// <summary>
        /// Convert SQL Data types to SQL fix Data types
        /// </summary>
        /// <param name="sqlType">SQL Data type</param>
        /// <returns>SQL fix Data types</returns>
        public string ToSqlType(string type)
        {
            string t = "";

            switch (type.ToLower())
            {
                case "nvarchar":
                    t = "NVarChar";
                    break;
                case "varchar":
                    t = "VarChar";
                    break;
                case "char":
                    t = "Char";
                    break;
                case "text":
                    t = "NText";
                    break;
                case "bit":
                    t = "Bit";
                    break;
                case "int":
                    t = "Int";
                    break;
                case "float":
                    t = "Float";
                    break;
                case "real":
                case "double":
                    t = "Real";
                    break;
                case "datetime":
                    t = "DateTime";
                    break;
                case "image":
                    t = " Image";
                    break;

            }

            return t;
        }

        #endregion

        #region MVP Based method

        /// <summary>
        /// Generate C# code base on SQL Table 
        /// </summary>
        /// <returns>Class represent SQL Table</returns>
        public string GenerateClassCode(string tableName)
        {
            StringBuilder myCode = new StringBuilder();

            string lib = "using System;\n" +
                         "using System.Collections.Generic;\n" +
                         "using System.ComponentModel;\n" +
                         "using System.Linq;\n" +
                         "using System.Text;\n" +
                         "using System.Threading.Tasks;\n\n" +
                         "namespace " + projectName + ".BasicClass\n{\n" +
                         "\tpublic class " + tableName.ToUpper() + " : Bus\n\t{";
            myCode.AppendLine(lib);

            foreach (Columns col in tables[tableName])
            {
                string dataType = ToCSharpType(col.DataType);
                string valName = col.Name.ToLower();
                string propName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(col.Name.ToLower());

                myCode.AppendLine("\t\t//Comment for '" + valName + "' property");
                myCode.AppendLine("\t\tprivate " + dataType + " " + valName + ";\n");

                myCode.AppendLine("\t\t[DisplayName(\"" + propName + "\")]\n");

                myCode.AppendLine("\t\tpublic " + dataType + " " + propName);
                myCode.AppendLine("\t\t{");
                myCode.AppendLine("\t\t\tget { return " + valName + "; }");
                myCode.AppendLine("\t\t\tset");
                myCode.AppendLine("\t\t\t{");
                myCode.AppendLine("\t\t\t\t" + valName + " = value;");
                myCode.AppendLine("\t\t\t\tthis.MakeUpdate();");
                myCode.AppendLine("\t\t\t}");
                myCode.AppendLine("\t\t}");
                myCode.AppendLine();
                myCode.AppendLine();
            }

            myCode.AppendLine("\t}\n}");

            return myCode.ToString();
        }

        /// <summary>
        /// Generate C# code for MVP Model
        /// </summary>
        /// <param name="tableName">The name of the Table</param>
        /// <returns></returns>
        public string GenerateModelCode(string tableName)
        {
            StringBuilder myCode = new StringBuilder();
            var pkList = tables[tableName].Where(w => w.IsPrimaryKey).ToList();
            string conditional = string.Format("{0} = @{1}", pkList[0].Name, pkList[0].Name.ToLower());
            if (pkList.Count > 1)
            {
                for (int i = 1; i < pkList.Count; ++i)
                {
                    conditional += string.Format(" AND {0} = @{1}", pkList[i].Name, pkList[i].Name.ToLower());
                }
            }
            conditional += ";";

            #region Generate Library
            string lib = "using System;\n" +
                            "using System.Collections.Generic;\n" +
                            "using System.Data;\n" +
                            "using System.Linq;\n" +
                            "using System.Security.Cryptography;\n" +
                            "using System.Text;\n" +
                            "using System.Threading.Tasks;\n" +
                            "using System.Data.SqlClient;\n" +
                            "using " + projectName + ".BasicClass;\n" +
                            "using Ultility;\n\n" +
                            "namespace " + projectName + ".Model\n{\n" +
                            "\tpublic class " + tableName.ToLower() + "Model\n\t{\n";
            #endregion

            #region Generate GetData() Method
            string getdataMethod = "\t\tpublic List<" + tableName + "> GetData()\n\t\t{\n" +
                                   "\t\t\tvar " + tableName.ToLower() + "Lst = new List<" + tableName + ">();\n\n" +
                                   "\t\t\tSqlConnection con = new SqlConnection(\"" + connectionString + "\");\n" +
                                   "\t\t\tcon.Open();\n\n" +
                                   "\t\t\tstring sql = \"SELECT * FROM " + tableName + "\";\n" +
                                   "\t\t\tSqlCommand cmd = new SqlCommand(sql, con);\n" +
                                   "\t\t\tcmd.CommandType = CommandType.Text;\n\n" +
                                   "\t\t\tSqlDataReader reader = cmd.ExecuteReader();\n" +
                                   "\t\t\twhile (reader.Read())\n\t\t\t{\n" +
                                   "\t\t\t\tvar item = new " + tableName + "\n\t\t\t\t{\n";

            foreach (Columns col in tables[tableName])
            {
                string dataType = ToCSharpConvertType(col.DataType);
                string valName = col.Name.ToLower();
                string propName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(col.Name.ToLower());

                getdataMethod += "\t\t\t\t\t" + propName + " = " + dataType + "reader[\"" + valName + "\"]),\n";
            }

            getdataMethod += "\t\t\t\t};\n" +
                             "\n\t\t\t\titem.MakeUnChange();\n" +
                             "\t\t\t\t" + tableName.ToLower() + "Lst.Add(item);\n\t\t\t};\n\n" +
                             "\t\t\treader.Close();\n" +
                             "\t\t\tcon.Close();\n\n" +
                             "\t\t\treturn " + tableName.ToLower() + "Lst;\n\t\t}\n\n";
            #endregion

            #region Generate SaveData() Method
            StringBuilder saveMethod = new StringBuilder();

            saveMethod.AppendLine("\t\tpublic void SaveData(List<" + tableName + "> items)");
            saveMethod.AppendLine("\t\t{");

            saveMethod.AppendLine("\t\t\tforeach (var item in items)");
            saveMethod.AppendLine("\t\t\t{");

            saveMethod.AppendLine("\t\t\t\tif (item.State == RowState.Insert)");
            saveMethod.AppendLine("\t\t\t\t{");
            saveMethod.AppendLine("\t\t\t\t\tthis.AddNewItem(item);");
            saveMethod.AppendLine("\t\t\t\t}");

            saveMethod.AppendLine("\t\t\t\tif (item.State == RowState.Update)");
            saveMethod.AppendLine("\t\t\t\t{");
            saveMethod.AppendLine("\t\t\t\t\tthis.UpdateItem(item);");
            saveMethod.AppendLine("\t\t\t\t}");

            saveMethod.AppendLine("\t\t\t\tif (item.State == RowState.Delete)");
            saveMethod.AppendLine("\t\t\t\t{");
            saveMethod.AppendLine("\t\t\t\t\tthis.DeleteItem(item);");
            saveMethod.AppendLine("\t\t\t\t}");

            saveMethod.AppendLine("\t\t\t}");
            saveMethod.AppendLine("\t\t}");
            #endregion

            #region Generate AddNewItem(Class item) Method

            StringBuilder insertMethod = new StringBuilder();
            insertMethod.AppendLine("\t\tpublic void AddNewItem(" + tableName + " item)\n\t\t{");
            insertMethod.AppendLine("\t\t\tSqlConnection con = new SqlConnection(\"" + connectionString + "\");");
            insertMethod.AppendLine("\t\t\tcon.Open();\n");
            insertMethod.Append("\t\t\tstring sql = \"INSERT INTO " + tableName + " VALUES (");

            foreach (Columns col in tables[tableName]) insertMethod.Append("@" + col.Name.ToLower() + ",");
            insertMethod.Append(")\";\n");
            insertMethod = insertMethod.Replace(",)\";", ")\";");

            insertMethod.AppendLine("\t\t\tSqlCommand cmd = new SqlCommand(sql, con);");
            insertMethod.AppendLine("\t\t\tcmd.CommandType = CommandType.Text;\n");

            int n = 0;
            foreach (Columns col in tables[tableName])
            {
                n++;
                string dataType = ToCSharpConvertType(col.DataType);
                string valName = col.Name.ToLower();
                string propName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(col.Name.ToLower());

                insertMethod.AppendLine("\t\t\tSqlParameter p" + n.ToString() + " = new SqlParameter(\"@" + valName + "\", SqlDbType." + ToSqlType(col.DataType) + ");");
                insertMethod.AppendLine("\t\t\tp" + n.ToString() + ".Value = item." + propName + ";");
            }
            insertMethod.AppendLine();

            for (int i = 1; i <= n; ++i) insertMethod.AppendLine("\t\t\tcmd.Parameters.Add(p" + i.ToString() + ");");
            insertMethod.AppendLine();

            insertMethod.AppendLine("\t\t\tcmd.ExecuteNonQuery();");
            insertMethod.AppendLine("\t\t\tcon.Close();\n\t\t}\n");

            #endregion

            #region Generate DeleteItem() Method

            StringBuilder deleteMethod = new StringBuilder();
            deleteMethod.AppendLine("\t\tpublic void DeleteItem(" + tableName + " item)\n\t\t{");
            deleteMethod.AppendLine("\t\t\tSqlConnection con = new SqlConnection(\"" + connectionString + "\");");
            deleteMethod.AppendLine("\t\t\tcon.Open();\n");
            deleteMethod.AppendLine("\t\t\tstring sql = \"DELETE " + tableName + " WHERE " + conditional + "\";");
            deleteMethod.AppendLine("\t\t\tSqlCommand cmd = new SqlCommand(sql, con);");
            deleteMethod.AppendLine("\t\t\tcmd.CommandType = CommandType.Text;\n");

            n = 0;
            foreach (var item in pkList)
            {
                n++;
                deleteMethod.AppendLine(string.Format("\t\t\tSqlParameter p{0} = new SqlParameter(\"@{1}\", SqlDbType.{2});", n.ToString(), item.Name.ToLower(), ToSqlType(item.DataType)));
                deleteMethod.AppendLine(string.Format("\t\t\tp{0}.Value = item.{1};", n.ToString(), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Name.ToLower())));
                deleteMethod.AppendLine(string.Format("\t\t\tcmd.Parameters.Add(p{0});\n", n.ToString()));
            }

            deleteMethod.AppendLine("\t\t\tcmd.ExecuteNonQuery();");
            deleteMethod.AppendLine("\t\t\tcon.Close();\n\t\t}\n");

            #endregion

            #region Generate UpdateItem() Method

            StringBuilder updateMethod = new StringBuilder();
            updateMethod.AppendLine("\t\tpublic void UpdateItem(" + tableName + " item)\n\t\t{");
            updateMethod.AppendLine("\t\t\tSqlConnection con = new SqlConnection(\"" + connectionString + "\");");
            updateMethod.AppendLine("\t\t\tcon.Open();\n");
            updateMethod.Append("\t\t\tstring sql = \"UPDATE " + tableName + " SET ");
            foreach (Columns col in tables[tableName])
            {
                string valName = col.Name.ToLower();
                updateMethod.Append(valName + " = @" + valName + ",");
            }
            updateMethod.Append(" WHERE " + conditional + " \";\n\n");
            updateMethod = updateMethod.Replace(", WHERE", " WHERE");

            updateMethod.AppendLine("\t\t\tSqlCommand cmd = new SqlCommand(sql, con);");
            updateMethod.AppendLine("\t\t\tcmd.CommandType = CommandType.Text;\n");

            n = 0;
            foreach (Columns col in tables[tableName])
            {
                n++;
                string dataType = ToCSharpConvertType(col.DataType);
                string valName = col.Name.ToLower();
                string propName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(col.Name.ToLower());

                updateMethod.AppendLine("\t\t\tSqlParameter p" + n.ToString() + " = new SqlParameter(\"@" + valName + "\", SqlDbType." + ToSqlType(col.DataType) + ");");
                updateMethod.AppendLine("\t\t\tp" + n.ToString() + ".Value = item." + propName + ";");
            }
            updateMethod.AppendLine();

            for (int i = 1; i <= n; ++i) updateMethod.AppendLine("\t\t\tcmd.Parameters.Add(p" + i.ToString() + ");");
            updateMethod.AppendLine();

            updateMethod.AppendLine("\t\t\tcmd.ExecuteNonQuery();");
            updateMethod.AppendLine("\t\t\tcon.Close();\n\t\t}\n");

            #endregion

            myCode.AppendLine(lib);
            myCode.AppendLine(getdataMethod);
            myCode.AppendLine(saveMethod.ToString());
            myCode.AppendLine(insertMethod.ToString());
            myCode.AppendLine(deleteMethod.ToString());
            myCode.AppendLine(updateMethod.ToString());
            myCode.AppendLine("\t}");
            myCode.AppendLine("}");

            return myCode.ToString();
        }

        public string GenerateViewCode(string tableName)
        {
            StringBuilder myCode = new StringBuilder();

            string lib = "using System;\n" +
                            "using System.Collections.Generic;\n" +
                            "using System.Data;\n" +
                            "using System.Linq;\n" +
                            "using System.Security.Cryptography;\n" +
                            "using System.Text;\n" +
                            "using System.Threading.Tasks;\n" +
                            "using " + projectName + ".BasicClass;\n\n" +
                            "namespace " + projectName + ".View\n{\n" +
                            "\tpublic interface " + tableName.ToLower() + "IView\n\t{\n" +
                            "\t\tList<" + tableName + "> " + tableName.ToLower() + "Items {get; set; } \n" +
                            "\t\t" + tableName + " " + tableName.ToLower() + "Current {get; }\n" +
                            "\t\tvoid ReLoad();\n\t}\n}";

            myCode.Append(lib);

            return myCode.ToString();
        }

        public string GeneratePresenterCode(string tableName)
        {
            StringBuilder myCode = new StringBuilder();

            #region Library

            string lib = "using System;\n" +
                            "using System.Collections.Generic;\n" +
                            "using System.Data;\n" +
                            "using System.Linq;\n" +
                            "using System.Security.Cryptography;\n" +
                            "using System.Text;\n" +
                            "using System.Threading.Tasks;\n" +
                            "using " + projectName + ".BasicClass;\n" +
                            "using " + projectName + ".Model;\n" +
                            "using " + projectName + ".View;\n\n" +
                            "namespace " + projectName + ".Presenter\n{\n" +
                            "\tpublic class " + tableName.ToLower() + "Presenter\n\t{\n";

            #endregion

            #region Constructor

            StringBuilder cons = new StringBuilder();
            cons.AppendLine("\t\t" + tableName.ToLower() + "IView View;");
            cons.AppendLine("\t\t" + tableName.ToLower() + "Model model;\n");

            cons.AppendLine("\t\tpublic " + tableName.ToLower() + "Presenter (" + tableName.ToLower() + "IView view)");
            cons.AppendLine("\t\t{");
            cons.AppendLine("\t\t\tthis.View = view;");
            cons.AppendLine("\t\t\tmodel = new " + tableName.ToLower() + "Model();");
            cons.AppendLine("\t\t}");

            #endregion

            #region Method

            StringBuilder method = new StringBuilder();
            method.AppendLine("\t\tpublic void ShowData()");
            method.AppendLine("\t\t{");
            //method.AppendLine("\t\t\tif (View." + tableName.ToLower() + "Current == null) return;");
            method.AppendLine("\t\t\tView." + tableName.ToLower() + "Items = model.GetData();");
            method.AppendLine("\t\t\tView.ReLoad();");
            method.AppendLine("\t\t}\n");

            method.AppendLine("\t\tpublic void Save()");
            method.AppendLine("\t\t{");
            method.AppendLine("\t\t\tmodel.SaveData(View." + tableName.ToLower() + "Items);");
            method.AppendLine("\t\t}\n");

            method.AppendLine("\t\tpublic void AddNewRow()");
            method.AppendLine("\t\t{");
            method.AppendLine("\t\t\tvar item = new " + tableName + "();");
            method.AppendLine("\t\t\tView." + tableName.ToLower() + "Items.Add(item);");
            method.AppendLine("\t\t\tView.ReLoad();");
            method.AppendLine("\t\t}\n");

            method.AppendLine("\t\tpublic void DeleteRow()");
            method.AppendLine("\t\t{");
            method.AppendLine("\t\t\tvar cur = View." + tableName.ToLower() + "Current;");
            method.AppendLine("\t\t\tif (cur == null) return;");
            method.AppendLine("\t\t\tif (cur.State == RowState.Insert)");
            method.AppendLine("\t\t\t{");
            method.AppendLine("\t\t\t\tView." + tableName.ToLower() + "Items.Remove(cur);");
            method.AppendLine("\t\t\t\tView.ReLoad();");
            method.AppendLine("\t\t\t\treturn;");
            method.AppendLine("\t\t\t}");
            method.AppendLine("\t\t\tcur.MakeDelete();");
            method.AppendLine("\t\t\tView.ReLoad();");
            method.AppendLine("\t\t}\n");

            method.AppendLine("\t}\n}");

            #endregion

            myCode.Append(lib);
            myCode.AppendLine(cons.ToString());
            myCode.AppendLine(method.ToString());

            return myCode.ToString();
        }

        #endregion

        /// <summary>
        /// Simple write file to disk
        /// </summary>
        /// <param name="fileName">Path & Filename of  file</param>
        /// <param name="content">Content of file</param>
        public void WriteFile(string fileName, string content)
        {
            StreamWriter writer = new StreamWriter(fileName);
            writer.Write(content);
            writer.Close();
        }
        
        /// <summary>
        /// Quick create C# Project
        /// </summary>
        /// <param name="path"></param>
        public void CreateProject(string path)
        {
            Directory.CreateDirectory(path + @"\bin");
            Directory.CreateDirectory(path + @"\bin\Debug");
            Directory.CreateDirectory(path + @"\obj");
            Directory.CreateDirectory(path + @"\obj\Debug");
            Directory.CreateDirectory(path + @"\Properties");
            WriteFile(path + @"\Properties\AssemblyInfo.cs", MVPPowerToolv2.Properties.Resources.AssemblyInfo);
        }

        /// <summary>
        /// Create the folder construct of MVP
        /// </summary>
        public void Initialize()
        {
            string projStruct = "";
            StringBuilder sb = new StringBuilder();

            #region Create BasicClass project

            basicclassPath = path + @"\" + projectName + ".BasicClass";
            CreateProject(basicclassPath);
            basicclassPath = path + @"\" + projectName + ".BasicClass";
            projStruct = MVPPowerToolv2.Properties.Resources.CSProject;
            projStruct = projStruct.Replace("---ProjectName---", projectName + ".BasicClass");
            projStruct = projStruct.Replace("<Reference Include=\"---Library---\" />", "");
            sb.AppendLine(string.Format("<Compile Include=\"{0}\" />", "Bus.cs"));
            foreach (var tbl in tables.Keys)
            {
                sb.AppendLine(string.Format("<Compile Include=\"{0}\" />", tbl.ToUpper() + ".cs"));
            }
            projStruct = projStruct.Replace("<Compile Include=\"---File CS---\" />", sb.ToString());
            WriteFile(basicclassPath + @"\" + projectName + ".BasicClass.csproj", projStruct);

            #endregion

            #region Create Model Project

            sb.Clear();
            modelPath = path + @"\" + projectName + ".Model";
            CreateProject(modelPath);
            projStruct = MVPPowerToolv2.Properties.Resources.CSProject;
            projStruct = projStruct.Replace("---ProjectName---", projectName + ".Model");
            sb.AppendLine(string.Format("<Reference Include=\"{0}.BasicClass\" />", projectName));
            sb.AppendLine("<Reference Include=\"Ultility\" />");
            projStruct = projStruct.Replace("<Reference Include=\"---Library---\" />", sb.ToString());
            sb.Clear();
            foreach (var tbl in tables.Keys)
            {
                sb.AppendLine(string.Format("<Compile Include=\"{0}\" />", tbl.ToLower() + "Model.cs"));
            }
            projStruct = projStruct.Replace("<Compile Include=\"---File CS---\" />", sb.ToString());
            WriteFile(modelPath + @"\" + projectName + ".Model.csproj", projStruct);

            #endregion

            #region  Create View Project

            sb.Clear();
            viewPath = path + @"\" + projectName + ".View";
            CreateProject(viewPath);
            projStruct = MVPPowerToolv2.Properties.Resources.CSProject;
            projStruct = projStruct.Replace("---ProjectName---", projectName + ".View");
            sb.AppendLine(string.Format("<Reference Include=\"{0}.BasicClass\" />", projectName));
            projStruct = projStruct.Replace("<Reference Include=\"---Library---\" />", sb.ToString());
            sb.Clear();
            foreach (var tbl in tables.Keys)
            {
                sb.AppendLine(string.Format("<Compile Include=\"{0}\" />", tbl.ToLower() + "IView.cs"));
            }
            projStruct = projStruct.Replace("<Compile Include=\"---File CS---\" />", sb.ToString());
            WriteFile(viewPath + @"\" + projectName + ".View.csproj", projStruct);

            #endregion

            #region  Create Presenter Project

            sb.Clear();
            presenterPath = path + @"\" + projectName + ".Presenter";
            CreateProject(presenterPath);
            projStruct = MVPPowerToolv2.Properties.Resources.CSProject;
            projStruct = projStruct.Replace("---ProjectName---", projectName + ".Presenter");
            sb.AppendLine(string.Format("<Reference Include=\"{0}.BasicClass\" />", projectName));
            sb.AppendLine(string.Format("<Reference Include=\"{0}.Model\" />", projectName));
            sb.AppendLine(string.Format("<Reference Include=\"{0}.View\" />", projectName));
            projStruct = projStruct.Replace("<Reference Include=\"---Library---\" />", sb.ToString());
            sb.Clear();
            foreach (var tbl in tables.Keys)
            {
                sb.AppendLine(string.Format("<Compile Include=\"{0}\" />", tbl.ToLower() + "Presenter.cs"));
            }
            projStruct = projStruct.Replace("<Compile Include=\"---File CS---\" />", sb.ToString());
            WriteFile(presenterPath + @"\" + projectName + ".Presenter.csproj", projStruct);

            #endregion

            //Create folder
            Directory.CreateDirectory(basicclassPath);
            Directory.CreateDirectory(modelPath);
            Directory.CreateDirectory(viewPath);
            Directory.CreateDirectory(presenterPath);

            //Create Bus class
            StreamWriter writer = new StreamWriter(basicclassPath + @"\Bus.cs");
            writer.Write(MVPPowerToolv2.Properties.Resources.BusClass.Replace("---ProjectName---",projectName));
            writer.Close();
        }

        /// <summary>
        /// Generate all Table in Database to MVP Model
        /// </summary>
        public void OneGenerate()
        {
            Initialize();

            foreach (var tableName in tables.Keys)
            {
                WriteFile(basicclassPath + @"\" + tableName.ToUpper() + ".cs", GenerateClassCode(tableName));
                WriteFile(modelPath + @"\" + tableName.ToLower() + "Model.cs", GenerateModelCode(tableName));
                WriteFile(viewPath + @"\" + tableName.ToLower() + "IView.cs", GenerateViewCode(tableName));
                WriteFile(presenterPath + @"\" + tableName.ToLower() + "Presenter.cs", GeneratePresenterCode(tableName));
            }
        }

        /// <summary>
        /// Gennerate specific Table in Database to MVP Model
        /// </summary>
        /// <param name="tableName"></param>
        public void Generate(string tableName)
        {
            WriteFile(path + @"\" + tableName.ToUpper() + ".cs", GenerateClassCode(tableName));
            WriteFile(path + @"\" + tableName.ToLower() + "Model.cs", GenerateModelCode(tableName));
            WriteFile(path + @"\" + tableName.ToLower() + "IView.cs", GenerateViewCode(tableName));
            WriteFile(path + @"\" + tableName.ToLower() + "Presenter.cs", GeneratePresenterCode(tableName));
        }

        #endregion
    }
}
