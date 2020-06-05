using System;
using System.Windows;
using System.Windows.Controls;

namespace ETRU_TestBench.Windows
{
    /// <summary>
    /// Operation.xaml 的交互逻辑
    /// </summary>
    public partial class Operation : Window
    {
        public Operation()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WebBrowser webBrowser = new WebBrowser(); //在窗口中打开网页
            webBrowser.Source = new Uri(@"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\Explain.html");
            this.Content = webBrowser;

            //System.Diagnostics.Process proc = new System.Diagnostics.Process();//在浏览器中打开网页
            //proc.StartInfo.FileName = @"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\02_19.html";
            //proc.Start();
        }
    }
}
