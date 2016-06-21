using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Basic;
using Ultility;

namespace MVPPowerToolv2
{
    public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        SqlProvider sql;
        List<SQLDatabase> lstDatabase;
        Dictionary<string, List<Columns>> lstTable;
        string path = "";

        public frmMain()
        {
            InitializeComponent();

            lstDatabase = new List<SQLDatabase>();
            lstTable = new Dictionary<string, List<Columns>>();

            txtServerName.EditValue = CommonVariable.DefaultServerName;
            if (!CommonVariable.DefaultAuthentication)
            {
                switchAuthentication.Checked = true;
                txtUid.Enabled = true;
                txtPwd.Enabled = true;
            }
        }

        private void switchAuthentication_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!switchAuthentication.Checked)
            {
                switchAuthentication.Caption = "Windows Authentication";
                txtUid.Enabled = false;
                txtPwd.Enabled = false;
            }
            else
            {
                switchAuthentication.Caption = "SQL Sever Authentication";
                txtUid.Enabled = true;
                txtPwd.Enabled = true;
            }
        }

        private void btnConnect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                txtStatus.Caption = "Connecting to Server . . . ";
                listDatabase.Items.Clear();

                if (!switchAuthentication.Checked)
                {
                    sql = new SqlProvider(txtServerName.EditValue.ToString());
                }
                else
                {
                    sql = new SqlProvider(txtServerName.EditValue.ToString(), "master", txtUid.EditValue.ToString(), txtPwd.EditValue.ToString());
                }

                lstDatabase = sql.GetDatabases();
                listDatabase.DataSource = lstDatabase.Select(s => s.Name).ToList();

                txtStatus.Caption = string.Format("Connnect to Server [{0}] successful !", txtServerName.EditValue.ToString());
            }
            catch (Exception ex)
            {
                txtStatus.Caption = "Error !";
                //txtStatus.Caption = ex.Message;
            }
        }

        private async void listDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtStatus.Caption = "Loading table . . . ";

                listTable.DataSource = null;

                if (!switchAuthentication.Checked)
                {
                    sql = new SqlProvider(txtServerName.EditValue.ToString(), listDatabase.SelectedValue.ToString());
                }
                else
                {
                    sql = new SqlProvider(txtServerName.EditValue.ToString(), listDatabase.SelectedValue.ToString(), txtUid.EditValue.ToString(), txtPwd.EditValue.ToString());
                }

                lstTable = await sql.GetTables();
                listTable.DataSource = lstTable.Keys.ToList();

                txtStatus.Caption = string.Format("Connnect to Database [{0}] successful !", listDatabase.SelectedValue.ToString());
            }
            catch(Exception ex)
            {
                txtStatus.Caption = ex.Message;
            }
        }

        private void listTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                tableBindingSource.DataSource = lstTable[listTable.SelectedValue.ToString()];
                gridTable.DataSource = tableBindingSource;
            }
            catch (Exception ex)
            {
                txtStatus.Caption = ex.Message;
            }
        }

        private void btnSelectProject_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (switchProject.Checked) //Project mode
                {
                    OpenFileDialog fileDlg = new OpenFileDialog();

                    fileDlg.Title = "Open project file - MVP Power Tool v2.0";
                    fileDlg.Filter = "Visual C# Project files (*.csproj)|*.csproj";
                    fileDlg.ShowDialog();

                    path = Directory.GetParent(fileDlg.FileName).FullName;
                    txtProjectName.EditValue = new FileInfo(fileDlg.FileName).Name;
                }
                else
                {
                    FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                    folderDlg.ShowDialog();
                    path = folderDlg.SelectedPath;
                    txtProjectName.EditValue = folderDlg.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                txtStatus.Caption = ex.Message;
            }
        }

        private void switchProject_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (switchProject.Checked) //Project mode
            {
                switchProject.Caption = "Project generation";
                txtProjectName.Caption = "Project name";
            }
            else
            {
                switchProject.Caption = "Normal generation ";
                txtProjectName.Caption = "Path ";
            }
        }

        private void btnGenerate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                Dictionary<string, List<Columns>> tbl = new Dictionary<string, List<Columns>>();
                tbl.Add(listTable.SelectedValue.ToString(), tableBindingSource.DataSource as List<Columns>);
                string projectName = new DirectoryInfo(path).Name;

                MvpGenerateCore core = new MvpGenerateCore(projectName, sql.ConnectionString, tbl, path);
                core.Generate(tbl.Keys.ToList()[0]);

                txtStatus.Caption = string.Format("Generate [{0}] is completed !", tbl.Keys.ToList()[0]);
            }
            catch (Exception ex)
            {
                txtStatus.Caption = ex.Message;
            }
        }

        private async void btnOneGenerate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                string projectName = new DirectoryInfo(path).Name;

                MvpGenerateCore core = new MvpGenerateCore(projectName, sql.ConnectionString, lstTable, path);
                core.OneGenerate();

                if (switchProject.Checked) //Project mode
                {
                    //Read project file
                    StreamReader ifile = new StreamReader(path + "\\" + txtProjectName.EditValue.ToString().Replace(".csproj", ""));
                    string data = await ifile.ReadToEndAsync();
                    ifile.Close();

                    //Processing data


                    //Write project file
                    StreamWriter ofile = new StreamWriter(path + "\\" + txtProjectName.EditValue.ToString().Replace(".csproj", ""));
                    await ofile.WriteAsync(data);
                    ofile.Close();
                }

                txtStatus.Caption = "One Generate is completed !";
            }
            catch (Exception ex)
            {
                txtStatus.Caption = ex.Message;
            }
        }
    }
}
