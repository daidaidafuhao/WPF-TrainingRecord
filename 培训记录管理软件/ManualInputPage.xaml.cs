using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using 培训记录管理软件;

namespace TrainingRecordManager
{
    public partial class ManualInputPage : MetroWindow
    {
        public ObservableCollection<TrainingRecord> TrainingRecords { get; set; }

        private bool isVisible = false;

        private string selectedImagePath;

        DatabaseManager dbManager = new DatabaseManager();

        private String OldIDCardNumber = "";

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
        }



        public ManualInputPage()
        {
            InitializeComponent();
            TrainingRecords = new ObservableCollection<TrainingRecord>();
            TrainingRecord.ItemsSource = TrainingRecords;
            EditEmployeeButton.Visibility = Visibility.Collapsed;
            TrainingRecords.Clear();
            GoToHomePageButton.Content = "返回主页";
        }

        public ManualInputPage(string id)
        {
            InitializeComponent();
            TrainingRecords = new ObservableCollection<TrainingRecord>();
            TrainingRecord.ItemsSource = TrainingRecords;

            TrainingRecords.Clear();
            HideControlsAndDisableInput(id);
            GoToHomePageButton.Content = "返回一览";
        }

        private void NameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        
        }

        private async void IdNumberTextBox_PreviewTextInput(object sender, KeyEventArgs e)
        {
            if (IdNumberTextBox.Text.Length == 18&& OldIDCardNumber=="")
            {
                // 如果输入完成就查询是否有培训记录
                
                List<TrainingRecord> Records =  await dbManager.GetTrainingRecordsByEmployeeId(IdNumberTextBox.Text);
                if (Records.Count > 0) {
                    HideControlsAndDisableInput(IdNumberTextBox.Text);
                    GoToHomePageButton.Visibility = Visibility.Visible;
                }
               
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            AddEmployee();
        }

        private void GoToHomePage_Click(object sender, RoutedEventArgs e)
        {
            if (GoToHomePageButton.Content.ToString().Equals( "返回主页"))
            {
                var homePage = new HomePage();
                homePage.Show();
                this.Close(); // 关闭当前窗口（可选）

            }
            else if(GoToHomePageButton.Content.ToString().Equals("返回一览"))
            {
                // 跳转到查看一览页面
                var summaryPage = new SummaryPage(); // 确保您已创建该页面
                summaryPage.Show();
                this.Close(); // 关闭当前页面（可选）
            }

            
        }

        private async void AddTrainingRecord_Click(object sender, RoutedEventArgs e)
        {
            if (IdNumberTextBox.Text.Length == 18)
            {
                List < TrainingRecord > trainingRecords = new List<TrainingRecord>();
                foreach (TrainingRecord trainingRecord in TrainingRecords)
                {
                    trainingRecord.EmployeeId = IdNumberTextBox.Text;
                    trainingRecords.Add(trainingRecord);
                }
                try
                {
                    TrainingRecords.Clear();
                    List<TrainingRecord> Records = await dbManager.InsertTrainingRecordOrUpdate(trainingRecords);
                    foreach (TrainingRecord record in Records)
                    {
                        TrainingRecords.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    ShowPopu("发生致命错误消息:" + ex.Message);
                }
                
            }
            else
            {
                ShowPopu("身份证格式不对，请检查是否满18位");
            }            
        }

        private void AddNewRow_Click(object sender, RoutedEventArgs e)
        {

            var newTrainingRecord = new TrainingRecord
            {
                Id = 0, // 或其他默认值
                SerialNumber = 0, // 假设有一个方法来生成序号
                TrainingDate = DateTime.Now,  // 设置为今天的日期
                TrainingContent = "",
                TrainingUnit = "",
                TrainingLocation = "",
                Assessment = "",
                Cost = 0,
                Remarks = ""
            };
            // 添加一行新的空数据
            TrainingRecords.Add(newTrainingRecord);
        }


        private async void SeletTable_Click(object sender, RoutedEventArgs e)
        {
            if (IdNumberTextBox.Text.Length == 18)
            {
                TrainingRecords.Clear();
                List<TrainingRecord> Records = await dbManager.GetTrainingRecordsByEmployeeId(IdNumberTextBox.Text);
                foreach (TrainingRecord record in Records)
                {
                    TrainingRecords.Add(record);
                }
            }
            else
            {
                // 抛出异常
                ShowPopu("身份证格式不对，请检查是否满18位");
            }
        }

        private void AddEmployee()
        {
          

            // 获取用户输入
            var name = NameTextBox.Text;
            var idCardNumber = IdNumberTextBox.Text;
            var education = EducationTextBox.Text;
            var title = TitleTextBox.Text;
            var Level = LevelTextBox.Text;
            var UnitName = UnitNameTextBox.Text;
            var RuzhiDate = RuzhiDateTextBox.Text;
            var ZhuanYe = ZhuanYeTextBox.Text;
            var LevelJobType= LevelJobTypeTextBox.Text;

            // 验证输入
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowPopu("姓名不能为空。");
                return;
            }
            if (!IsValidName(name))
            {
                ShowPopu("姓名只能是英文或汉字，且不超过50个字。");
                return;
            }

            if (string.IsNullOrWhiteSpace(idCardNumber))
            {
                ShowPopu("身份证号不能为空。");
                return;
            }
            if (!IsValidIDCardNumber(idCardNumber))
            {
                ShowPopu("身份证号必须是18位，并且只能是数字。");
                return;
            }

            if (education.Length > 50)
            {
                ShowPopu("学历不能超过50个字。");
                return;
            }

            if ( title.Length > 50)
            {
                ShowPopu("职称不能超过50个字。");
                return;
            }

            DateTime date;
            string[] formats = { "yyyy-MM-dd", "MM/dd/yyyy", "yyyy/MM/dd" }; // 你可以根据需要增加更多的格式
            if (RuzhiDate.Equals("") || !DateTime.TryParseExact(RuzhiDate, formats, null, System.Globalization.DateTimeStyles.None, out date))
            {
                ShowPopu("时间不能为且空要注意格式");
                return;
            }

            try
            {
                var record = new Employee
                {
                    Name = name,
                    IDCardNumber = idCardNumber,
                    Photo = "无", // 如果有上传照片的逻辑，可以替换
                    Education = education,
                    Title = title,
                    LevelJobType = LevelJobType, // 根据需要修改
                    UnitName = UnitName,
                    Level = Level,
                    RuzhiDate = DateTime.Parse(RuzhiDate),
                    SchoolName = education,
                    ZhuanYe = ZhuanYe,
                };
                if (OldIDCardNumber == "") {
                    // 插入新员工记录
                    dbManager.InsertEmployee(record);
                }
                TrainingRecords.Clear();
                HideControlsAndDisableInput(IdNumberTextBox.Text);
                GoToHomePageButton.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                ShowPopu("发生致命错误消息:" + ex.Message);
            }
        }

        private bool IsValidName(string name)
        {
            // 验证姓名只能包含汉字和英文字母，并且不超过50个字
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[\u4e00-\u9fa5a-zA-Z]{1,50}$");
        }

        private bool IsValidIDCardNumber(string idCardNumber)
        {
            // 验证身份证号是否为18位数字
            return idCardNumber.Length == 18 && System.Text.RegularExpressions.Regex.IsMatch(idCardNumber, @"^\d{17}[\dX]$");
        }
        private bool isEditing = false; // 用于跟踪是否处于编辑模式

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (isEditing)
                {
                  
                    // 创建一个新的Employee对象，更新员工的值
                    DateTime updatedRuzhiDate = DateTime.TryParse(RuzhiDateTextBox.Text, out DateTime parsedDate) ? parsedDate : DateTime.MinValue;
                    // 获取用户输入
                    var name = NameTextBox.Text;
                    var idCardNumber = IdNumberTextBox.Text;
                    var education = EducationTextBox.Text;
                    var title = TitleTextBox.Text;
                    var Level = LevelTextBox.Text;
                    var UnitName = UnitNameTextBox.Text;
                    var RuzhiDate = RuzhiDateTextBox.Text;
                    var ZhuanYe = ZhuanYeTextBox.Text;
                    var LevelJobType = LevelJobTypeTextBox.Text;


                    // 验证输入
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        ShowPopu("姓名不能为空。");
                        return;
                    }
                    if (!IsValidName(name))
                    {
                        ShowPopu("姓名只能是英文或汉字，且不超过50个字。");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(idCardNumber))
                    {
                        ShowPopu("身份证号不能为空。");
                        return;
                    }
                    if (!IsValidIDCardNumber(idCardNumber))
                    {
                        ShowPopu("身份证号必须是18位，并且只能是数字。");
                        return;
                    }

                    if ( education.Length > 50)
                    {
                        ShowPopu("学历不能超过50个字。");
                        return;
                    }

                    if (title.Length > 50)
                    {
                        ShowPopu("职称能超过50个字。");
                        return;
                    }
                    DateTime date;
                    string[] formats = { "yyyy-MM-dd", "MM/dd/yyyy", "yyyy/MM/dd",}; // 你可以根据需要增加更多的格式
                    if (RuzhiDate.Equals("") || !DateTime.TryParseExact(RuzhiDate, formats, null, System.Globalization.DateTimeStyles.None, out date))
                    {
                        ShowPopu("时间不能为且空要注意格式");
                        return;
                    }

                    Employee updatedEmployee = new Employee
                    {
                        Name = NameTextBox.Text,
                        IDCardNumber = IdNumberTextBox.Text,
                        Title = TitleTextBox.Text,
                        Level = LevelTextBox.Text,
                        UnitName = UnitNameTextBox.Text,
                        SchoolName = EducationTextBox.Text,
                        RuzhiDate = updatedRuzhiDate,
                        ZhuanYe = ZhuanYeTextBox.Text,
                        Education = EducationTextBox.Text,
                        LevelJobType= LevelJobTypeTextBox.Text,
                    };
                    // 调用数据库管理器更新员工及其培训记录
                    dbManager.UpdateEmployeeAndTrainingRecord(updatedEmployee, OldIDCardNumber);
                    OldIDCardNumber = updatedEmployee.IDCardNumber;
                    // 将按钮文本设置为 "编辑人员"
                    EditEmployeeButton.Content = "编辑人员";
                    // 执行保存操作
                    ShowControlsAndEnableInput(false);
                }
                else
                {
                    // 进入编辑模式
                    ShowControlsAndEnableInput(true);

                    // 将按钮文本设置为 "确认保存"
                    EditEmployeeButton.Content = "确认保存";
                }

                // 切换编辑状态
                isEditing = !isEditing;
            }
            catch (Exception ex)
            {
                ShowPopu("导入失败，已回滚数据：" + ex.Message);
            }
            
        }




        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            ImagePage popup = new ImagePage(IdNumberTextBox.Text);
            popup.Owner = this; // 设置弹出框的拥有者为主窗口
            popup.ShowDialog(); // 显示模态对话框
        }

        private void ManualInput_Click(object sender, RoutedEventArgs e)
        {
            isVisible = !isVisible;
            ManualInputPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            SeletTableButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            AddTrainingRecordButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            // 跳转到文件导入页面
            var importPage = new InputExecl(); // 确保您已创建该页面
            importPage.Show();
            this.Close(); // 关闭当前页面（可选）
        }



        private void ShowPopu(string errorMessage)
        {
            PopupWindow popup = new PopupWindow(errorMessage);
            popup.Owner = this; // 设置弹出框的拥有者为主窗口
            popup.ShowDialog(); // 显示模态对话框
        }

        // 从一览进入
        private async void HideControlsAndDisableInput(string IdNumber)
        {

            List<Employee> employees =  await dbManager.GetEmployees(IdNumber);
            // 隐藏按钮
            ImportFileButton.Visibility = Visibility.Collapsed;
            ManualInputButton.Visibility = Visibility.Collapsed;
            SeletTableButton.Visibility = Visibility.Collapsed;
           
            AddEmployeeButton.Visibility = Visibility.Collapsed;
            // 显示 ManualInputPanel，并禁用其中的控件
            ManualInputPanel.Visibility = Visibility.Visible;
            AddTrainingRecordButton.Visibility = Visibility.Visible;

            if (employees .Count> 0) {
                // 这里可以根据需要为 ManualInputPanel 中的控件赋值
                NameTextBox.Text = employees[0].Name; // 示例值
                IdNumberTextBox.Text = employees[0].IDCardNumber; // 示例值
                EducationTextBox.Text = employees[0].Education; // 示例值
                TitleTextBox.Text = employees[0].Title; // 示例值
                LevelTextBox.Text = employees[0].Level; // 示例值
                RuzhiDateTextBox.Text = employees[0].RuzhiDate.ToString("yyyy/MM/dd"); // 示例值
                ZhuanYeTextBox.Text = employees[0].ZhuanYe; // 示例值
                UnitNameTextBox.Text = employees[0].UnitName;
                OldIDCardNumber = IdNumberTextBox.Text;
                LevelJobTypeTextBox.Text= employees[0].LevelJobType;

            }

            // 禁用 ManualInputPanel 中的文本框和按钮
            NameTextBox.IsReadOnly = true; // 设置为只读
            IdNumberTextBox.IsReadOnly = true; // 设置为只读
            EducationTextBox.IsReadOnly = true; // 设置为只读
            TitleTextBox.IsReadOnly = true; // 设置为只读
            LevelTextBox.IsReadOnly = true; // 设置为只读
            RuzhiDateTextBox.IsReadOnly = true; // 设置为只读
            ZhuanYeTextBox.IsReadOnly = true; // 设置为只读
            LevelJobTypeTextBox.IsReadOnly = true; // 设置为只读
            UnitNameTextBox.IsReadOnly = true;

            NameTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            IdNumberTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            EducationTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            TitleTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            LevelTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            RuzhiDateTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            ZhuanYeTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            LevelJobTypeTextBox.Background = Brushes.LightGray; // 设置为灰色背景
            UnitNameTextBox.Background = Brushes.LightGray;// 设置为灰色背景

            //显示培训记录
            TrainingRecords.Clear();
            List<TrainingRecord> Records = await dbManager.GetTrainingRecordsByEmployeeId(IdNumberTextBox.Text);
            foreach (TrainingRecord record in Records)
            {
                TrainingRecords.Add(record);
            }
        }

        private void ShowControlsAndEnableInput(bool isShowu)
        {

            if (isShowu)
            {// 启用 ManualInputPanel 中的文本框和按钮
                NameTextBox.IsReadOnly = false; // 设置为可编辑
                IdNumberTextBox.IsReadOnly = false; // 设置为可编辑
                EducationTextBox.IsReadOnly = false; // 设置为可编辑
                TitleTextBox.IsReadOnly = false; // 设置为可编辑
                LevelTextBox.IsReadOnly = false; // 设置为可编辑
                RuzhiDateTextBox.IsReadOnly = false; // 设置为可编辑
                ZhuanYeTextBox.IsReadOnly = false; // 设置为可编辑
                LevelJobTypeTextBox.IsReadOnly = false; // 设置为可编辑
                UnitNameTextBox.IsReadOnly = false; // 设置为可编辑

                NameTextBox.Background = Brushes.White; // 设置为默认背景
                IdNumberTextBox.Background = Brushes.White; // 设置为默认背景
                EducationTextBox.Background = Brushes.White; // 设置为默认背景
                TitleTextBox.Background = Brushes.White; // 设置为默认背景
                LevelTextBox.Background = Brushes.White; // 设置为默认背景
                RuzhiDateTextBox.Background = Brushes.White; // 设置为默认背景
                ZhuanYeTextBox.Background = Brushes.White; // 设置为默认背景
                LevelJobTypeTextBox.Background = Brushes.White; // 设置为默认背景
                UnitNameTextBox.Background = Brushes.White; // 设置为默认背景


            }
            else{
                // 禁用 ManualInputPanel 中的文本框和按钮
                NameTextBox.IsReadOnly = true; // 设置为只读
                IdNumberTextBox.IsReadOnly = true; // 设置为只读
                EducationTextBox.IsReadOnly = true; // 设置为只读
                TitleTextBox.IsReadOnly = true; // 设置为只读
                LevelTextBox.IsReadOnly = true; // 设置为只读
                RuzhiDateTextBox.IsReadOnly = true; // 设置为只读
                ZhuanYeTextBox.IsReadOnly = true; // 设置为只读
                LevelJobTypeTextBox.IsReadOnly = true; // 设置为只读
                UnitNameTextBox.IsReadOnly = true;

                NameTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                IdNumberTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                EducationTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                TitleTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                LevelTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                RuzhiDateTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                ZhuanYeTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                LevelJobTypeTextBox.Background = Brushes.LightGray; // 设置为灰色背景
                UnitNameTextBox.Background = Brushes.LightGray;// 设置为灰色背景
            }
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            string data = button.Tag.ToString(); // 获取身份证号

            // 第一次确认删除
            if (MessageBox.Show($"确定删除：{data}？", "确认删除", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            // 第二次确认是否继续删除
            if (MessageBox.Show("删除操作不可恢复，是否继续？", "再次确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                dbManager.DeleteTrainingRecordBySerialNumber(data, IdNumberTextBox.Text);
                MessageBox.Show($"删除成功：{data}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}");
            }

            //显示培训记录
            TrainingRecords.Clear();
            List<TrainingRecord> Records = await dbManager.GetTrainingRecordsByEmployeeId(IdNumberTextBox.Text);
            foreach (TrainingRecord record in Records)
            {
                TrainingRecords.Add(record);
            }

        }


    }






}
