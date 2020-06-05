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
    /// SearialWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearialWindow : Window
    {
        public SearialWindow()
        {
            InitializeComponent();
            
        }

        private void Read_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}
