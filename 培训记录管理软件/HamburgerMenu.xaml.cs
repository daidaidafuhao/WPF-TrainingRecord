using System;
using System.Collections.Generic;
using System.Configuration;
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
using TrainingRecordManager;

namespace 培训记录管理软件
{
    /// <summary>
    /// HamburgerMenu.xaml 的交互逻辑
    /// </summary>
    public partial class HamburgerMenu : Window
    {
        public HamburgerMenu()
        {
            InitializeComponent();
            // 默认加载主页
            MainContent.Content = new HomePage();
        }

        private void HamburgerMenu_ItemClick(object sender, MahApps.Metro.Controls.HamburgerMenuItemInvokedEventArgs args)
        {
            // 根据 Tag 加载对应的页面
            string tag = (args.InvokedItem as MahApps.Metro.Controls.HamburgerMenuIconItem)?.Tag.ToString();
            switch (tag)
            {
                case "HomePage":
                    MainContent.Content = new HomePage();
                    break;
                case "SettingsPage":
                    MainContent.Content = new SummaryPage();
                    break;
                case "AboutPage":
                    MainContent.Content = new InputExecl();
                    break;
            }
        }
    }
}
