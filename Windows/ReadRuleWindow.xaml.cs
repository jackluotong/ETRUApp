using MahApps.Metro.Controls;
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

namespace ETRU_TestBench.Windows
{
    /// <summary>
    /// ReadRuleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ReadRuleWindow : MetroWindow
    {
        public ReadRuleWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(10);
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.Source = new Uri(@"C:\Work\ETRU\ETRU_TestBench\ETRU_TestBench\ReadRule.html");
            this.Content = webBrowser;
        }
    }
}
