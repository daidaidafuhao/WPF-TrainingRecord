using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace TrainingRecordManager
{
    public partial class QueryPage : MetroWindow
    {
        DatabaseManager databaseManager = new DatabaseManager();
        // 声明关闭事件
        public event EventHandler PageClosed;

        public QueryPage(string page)
        {
            InitializeComponent();
            if (page.Equals("SummaryPage") ){
                GoToHomePage.Visibility = Visibility.Collapsed;
            }
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取屏幕的工作区域
            var screen = SystemParameters.WorkArea;

            // 计算窗口的位置
            double left = (screen.Width - this.Width) / 2 + screen.Left;
            double top = (screen.Height - this.Height) / 2 + screen.Top;

            // 设置窗口的位置
            this.Left = left;
            this.Top = top;
            LoadComboBoxData();
        }
      
        // 加载数据到下拉框
        private void LoadComboBoxData()
        {
            if( TokenManager.Instance.GetRole()==null){
                // 单位名称下拉框  
                databaseManager.LoadComboBoxItems("SELECT DISTINCT UnitName FROM Employee", SearchUnitName);
            }else{
                  SearchUnitName.Items.Add(TokenManager.Instance.GetRole()); 
                  SearchUnitName.SelectedItem = TokenManager.Instance.GetRole();
                  SearchUnitName.IsEnabled = false;  // 设置为不可编辑
            }
            // 姓名下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT Name FROM Employee", SearchName);
            // 身份证号下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT IdCardNumber FROM Employee", SearchIdCardNumber);
            // 职位下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT Title FROM Employee", SearchTitle);
            // 级别下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT Level FROM Employee", SearchLevel);
            
            // 学校名称下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT SchoolName FROM Employee", SearchSchoolName);
            // 专业下拉框  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT ZhuanYe FROM Employee", SearchZhuanYe);
            // 工种  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT LevelJobType FROM Employee", LevelJobType);
            // 导入时间  
            databaseManager.LoadComboBoxItems("SELECT DISTINCT ImportTime FROM ImportHistory", ImportTime);
        }

        private void GoToHomePage_Click(object sender, RoutedEventArgs e)
        {
            var homePage = new HomePage();
            homePage.Show();
            this.Close(); // 关闭当前窗口（可选）
        }

        // 处理查询按钮点击事件
        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取查询条件
            string name = SearchName.Text;
            string idCardNumber = SearchIdCardNumber.Text;
            string title = SearchTitle.Text;
            string level = SearchLevel.Text;
            string unitName = SearchUnitName.Text;
            DateTime? ruzhiDateStart = RuzhiDateStart.SelectedDate;
            DateTime? ruzhiDateEnd = RuzhiDateEnd.SelectedDate;
            string schoolName = SearchSchoolName.Text;
            string zhuanYe = SearchZhuanYe.Text;
            string levelJobType = LevelJobType.Text;

            // 创建 Employee 对象并赋值
            var employeeFilter = new Employee
            {
                Name = string.IsNullOrWhiteSpace(name) ? null : name,
                IDCardNumber = string.IsNullOrWhiteSpace(idCardNumber) ? null : idCardNumber,
                Title = string.IsNullOrWhiteSpace(title) ? null : title,
                Level = string.IsNullOrWhiteSpace(level) ? null : level,
                UnitName = string.IsNullOrWhiteSpace(unitName) ? null : unitName,
                RuzhiDateStart = ruzhiDateStart ?? DateTime.MinValue, // 如果为空，用 DateTime.MinValue 代表未设置
                RuzhiDateEnd = ruzhiDateEnd ?? DateTime.MinValue, // 如果为空，用 DateTime.MinValue 代表未设置
                SchoolName = string.IsNullOrWhiteSpace(schoolName) ? null : schoolName,
                ZhuanYe = string.IsNullOrWhiteSpace(zhuanYe) ? null : zhuanYe,
                LevelJobType = string.IsNullOrWhiteSpace(levelJobType) ? null : levelJobType
            };
            // 跳转到查看一览页面
            var summaryPage = new SummaryPage(employeeFilter, string.IsNullOrWhiteSpace(ImportTime.Text) ? null : ImportTime.Text); // 确保您已创建该页面
            summaryPage.Show();
            this.Close(); // 关闭当前页面（可选）
            // 触发 PageClosed 事件
            PageClosed?.Invoke(this, EventArgs.Empty);

        }

    }
}
