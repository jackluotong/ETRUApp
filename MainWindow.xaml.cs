#region Using
using ControlzEx.Standard;
using ETRU_TestBench.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Threading;
using static ETRU_TestBench.DataGridClass;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
#endregion


namespace ETRU_TestBench
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region  Reference
        public enum AllDatabase
        {
            ETRUDB_First = 0,
            ETRUDB_Second = 1,
            ETRUDB_Thrid = 2,
            ETRUDB_Fourth = 3,
            ETRUDB_Fifth = 4
        }
        public enum RunCommands { RunETRUModel, RunEtruFull }
        private DatabaseMethod MyDatabaseMethod;
        private DatabaseMethod.ETRUFullTestResult myETRUFullTest;
        private DatabaseMethod.ETRUMoudleTestResult myETRUMoudleTest;
        private delegate void DataGridDelegate(int line, bool bo, bool IsMaxLine);
        private CancellationTokenSource cancelTokenSource; 
        public COMPortSettingClass myCom;
        private ComWindow myWindow;
        public static SerialPort sp = new SerialPort();
        private System.Windows.Threading.DispatcherTimer MyTestTimeTimer;
        private long PruefzeitTicksStart;
        private long PruefzeitTicksJetzt;
        private long PruefzeitInSekunden;
        ModelWindows model = new ModelWindows();

        #endregion

        #region Window events
        public MainWindow()
        {
            InitializeComponent();
            List<string> TestType = new List<string>();
            TestType.Add("模块测试");
            TestType.Add("整表测试");
            this.comBoxTestCourse.ItemsSource = TestType;
            List<string> list = new List<string>();
            list.Add("模块测试结果");
            list.Add("整表测试结果");
            this.comBoxTestResult.ItemsSource = list;
            MyDatabaseMethod = new DatabaseMethod(this.comBoxTestCourse, this.DataGridTestCourse);
            myWindow = new ComWindow();
            MessageExt.Instance.ShowDialog = ShowDialog;
            //timer
            this.MyTestTimeTimer = new DispatcherTimer(DispatcherPriority.Normal);
            this.MyTestTimeTimer.Tick += new EventHandler(TestTimeTimer_Tick);
            this.MyTestTimeTimer.Interval = new TimeSpan(0, 0, 1); 
        }

         private async void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            myWindow.ShowDialog();
            if (PublicClass.ContinueToTest)
            {
                try
                {
                    if (!MyDatabaseMethod.SelectCurrentDatabase())
                    {
                        PublicClass.ContinueToTest = false;
                         MessageExt.Instance.ShowDialog("数据库连接出错","提示");
                        await Task.Delay(10);
                        return;
                    }
                    else
                    {
                        this.txtDatabase.Text = PublicClass.CurrentDatabase;
                    }
                }
                catch (Exception ex)
                {
                    MessageExt.Instance.ShowDialog("数据库连接出错"+ex.ToString(), "提示");
                }
               
            }  
        } 
        #endregion

        #region click events

        private void btnComsetting_Click(object sender, RoutedEventArgs e)
        {
            ComWindow myWindow = new ComWindow();
            myWindow.ShowDialog();
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            this.textBox.Clear();
             if (!this.myWindow.CheckCom())
            {
                MessageExt.Instance.ShowDialog("串口没有配置", "提示");
                return;
            }

            if (!this.CheckInput())
            {
                MessageExt.Instance.ShowDialog("请输入正确信息", "提示");
                return;
            }

            if (!CheckReaptID())
            {
                return;
            }
            if (comBoxSelect.SelectedIndex == -1)
            {
                MessageExt.Instance.ShowDialog("请选择数据位","提示");
                return;
            }
            //begin test
            switch (this.comBoxTestCourse.SelectedIndex)
            {
                case 0:
                    {
                        bool result = false;
                        try
                        {
                            PruefzeitTicksStart = DateTime.Now.Ticks; 
                            this.AddMessage("开始模块测试");
                            myETRUMoudleTest.ETRUID = this.IDNumber.Text.Trim().ToString();
                            myETRUMoudleTest.StartTime = MyDatabaseMethod.GetTimeNow();
                            myETRUMoudleTest.SerNumber = this.SerialNumber.Text.Trim().ToString();
                            myETRUMoudleTest.OrdNumber = this.orderNumber.Text.Trim().ToString();
                            cancelTokenSource = new CancellationTokenSource();
                            PublicClass.IsRunning = true;
                            result = await RunCommandAsync(RunCommands.RunETRUModel);  
                            if (!result)
                            {
                                myETRUMoudleTest.EndState = "Fail";
                                this.AddMessage("模块测试失败，等待写入数据库···");
                                myETRUMoudleTest.EndTime = MyDatabaseMethod.GetTimeNow();
                            }
                            else
                            {
                                myETRUMoudleTest.EndState = "Pass";
                                this.AddMessage("模块测试成功，等待写入数据库···");
                                myETRUMoudleTest.EndTime = MyDatabaseMethod.GetTimeNow();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageExt.Instance.ShowDialog("模块测试出错" + ex.Message,"提示");
                            myETRUMoudleTest.EndState = "Fail";
                        }
                        try
                        {
                             await this.WriteTestResultIntoDatabaseAsync();

                        }
                        catch (Exception ex)
                        {
                            MessageExt.Instance.ShowDialog("数据写入出错" + ex.Message, "提示");
                            return;
                        }
                        break;
                    }
                case 1:
                    {
                        bool result = false;
                        try
                        {
                            this.AddMessage("这是整表测试");
                            myETRUFullTest.ETRUID = this.IDNumber.Text.Trim().ToString();
                            myETRUFullTest.StartTime = MyDatabaseMethod.GetTimeNow();
                            myETRUFullTest.OrdNumber = this.orderNumber.Text.Trim().ToString();
                            cancelTokenSource = new CancellationTokenSource();
                            PublicClass.IsRunning = true;
                            result= await RunCommandAsync(RunCommands.RunEtruFull);
                            if (!result)
                            {
                                myETRUFullTest.EndState = "Fail";
                                this.AddMessage("整表测试失败，等待写入数据库···");
                                myETRUFullTest.EndTime = MyDatabaseMethod.GetTimeNow();
                            }
                            else
                            {
                                myETRUFullTest.EndState = "Pass";
                                this.AddMessage("整表测试成功，等待写入数据库···");
                                myETRUMoudleTest.EndTime = MyDatabaseMethod.GetTimeNow();
                            }
                        }
                        catch (Exception ex)
                        {
                            myETRUFullTest.EndState = "Fail";
                            MessageExt.Instance.ShowDialog("整表测试失败" + ex.Message, "提示");
                        }
                        try
                        {
                             await WriteTestResultIntoDatabaseFullAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageExt.Instance.ShowDialog("整表写入数据库失败" + ex.Message, "提示");
                        }
                        break;
                    }
                default:
                    MessageExt.Instance.ShowDialog("请选择测试项目","提示");
                    break;
            }  
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.cancelTokenSource.Cancel();
            this.Close();
        }

        private async void comBoxTestCourse_SelectionChanged(object sender, RoutedEventArgs e)
        {
            bool result = await cmbTestType_SelectionChangedAsync();
            if (result)
            {
                PublicClass.ContinueToTest = true;
            }
            else
            {
                PublicClass.ContinueToTest = false;
                this.AddMessageError("从数据库获取测试流程失败");
                return;
            }
        }

        private Task<bool> cmbTestType_SelectionChangedAsync()
        {
            return Task.Run<bool>(() =>
            {
                return MyDatabaseMethod.ConnectETRUConfigDBShowTestCourse();
            });
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Operation operation = new Operation();
            operation.ShowDialog();
        }

        private void  Check_Click(object sender, RoutedEventArgs e)
        {
            if (this.IDNumber.Text.Length != 8 || this.IDNumber.Text == null)
            {
                MessageExt.Instance.ShowDialog("请先输入正确的产品编号", "提示");
                return;
            }
            else
            {
                try
                {
                    if (this.comBoxTestCourse.SelectedIndex == 0)
                    {
                        if (!MyDatabaseMethod.SelectETRUIDTested((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 0, this.IDNumber.Text.Trim().ToString()))
                        {
                            MessageExt.Instance.ShowDialog("产品编号重复", "提示");
                            return;
                        }
                        else
                        {
                            this.AddMessage("产品编号没有重复可以继续测试");
                            return;
                        }
                    }
                    if (this.comBoxTestCourse.SelectedIndex == 1)
                    {
                        if (!MyDatabaseMethod.SelectETRUIDTested((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 1, this.IDNumber.Text.Trim().ToString()))
                        {
                            this.AddMessage("产品编号重复");
                            return;
                        }
                        else
                        {
                            this.AddMessage("产品编号没有重复可以继续测试");
                            return;
                        }
                    }
                    else
                    {
                        MessageExt.Instance.ShowDialog("请选择测试类型", "提示");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageExt.Instance.ShowDialog("错误信息" + ex.ToString(), "提示");
                    return;
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process open = new System.Diagnostics.Process();
                open.StartInfo.FileName = @"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\bin\Debug\ETRUDB_First.accdb";
                open.Start();
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("打开文件失败"+ex.Message,"提示");
            } 
        }

        private void btnClear_Click(object sender,RoutedEventArgs e)
        {
            this.textBox.Text = "";
            this.IDNumber.Text = "";
            this.SerialNumber.Text = "";
            this.orderNumber.Text = "";
            this.comBoxTestCourse.SelectedIndex = -1;
            this.comBoxSelect.SelectedIndex = -1;
        }

        private void btnSave_Click(object sender,RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "*.txt|",//JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif
                    Title = "Save File",
                    FileName="测试信息表"+DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")
                };
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        string content = "测试窗口信息:" + textBox.Text + "\r\n" + "产品编号:" + IDNumber.Text + "\r\n" + "序列号:" + SerialNumber.Text + "\r\n" + "订单号:" + orderNumber.Text + "\r\n";
                        sw.Write(DateTime.Now + "\r\n" + content);
                        sw.WriteLine("-------------------");
                        sw.WriteLine("Done");
                        sw.Close();
                    }
                 }
                else
                {
                    MessageExt.Instance.ShowDialog("信息保存异常", "提示"); 
                }
            }
            catch(Exception ex)
            {
                MessageExt.Instance.ShowDialog("信息保存异常"+ex.Message,"提示");
            }
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            Read readWindow = new Read();
            readWindow.ShowDialog();
        }

        private void comBoxTestResult_SelectionChanged(object sender, RoutedEventArgs e)
        {
            MessageExt.Instance.ShowDialog("等待开发","提示");
            this.comBoxTestResult.IsEnabled = false;
        }

        private void Model_Click(object sender, RoutedEventArgs e)
        {
            model.ShowDialog();
         }

        #endregion

        #region Addmessage
        internal void AddMessageError(string messageLine)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run() { Text = messageLine, Background = System.Windows.Media.Brushes.Red};
            paragraph.Inlines.Add(run);
            this.textBox.AppendText(DateTime.Now.ToString() + " :" + messageLine + Environment.NewLine);
            //this.textBox.Foreground = System.Windows.Media.Brushes.Red;
            this.textBox.ScrollToEnd();

        }

        internal void AddMessage(string messageLine)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run() { Text = messageLine, Foreground = System.Windows.Media.Brushes.Black};
            paragraph.Inlines.Add(run);
            this.textBox.AppendText(DateTime.Now.ToString() + " :" + messageLine + Environment.NewLine); 
            this.textBox.ScrollToEnd();
         }
        #endregion

        #region Test flow

        private async Task<bool> RunETRUMoudleTest(CancellationToken cancelToken)
        {
            try
            { 
                bool result = false; 
                result = await SetPassword(cancelToken);
                if (!result)
                { 
                    ChangeOneStateInvoke(1, false, false);
                    return result;
                }
                Thread.Sleep(100);
                ChangeOneStateInvoke(1, true, false);  
                result = await SetID(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(2, false, false);
                    return result;
                }
                ChangeOneStateInvoke(2, true, false);

                
                result = await SetSerialNumber(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(3,false,false);
                    return result;
                }
                ChangeOneStateInvoke(3, true, false);

                result = await SetReading(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(4, false, false);
                    return result; 
                }
                ChangeOneStateInvoke(4, true, false);

                result = await SetWheelModel(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(5, false, false);
                    return result;
                }
                ChangeOneStateInvoke(5, true, false);

                result = await Wheel(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(6, false, false);
                    return result;
                }
                ChangeOneStateInvoke(6, true, false);

                result = await SetTouchScreen(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(7, false, false);
                    return result;
                }
                ChangeOneStateInvoke(7, true, false);

                result = await ModelSelection(cancelToken);
                if (!result)
                { 
                    ChangeOneStateInvoke(8, false, false);
                    return result;
                }
                ChangeOneStateInvoke(8, true, true);
                return result;
             }
            catch (Exception ex)
            {
                this.AddMessage("测试流程异常："+ex.ToString());
                return false;
            }
        }

        private async Task<bool> RunETRUFullTest(CancellationToken cancelToken)
        {
            try
            {
                bool result = false;
                result = await SetPassword(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(1, false, false);
                    return result;
                } 

                ChangeOneStateInvoke(1, true, false);
                result = await SetID(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(2, false, false);
                    return result;
                }
                ChangeOneStateInvoke(2, true, false); 

                result = await SetReading(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(3, false, false);
                    return result;
                }
                ChangeOneStateInvoke(3, true, false);

                result = await SetWheelModel(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(4, false, false);
                    return result;
                }
                ChangeOneStateInvoke(4, true, false);

                result = await Wheel(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(5, false, false);
                    return result;
                }
                ChangeOneStateInvoke(5, true, false);

                result = await SetTouchScreen(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(6, false, false);
                    return result;
                }
                ChangeOneStateInvoke(6, true, false);

                result = await ModelSelection(cancelToken);
                if (!result)
                {
                    ChangeOneStateInvoke(7, false, true);
                    return result;
                }
                ChangeOneStateInvoke(7, true, true);
                return result;
            }
            catch (Exception ex)
            {
                this.AddMessage("测试流程异常：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region All test steps
        //1 Set Password
        private async Task<bool> SetPassword(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName);
                myWindow.SendDataCom("P1066"+"\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "P1066"))
                {
                    this.AddMessageError("密码设置错误");
                    return false;
                }
                else 
                {
                    this.AddMessage("密码设置正确");
                    return true;
                }
             }
            catch(Exception ex)
            {
                this.AddMessage("密码设置出现异常" + ex.ToString());
                return false;
            }
        }
        //2 Set ID
        private async Task<bool> SetID(CancellationToken cancelToken)
        {
            try
            {
                myWindow.COMInit(sp.PortName);  
                myWindow.SendDataCom("I=" + this.IDNumber.Text.ToString()+"\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "ID="+this.IDNumber.Text.ToString()))
                {
                    this.AddMessageError("编号设置错误");
                    return false;
                } 
                else
                {
                    this.AddMessage("编号设置正确");
                    return true;
                }  
            }
            catch (Exception ex)
            {
                this.AddMessageError("异常"+ex.ToString());
                return false;
            }
        }
        //3 Set Serial Number
        private async Task<bool> SetSerialNumber(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName);
                myWindow.SendDataCom("S=" + this.SerialNumber.Text.ToString() + "\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "S=" + this.SerialNumber.Text.ToString()))
                {
                    this.AddMessageError("序列号设置错误");
                    return false;
                }
                else
                {
                    this.AddMessage("序列号设置正确");
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.AddMessage("序列号设置出现异常" + ex.ToString());
                return false;
            }
        }
        //4 Set Reading
        private async Task<bool> SetReading(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName); 
                Read read = new Read();
                read.ShowDialog();
                if (read.textRead.Text.Length != 7)
                {
                    MessageExt.Instance.ShowDialog("请输入正确的读数","提示");
                    return false;
                }
                myWindow.SendDataCom("R=" + read.textRead.Text.ToString() + "\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "RD=" + read.textRead.Text.ToString()))
                {
                    this.AddMessageError("读数设置失败");
                    return false;
                }
                else
                {
                    this.AddMessage("读数设置成功");
                    return true;
                } 
            }
            catch (Exception ex)
            {
                this.AddMessage("读数设置出现异常" + ex.ToString());
                return false;
            }
        }
        //5 Set Wheel Model
        private async Task<bool> SetWheelModel(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName); 
                if (comBoxSelect.SelectedIndex == 0)//表示是6
                {
                    myWindow.SendDataCom("H=0" + "\r");//0
                    await Task.Delay(1500);
                    if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "H=0"))
                    {
                        this.AddMessage(string.Format("字轮{0}模式设置错误", 6));
                        return false;
                    }
                    else
                    {
                        this.AddMessageError(string.Format("字轮{0}模式设置正确", 6));
                        return true;
                    }
                }
                if (this.comBoxSelect.SelectedIndex == 1)
                {
                    myWindow.SendDataCom("H=1" + "\r");
                    await Task.Delay(1500);
                    if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "H=1"))
                    {
                        this.AddMessage(string.Format("字轮{0}模式设置错误", 7));
                        return false;
                    }
                    else
                    {
                        this.AddMessageError(string.Format("字轮{0}模式设置正确", 7));
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.AddMessage("字轮设置出现异常" + ex.ToString());
                return false;
            }
        }
        //6 Set Wheel
        private async Task<bool> Wheel(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName);
                if (comBoxSelect.SelectedIndex==0)//表示是6
                {
                    myWindow.SendDataCom("W=6" + "\r");//0
                    await Task.Delay(1500);
                    if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "W=6"))
                    {
                        this.AddMessage(string.Format("字轮{0}模式设置错误", 6));
                        return false;
                    }
                    else
                    {
                        this.AddMessageError(string.Format("字轮{0}模式设置正确", 6));
                        return true;
                    } 
                }
                if (this.comBoxSelect.SelectedIndex == 1)
                {
                    myWindow.SendDataCom("W=7" + "\r");
                    await Task.Delay(1500);
                    if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "W=7"))
                    {
                        this.AddMessage(string.Format("字轮{0}模式设置错误", 7));
                        return false;
                    }
                    else
                    {
                        this.AddMessageError(string.Format("字轮{0}模式设置正确", 7));
                        return true;
                    }
                } 
                else
                {
                    return false;
                } 
            }
            catch (Exception ex)
            {
                this.AddMessage("字轮设置出现异常" + ex.ToString());
                return false;
            }
        }
        //7 Set Touch Screen
        private async Task<bool> SetTouchScreen(CancellationToken cancellation)
        {
            try
            {
                myWindow.COMInit(sp.PortName);
                myWindow.SendDataCom("Z?" + "\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "Z=1"))//1表示已经开启touch pad mode
                {
                    myWindow.SendDataCom("Z=1"+"\r");
                    await Task.Delay(1500); 
                    if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "Z=1"))
                    {
                        this.AddMessage("触摸屏模式开启失败");
                        return false;
                    }
                    else
                    {
                        this.AddMessage("已经开启触摸屏模式");
                        return true;
                    } 
                }
                else
                {
                    this.AddMessage("已经开启触摸屏模式"); 
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.AddMessage("触摸屏幕设置出现异常" + ex.ToString());
                return false;
            }
        }
        //8 Set Model Selection
        private async Task<bool> ModelSelection(CancellationToken cancellation)
        {
             try
            {
                myWindow.COMInit(sp.PortName);
                this.AddMessage("选择模式");
                ModelWindows model = new ModelWindows();
                model.ShowDialog(); 
                myWindow.SendDataCom("M="+model.ModelBox.Text.ToString() + "\r");
                await Task.Delay(1500);
                if (!myWindow.CheckReceiveData(myWindow.list_COM_1, "M="+model.ModelBox.Text.ToString()))
                {
                    this.AddMessageError("模式设置错误");
                    return false;
                }
                else
                {
                    this.AddMessageError("模式设置完成");
                    return true;
                }
                 
            }
            catch (Exception ex)
            {
                this.AddMessageError("模式设置异常"+ex.ToString());
                return false;
            }
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
            try
            { 
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "关闭",
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("数据处理错误"+ex.Message,"提示");
            }
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

        public bool CheckReaptID()
        {
            try
            {
                if (this.comBoxTestCourse.SelectedIndex == 0)
                {
                    if (!MyDatabaseMethod.SelectETRUIDTested((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 0, this.IDNumber.Text.Trim().ToString()))
                    {
                        MessageExt.Instance.ShowDialog("产品编号重复", "提示");
                        return false;
                    }
                    else
                    {
                        this.AddMessage("产品编号没有重复可以继续测试");
                        return true;
                    }
                }
                if (this.comBoxTestCourse.SelectedIndex == 1)
                {
                    if (!MyDatabaseMethod.SelectETRUIDTested((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 1, this.IDNumber.Text.Trim().ToString()))
                    {
                        this.AddMessage("产品编号重复");
                        return false;
                    }
                    else
                    {
                        this.AddMessage("产品编号没有重复可以继续测试");
                        return true;
                    }
                }
                else
                {
                    MessageExt.Instance.ShowDialog("异常", "提示");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("错误信息" + ex.ToString(), "提示");
                return false;
            }
        }

        public bool CheckInput()
        {
            bool result = false;
            if (this.IDNumber == null || this.IDNumber.Text.Length!=8)
            {
                return result;
            }
            if (this.orderNumber == null || this.orderNumber.Text.Length != 8)
            {
                return result;
            }
            if (this.comBoxTestCourse.SelectedIndex == 0)
            {
                if (this.SerialNumber == null || this.SerialNumber.Text.Length != 8)
                {
                    return result;
                }
            }
            return true;
        }

        public async Task<bool> WriteTestResultIntoDatabaseAsync()
        {
            //write test result into database 
            try
            {
                if (!MyDatabaseMethod.InsertIntoAccess((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 0, myETRUMoudleTest, myETRUFullTest))
                {
                    MessageExt.Instance.ShowDialog("数据写入数据库失败", "提示");
                    PublicClass.IsRunning = false;
                    await Task.Delay(50);
                    ChangeOneStateInvoke(8, false, false);
                    return false;
                }
                else
                {
                    if (myETRUMoudleTest.EndState == "Pass")
                    {
                        this.AddMessage("数据写入数据库成功，测试完成");
                        return true;
                    }
                    else
                    {
                        this.AddMessage("写入数据库成功,测试失败");
                        PublicClass.IsRunning = false;
                        await Task.Delay(50);
                        ChangeOneStateInvoke(1, false, false);
                        return false; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("数据写入异常" + ex.Message, "提示");
                ChangeOneStateInvoke(1, false, false);
                return false;
            }
        }

        public async Task<bool> WriteTestResultIntoDatabaseFullAsync()
        {
            //write test result into database 
            try
            {
                if (!MyDatabaseMethod.InsertIntoAccess((int)Enum.Parse(typeof(AllDatabase), PublicClass.CurrentDatabase), 1, myETRUMoudleTest, myETRUFullTest))
                {
                    MessageExt.Instance.ShowDialog("数据写入数据库失败", "提示");
                    PublicClass.IsRunning = false;
                    await Task.Delay(50);
                    ChangeOneStateInvoke(1, false, false);
                    return false;
                }
                else
                {
                    if (myETRUMoudleTest.EndState == "Pass")
                    {
                        this.AddMessage("数据写入数据库成功，测试完成");
                        return true;
                    }
                    else
                    {
                        MessageExt.Instance.ShowDialog("写入数据库成功", "测试失败");
                        PublicClass.IsRunning = false;
                        await Task.Delay(50);
                        ChangeOneStateInvoke(1, false, false);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageExt.Instance.ShowDialog("数据写入异常" + ex.ToString(), "提示");
                ChangeOneStateInvoke(1, false, false);
                return false;
            }
        }
        #endregion

        #region Datagrid Notify to change the status
        private void ChangeOneStateInvoke(int line, bool bo, bool IsMaxLine)
        {
            try
            {
                DataGridDelegate DGD = new DataGridDelegate(ChangeOneState);
                DGD(line, bo, IsMaxLine);
            }
            catch (Exception ex)
            {
                this.AddMessageError("出现异常" + ex.ToString()); 
            }
        }
        private void ChangeOneState(int line, bool bo, bool IsMaxLine)//更改datagridview上的一个状态
        {
            try
            { 
                line =line-1;
                if (bo == true)
                {
                    (DataGridTestCourse.Columns[2].GetCellContent(DataGridTestCourse.Items[line]) as TextBlock).Text = "Pass";
                    (DataGridTestCourse.ItemContainerGenerator.ContainerFromItem(DataGridTestCourse.Items[line]) as DataGridRow).Background = System.Windows.Media.Brushes.LightGreen;
                    if (!IsMaxLine)
                    {
                        (DataGridTestCourse.ItemContainerGenerator.ContainerFromItem(DataGridTestCourse.Items[line + 1]) as DataGridRow).Background = System.Windows.Media.Brushes.Yellow;
                    }
                }
                else
                {
                    (DataGridTestCourse.Columns[2].GetCellContent(DataGridTestCourse.Items[line]) as TextBlock).Text = "Fail";
                    (DataGridTestCourse.ItemContainerGenerator.ContainerFromItem(DataGridTestCourse.Items[line]) as DataGridRow).Background = System.Windows.Media.Brushes.Red;
                }
            }
            catch(Exception ex)
            {
                this.AddMessageError("出现异常"+ex.ToString());
            }
        }
        #endregion

        #region Threading
         async Task<bool> RunCommandAsync(RunCommands theCommand)
        {
            try
            {
                bool result = false;
                switch (theCommand)
                {
                    case RunCommands.RunEtruFull:
                       result= await this.RunETRUFullTest(cancelTokenSource.Token);  
                        break;
                     case RunCommands.RunETRUModel:
                       result=await this.RunETRUMoudleTest(cancelTokenSource.Token);
                        break;
                }
                return result;
            }
            catch(OperationCanceledException)
            {
                return false;
            }
        }

        #endregion

        #region Foreach
         public void RunTestPoint()
        {
             

        }
       
        #endregion

        #region Test time
        private void TestTimeTimer_Tick(object sender, EventArgs e)
        {
            long Tick = 10000000;
            long DifferenzInTicks;
            long Rest;
            PruefzeitTicksJetzt = DateTime.Now.Ticks;
            DifferenzInTicks = PruefzeitTicksJetzt - PruefzeitTicksStart;
            Rest = DifferenzInTicks % Tick;
            this.PruefzeitInSekunden = (long)(DifferenzInTicks - Rest) / Tick;
            this.TextBoxTesttime.Text = MinutenUndSekunden(this.PruefzeitInSekunden); 
        }
        private string MinutenUndSekunden(long Sekunden)
        {
            long Minuten;
            long Rest;
            Rest = Sekunden % 60;
            Minuten = (Sekunden - Rest) / 60;
            return Minuten.ToString("00") + ":" + Rest.ToString("00");
        }

        #endregion

    }

}
