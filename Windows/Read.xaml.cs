using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ETRU_TestBench.Windows
{
    /// <summary>
    /// Read.xaml 的交互逻辑
    /// </summary>
    /// 
    
    public partial class Read : MetroWindow
    {
        public Read()
        {
            InitializeComponent();
            MessageExt.Instance.ShowDialog = ShowDialog;
            //this.Time.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            System.Windows.Threading.DispatcherTimer dispatcher = new System.Windows.Threading.DispatcherTimer();
            dispatcher.Tick += new EventHandler(dispatcherTimer_Tick); 
            dispatcher.Interval = new TimeSpan(0,0,1);
            dispatcher.Start();
         }
        private void TextRead_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            { 
                if (this.textRead.Text.ToString().Length !=7 || this.textRead.Text.ToString() == null)
                {
                     MessageExt.Instance.ShowDialog("请输入正确的读数","提示");
                    return;
                }
                else
                {
                    this.Close(); 
                }
            }
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            ReadRuleWindow ruleWindow = new ReadRuleWindow();
            ruleWindow.ShowDialog();  
        }

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
                MessageExt.Instance.ShowDialog("数据处理错误" + ex.Message, "提示");
            }
        }

        private void Date_Click(object sender, RoutedEventArgs e)
        {
            MessageExt.Instance.ShowDialog("此模块等待开发","提示");
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
