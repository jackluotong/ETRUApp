using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static ETRU_TestBench.MainWindow;
using System.Threading;
using System.Text;

namespace ETRU_TestBench
{
    /// <summary>
    /// ComWindow.xaml 的交互逻辑
    /// </summary>
    ///


    public partial class ComWindow : MetroWindow
    {
        #region Reference  
        public enum BaudRateParam : int
        {
            _300 = 300,
            _600 = 600,
            _1200 = 1200,
            _2400 = 2400,
            _4800 = 4800,
            _9600 = 9600
        }
        public COMPortSettingClass COMPortSetting;
        public List<byte> list_COM_1 = new List<byte>(4096);//RTU串口接收数据列表
        private delegate void DelegateCommuniction(string mySendData);//委托RS232数据显示    
        #endregion

        public ComWindow()
        {
            InitializeComponent();
            MessageExt.Instance.ShowDialog = ShowDialog;
        }

        #region Windows Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] strCom = SerialPort.GetPortNames();
            if (strCom == null)
            {
                MessageExt.Instance.ShowDialog("本机没有串口！", "提示");
                return;
            }
            foreach (string com in System.IO.Ports.SerialPort.GetPortNames())
            {
                cmBoxPortName.Items.Add(com);
            }
            foreach (var item in Enum.GetValues(typeof(BaudRateParam)))
            {
                cmBoxBaudRate.Items.Add(item.ToString());
            } 
            foreach (var item in Enum.GetValues(typeof(System.IO.Ports.Parity)))
            {
                cmBoxParity.Items.Add(item.ToString());
            }
            foreach (var item in Enum.GetValues(typeof(System.IO.Ports.StopBits)))
            {
                this.cmBoxStopBits.Items.Add(item.ToString());
            }
            //set default value
            this.cmBoxDataBits.Items.Add("8");
            this.txtTimeOut.Text = string.Concat(1000);
            sp.DtrEnable = true;
            sp.RtsEnable = true;
            cmBoxPortName.SelectedIndex = 0;
            cmBoxBaudRate.SelectedIndex = 2;
            cmBoxParity.SelectedIndex = 0;
            cmBoxStopBits.SelectedIndex = 1;
            cmBoxDataBits.SelectedIndex = 0;

            System.Windows.Threading.DispatcherTimer dispatcher = new System.Windows.Threading.DispatcherTimer();
            dispatcher.Tick += new EventHandler(dispatcher_Tlick);
            dispatcher.Interval = new TimeSpan(0,0,1);
            dispatcher.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (COMPortSetting != null)
            {
                COMPortSetting.DisconnectSerialPort();
            }
        }

        #endregion

