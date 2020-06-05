using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;

namespace ETRU_TestBench
{
    class DatabaseMethod
    {
        ComboBox MyComboBox;
        DataGrid MyDataGrid;
        private delegate int DelegateComboBox();//委托
        private delegate void DelegateDataGrid(DataTable DT);

        public struct ETRUMoudleTestResult
        {
            public string ETRUID;
            public string OrdNumber;
            public string SerNumber;
            public string StartTime;
            public string EndTime;
            public string EndState;

        }

        public struct ETRUFullTestResult
        {
            public string ETRUID;
            public string StartTime;
            public string EndTime;
            public string EndState;
            public string OrdNumber;
        }

        public DatabaseMethod(ComboBox TheComboBox, DataGrid TheDataGrid)
        {
            MyComboBox = TheComboBox;
            MyDataGrid = TheDataGrid;
        }

        public DatabaseMethod(DataGrid TheDataGrid)
        {
            MyDataGrid = TheDataGrid;
        }

        private void DataGridInvoke(DataTable DT)
        {
            DelegateDataGrid DDG = DataGridShow;
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                DDG(DT);
            }));
        }

        private void DataGridShow(DataTable DT)
        {
            MyDataGrid.ItemsSource = DT.DefaultView;
        }

        private int ComboBoxIndexInvoke()
        {
            object i = MyComboBox.Dispatcher.Invoke(new DelegateComboBox(ComboBoxIndex));
            return Convert.ToInt32(i);
        }

        private int ComboBoxIndex()
        {
            return this.MyComboBox.SelectedIndex;
        }

        public string GetTimeNow()
        {
            string Time = DateTime.Now.ToString();
            return Time;
        }

        public bool ConnectETRUDBTestResult(int IndexOfBase, int IndexOfTable, string ETRUID, string EndStatePass, bool Recent)
        {
            try
            {
                string strcon = string.Empty;
                switch (IndexOfBase)
                {
                    case 0:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_First.accdb";
                            break;
                        }
                    case 1:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Second.accdb";
                            break;
                        }
                    case 2:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Thrid.accdb";
                            break;
                        }
                    case 3:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fourth.accdb";
                            break;
                        }
                    case 4:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fifth.accdb";
                            break;
                        }
                    default: break;
                }
                OleDbConnection con = new OleDbConnection();
                OleDbCommand cmd = null;
                con = new OleDbConnection(strcon);
                con.Open();
                cmd = con.CreateCommand();
                string sql = string.Empty;
                if (ETRUID != "")
                {
                    switch (IndexOfTable)
                    {
                        case 0:
                            {
                                sql = "select * from ETRU_MoudleDB where ETRUID= '" + ETRUID + "'";
                                break;
                            }
                        case 1:
                            {
                                sql = "select * from ETRU_FullDB where ETRUID='" + ETRUID + "'";
                                break;
                            }
                        default: break;
                    }
                }
                if (EndStatePass != "")
                {
                    switch (IndexOfTable)
                    {
                        case 0:
                            {
                                sql = "select * from ETRU_MoudleDB where EndState= '" + EndStatePass + "'";
                                break;
                            }
                        case 1:
                            {
                                sql = "select * from ETRU_FullDB where EndState= '" + EndStatePass + "'";
                                break;
                            }
                        default: break;
                    }
                }
                if (Recent)
                {
                    switch (IndexOfTable)
                    {
                        case 0:
                            {
                                sql = "select top 100 * from ETRU_MoudleDB order by ID desc";
                                break;
                            }
                        case 1:
                            {
                                sql = "select top 100 * from ETRU_FullDB order by ID desc";
                                break;
                            }
                        default: break;
                    }
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                OleDbDataAdapter da = new OleDbDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DataGridInvoke(dt);
                con.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //连接配置数据库，读取测试步骤所有数据
        public bool ConnectETRUConfigDBShowTestCourse()
        {
            try
            {
                string strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUConfigDB.accdb";
                OleDbConnection con = new OleDbConnection(strcon);
                OleDbCommand cmd = null;
                con.Open();
                cmd = con.CreateCommand();
                string sql;
                if (ComboBoxIndexInvoke() == 0)
                {
                    sql = "select * from ETRUMoudle_Test";
                }
                else
                {
                    sql = "select * from ETRUFull_Test";
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                OleDbDataAdapter da = new OleDbDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DataGridInvoke(dt);
                con.Close();
                return true;
            }
            catch (Exception  )
            {
                return false; 

            }
        }

        //读取ID是否已经存在
        public bool SelectETRUIDTested(int IndexOfBase, int IndexOfTable, string ETRUID)
        {
            try
            {
                string strcon = string.Empty;
                switch (IndexOfBase)
                {
                    case 0:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_First.accdb";
                            break;
                        }
                    case 1:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Second.accdb";
                            break;
                        }
                    case 2:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Thrid.accdb";
                            break;
                        }
                    case 3:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fourth.accdb";
                            break;
                        }
                    case 4:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fifth.accdb";
                            break;
                        }
                    default: break;
                }
                OleDbConnection con = new OleDbConnection();
                OleDbCommand cmd = null;
                con = new OleDbConnection(strcon);
                con.Open();
                cmd = con.CreateCommand();
                string sql = string.Empty;
                if (ETRUID != "")
                {
                    switch (IndexOfTable)
                    {
                        case 0:
                            {
                                sql = "select * from ETRU_MoudleDB where ETRUID = " + "'" + ETRUID + "'" + "and EndState = 'Pass'";
                                break;
                            }
                        case 1:
                            {
                                sql = "select * from ETRU_FullDB where ETRUID = " + "'" + ETRUID + "'" + "and EndState = 'Pass'";
                                break;
                            }
                    }
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                OleDbDataAdapter da = new OleDbDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count >= 1)
                {
                    con.Close();
                    return false;
                }
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
        public bool InsertIntoAccess(int IndexOfBase, int IndexOfTable, ETRUMoudleTestResult eTRUMoudleTestResult, ETRUFullTestResult eTRUFullTestResult)
        {
            try
            {
                string strcon = string.Empty;
                switch (IndexOfBase)
                {
                    case 0:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_First.accdb";
                            break;
                        }
                    case 1:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Second.accdb";
                            break;
                        }
                    case 2:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Thrid.accdb";
                            break;
                        }
                    case 3:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fourth.accdb";
                            break;
                        }
                    case 4:
                        {
                            strcon = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUDB_Fifth.accdb";
                            break;
                        }
                    default: break;
                }
                OleDbConnection con = new OleDbConnection(); 
                con = new OleDbConnection(strcon);
                con.Open(); 
                string sql = string.Empty;
                switch (IndexOfTable)
                {
                    case 0:
                        {
                            sql = @"insert into ETRU_MoudleDB (ETRUID,OrdNumber,SerNumber,StartTime,EndTime,EndState)
                                    values('"+eTRUMoudleTestResult.ETRUID+"','"+eTRUMoudleTestResult.OrdNumber+"','"
                                    +eTRUMoudleTestResult.SerNumber+"','"+eTRUMoudleTestResult.StartTime+"','"
                                    +eTRUMoudleTestResult.EndTime+"','"+eTRUMoudleTestResult.EndState+"')"; 
                            break;
                        }
                    case 1:
                        {
                            sql = @"insert into ETRU_FullDB (ETRUID,OrdNumber,StartTime,EndTime,EndState)
                                    values('" + eTRUFullTestResult.ETRUID + "','" + eTRUFullTestResult.OrdNumber + "','"
                                     + eTRUFullTestResult.StartTime + "','"+ eTRUFullTestResult.EndTime + "','"
                                     +eTRUFullTestResult.EndState + "')";
                            break;
                        }
                }
                OleDbCommand myCommand = new OleDbCommand(sql,con);
                myCommand.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                throw (new Exception("写入数据库出错"+e.Message)); 
            }

        }

        //查询当前使用的数据库名称
        public bool SelectCurrentDatabase()
        {
            PublicClass.CurrentDatabase = string.Empty;
            string connstr = "Provider=Microsoft.ACE.OLEDB.15.0;Data Source=|DataDirectory|\\ETRUConfigDB.accdb";
            OleDbConnection tempconn = new OleDbConnection(connstr);
            try
            {
                string strCom = "Select DatabaseName from CurrentDatabase where ID=" + (int)1;
                OleDbCommand myCommand = new OleDbCommand(strCom, tempconn);
                tempconn.Open();
                OleDbDataReader reader;
                reader = myCommand.ExecuteReader();
                if (reader.Read())
                {
                    PublicClass.CurrentDatabase = reader["DatabaseName"].ToString();
                }
                else
                {
                    return false;
                }
                reader.Close();
                tempconn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(""+ex.ToString());
                return false;
            }
            finally
            {
                tempconn.Close();
            }
            return true;
        }
    }
}
