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
    /// ModelWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ModelWindows : MetroWindow
    {
        public enum Model:int
        {
            _0= 0,
            _1= 1,
             _2=2,
             _3=3
        }


        public ModelWindows()
        {
            InitializeComponent();
            List<string> ModelSelect = new List<string>();//定义集合
            foreach (var x in Enum.GetValues(typeof(Model)))//使用遍历从枚举中获取数据
            {
                SelectList.Items.Add(x);
            } 
         }

        private void ModelBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ModelBox.Text.ToString() == null || ModelBox.Text.ToString().Length == 0)
                {
                    MessageBox.Show("请输入正确的模式");
                    return;
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                return;
            }
        } 

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.ModelBox.Text ="0";
        }

        private void Button_Click_0(object sender, RoutedEventArgs e)
        { 
            this.ModelBox.Text = "1";
        }

        private void SelectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.SelectList.SelectedIndex)
            {
                case 0:
                    Select("0");
                    break;
                case 1:
                    Select("1");
                    break;
                case 2:
                    Select("2");
                    break;
                case 3:
                    Select("3");
                    break;
                default:
                    MessageBox.Show("请选择正确的模式");
                    break;
            }
                 
        } 
        public void Select(string str)
        {
            this.ModelBox.Text = str; 
        }
    }
}