        #region Click Events
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.cmBoxPortName.ItemsSource = SerialPort.GetPortNames();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btnOpen.Content.ToString() == "连接")
                {
                    if (!sp.IsOpen)
                    {
                        int baudrate = (int)Enum.Parse(typeof(BaudRateParam), this.cmBoxBaudRate.SelectedValue.ToString());
                        int dataBits = int.Parse(this.cmBoxDataBits.SelectedValue.ToString());
                        int timeout = int.Parse(this.txtTimeOut.Text.Trim().ToString());
                        sp.PortName = this.cmBoxPortName.SelectedValue.ToString();
                        sp.BaudRate = baudrate;
                        sp.DataBits = int.Parse(this.cmBoxDataBits.SelectedValue.ToString());
                        sp.StopBits = (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), this.cmBoxStopBits.SelectedValue.ToString());
                        sp.ReadTimeout = timeout;
                        sp.Parity = (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), this.cmBoxParity.SelectedValue.ToString());
                        sp.DataReceived += sp_DataReceived;
                        sp.Open();
                        this.AddMessage("串口打开成功");
                        this.btnOpen.Content = "断开";
                    }
                    else
                    {
                        sp.Close();
                        int baudrate = (int)Enum.Parse(typeof(BaudRateParam), this.cmBoxBaudRate.SelectedValue.ToString());
                        int dataBits = int.Parse(this.cmBoxDataBits.SelectedValue.ToString());
                        int timeout = int.Parse(this.txtTimeOut.Text.Trim().ToString());
                        sp.PortName = this.cmBoxPortName.SelectedValue.ToString();
                        sp.BaudRate = baudrate;
                        sp.DataBits = int.Parse(this.cmBoxDataBits.SelectedValue.ToString());
                        sp.StopBits = (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), this.cmBoxStopBits.SelectedValue.ToString());
                        sp.ReadTimeout = timeout;
                        sp.Parity = (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), this.cmBoxParity.SelectedValue.ToString());
                        sp.Open();
                        this.AddMessage("串口打开成功");
                        this.btnOpen.Content = "断开";
                    }
                }
                else
                {
                    sp.Close();
                    this.AddMessage("串口已关闭");
                    this.btnOpen.Content = "连接";
                }

            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("打开串口出错！" + ex.Message, "提示");
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.cmBoxBaudRate.Text = string.Empty;
            this.cmBoxDataBits.Text = string.Empty;
            this.cmBoxParity.Text = string.Empty;
            this.cmBoxPortName.Text = string.Empty;
            this.cmBoxStopBits.Text = string.Empty;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!sp.IsOpen)
                {
                    MessageExt.Instance.ShowDialog("请先打开串口", "提示");
                    return;
                }
                else
                {
                    this.SendDataCom(this.SendBox.Text.ToString()+ "\r"); 
                }
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("发送错误" + ex.Message,"提示");
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sp.IsOpen)
            {
                int n = sp.BytesToRead;
                byte[] buf = new byte[n];
                sp.Read(buf, 0, n);
                string StrOnForm = System.Text.Encoding.Default.GetString(buf);
                ShowCommunictionInvoke(StrOnForm);
                list_COM_1.AddRange(buf);
            }
            else
            {
                MessageExt.Instance.ShowDialog("请先打开串口", "提示");
            }
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\Info\Receive.txt";
                string content = this.ReceiveBox.Text;
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter write = new StreamWriter(fs);
                write.Write(content);
                write.Flush();
                write.Close();
                fs.Close();
                this.AddMessage("信息存在：" + path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtMessage.Text = string.Empty;
        }

        private void btnSendClear_Click(object sender, RoutedEventArgs e)
        { 
            this.SendBox.Text = string.Empty; 
        }

        private void btnReceiveClear_Click(object sender, RoutedEventArgs e)
        {
            this.ReceiveBox.Text = string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\Info\ComWindow.txt";
                string content = this.txtMessage.Text.ToString();
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter write = new StreamWriter(fs);
                write.Write(content);
                write.Flush();
                write.Close();
                fs.Close();
                this.AddMessage("信息存在：" + path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnLook_Click(object sender,RoutedEventArgs e)
        {
        
        }
        #endregion

        #region Info Module

        internal void AddMessage(string messageLine)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run() { Text = messageLine, Foreground = Brushes.Red };
            paragraph.Inlines.Add(run);
            this.txtMessage.AppendText(DateTime.Now.ToString() + " :" + messageLine + Environment.NewLine);
            this.txtMessage.ScrollToEnd();
        }

        public  void sendData(string data) //before
        {
            try
            {
                if (!sp.IsOpen)
                {
                    sp.Open();
                }
                byte[] Data = System.Text.Encoding.Default.GetBytes(data);
                sp.Write(Data, 0, Data.Length);
                byte[] enter = HexStringToByteArray("0D");
                sp.Write(enter, 0, 1);
            }
            catch (Exception ex)
            {
                this.AddMessage("发送数据出错" + ex.Message);
            }
        } 

        public byte[] HexStringToByteArray(string s)//将16进制的字符串转化为byte型
        {
            s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = s.Substring(0, s.Length - 1) + "0" + s.Substring(s.Length - 1);
            }
            byte[] buffer = new byte[s.Length / 2];

            try
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch
            {
                string errorString = "E4";
                byte[] errorData = new byte[errorString.Length / 2];
                errorData[0] = (byte)Convert.ToByte(errorString, 16);
                return errorData;
            }
        }

        public void ShowCommunictionInvoke(string Data)//将接受的数据显示出来
        {
            DelegateCommuniction SC = ShowCommuniction;
            Application.Current.Dispatcher.Invoke(SC, Data);
        }

        private void ShowCommuniction(string Data)
        {
            this.ReceiveBox.Text += Data;
            this.ReceiveBox.ScrollToEnd();
        }
        #endregion

        #region Check COM Set

        public bool CheckCom()
        {
            if (!sp.IsOpen)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Define function
        public sealed class MessageExt
        {
            private static readonly MessageExt insstance = new MessageExt();
            private MessageExt()
            {
            }

            public static MessageExt Instance
            {
                get
                {
                    return insstance;
                }
            }
            public Action<string, string> ShowDialog { get; set; }
        }

        public async void ShowDialog(string message, string title)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "关闭",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
        }

        public async void ShowYesNo(string message, string title, Action action)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "确定",
                NegativeButtonText = "取消",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, mySettings);
            if (result == MessageDialogResult.Affirmative)
            {
                await Task.Factory.StartNew(action);
            }
        }

        //查询串口收到的数据是否有需要的
        public bool CheckReceiveData(List<byte> L,string s)
        { 
            string Str = System.Text.Encoding.Default.GetString(L.ToArray());
            if (Str.Contains(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //查询显示需要的数据
        public string SelectListDataString(List<byte> L, string s) 
        {
            string Str = System.Text.Encoding.Default.GetString(L.ToArray());
            int i = Str.IndexOf(s);
            if (i == -1)
            {
                return "Null";
            }
            else
            {
                switch (s)
                {
                    case "*":
                        {
                            byte[] p = new byte[10];
                            for (int j = 0; j < p.Length; j++)
                            {
                                p[i] = L.ToArray()[i + j];
                            } 
                            return System.Text.Encoding.Default.GetString(p);
                        } 
                }
                return "null";
            }
        } 
        //发送数据处理
        public void SendDataCom(string dt)
        {
            Char[] ch = dt.ToCharArray(); 
            for (int i = 0; i < ch.Length; i++)
            {
                sp.Write(ch, i, 1);
                sp.Write("\r"); 
            }
        }
        //串口初始化
        public void COMInit(string COM)
        {
            try
            {
                if (sp.IsOpen)
                {
                    sp.DiscardInBuffer();
                    sp.DiscardOutBuffer();
                    list_COM_1 = new List<byte> { }; 
                }
                else
                {
                    MessageExt.Instance.ShowDialog("串口没有打开", "提示");
                }
            }
            catch(Exception e)
            {
                throw e; 
            }
        }
        //时间显示
        private void dispatcher_Tlick(object sender, EventArgs e)
        {
            this.TextTime.Text = DateTime.Now.ToString();
        }
        #endregion

    }

}
