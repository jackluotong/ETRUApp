using System;
using System.ComponentModel;
using System.Data.OleDb;
using System.Threading;
using System.Threading.Tasks;

namespace ETRU_TestBench
{
    class TestCourse
    { 
        public bool Connect()
        {

            OleDbConnectionStringBuilder oleString = new OleDbConnectionStringBuilder(); //为了使大家更清楚使用这个类，制造一个连接字符串
            oleString.Provider = "Microsoft.ACE.OleDB.15.0"; //使用刚刚安装的数据库引擎，大家不要写错了 
            oleString.DataSource = @"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\bin\Debug.accdb"; //这里写你数据库连接的位置 
            OleDbConnection conn = new OleDbConnection(); //创建OleDb连接对象 
            conn.ConnectionString = oleString.ToString(); //将生成的字符串传入
            conn.Open(); //打开数据库 
            OleDbCommand mycmd = new OleDbCommand(); //创建sql命令对象
            mycmd.Connection = conn; //设置连接 
            mycmd.CommandText = "Insert into Users(用户名,密码,家庭地址) values(@name,@pwd,@address)"; //并且用sql参数形式插入数据 
            mycmd.Parameters.AddWithValue("@name", "apple"); mycmd.Parameters.AddWithValue("@pwd", "password"); mycmd.Parameters.AddWithValue("@address", "address1"); //加入参数值 
            mycmd.ExecuteNonQuery(); //执行插入语句 
            conn.Close(); //最后不要忘了关数据库
            mycmd.Dispose();
            return false;
        }
       
    }
}
