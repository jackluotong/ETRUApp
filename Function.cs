using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;

namespace ETRU_TestBench
{
    class Function
    {
        // private MainWindow myWindow;
        MainWindow myWindow = new MainWindow();
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
            MessageDialogResult result = await this.myWindow.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
        }

        public async void ShowYesNo(string message, string title, Action action)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "确定",
                NegativeButtonText = "取消",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await this.myWindow.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, mySettings);
            if (result == MessageDialogResult.Affirmative)
            {
                await Task.Factory.StartNew(action);
            }
        }

        //MessageExt.Instance.ShowYesNo("查询", "提示", new Action(() => {
        //    MessageBox.Show("我来了");
        //}));                                 
        #endregion
    }
}
