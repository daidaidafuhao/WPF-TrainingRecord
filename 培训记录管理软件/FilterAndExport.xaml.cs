using MahApps.Metro.Controls;
using System;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using 培训记录管理软件;

namespace TrainingRecordManager
{
    public partial class FilterAndExport : MetroWindow
    {

        private List<TrainingRecord> _trainingRecords; // 模拟数据库的记录列表
        DatabaseManager dbManager = new DatabaseManager();
        public FilterAndExport()
        {
            InitializeComponent();
            // 加载培训记录（实际应用中从数据库获取）
                        // 模拟数据库数据
            _trainingRecords= dbManager.GetALLTrainingRecords();
            LoadTrainingRecords(_trainingRecords);
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
        }


        private void LoadTrainingRecords(List<TrainingRecord> trainingRecords)
        {


            // 按培训内容、单位和地点分组统计
            var summary = trainingRecords
                .GroupBy(r => new { r.TrainingContent, r.TrainingUnit, r.TrainingLocation })
                .Select(g => new
                {
                    TrainingContent = g.Key.TrainingContent,
                    TrainingUnit = g.Key.TrainingUnit,
                    TrainingLocation = g.Key.TrainingLocation,
                    Count = g.Count()
                })
                .ToList();

            // 绑定到 DataGrid
            TrainingSummaryGrid.ItemsSource = summary;
        }


        private void GoSummaryPage_Click(object sender, RoutedEventArgs e)
        {

            var SummaryPage = new SummaryPage();
            SummaryPage.Show();
            this.Close();
        }

        // 查询按钮点击事件
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取查询条件
            string contentFilter = SearchTrainingContent.Text?.Trim();
            string unitFilter = SearchTrainingUnit.Text?.Trim();
            string locationFilter = SearchTrainingLocation.Text?.Trim();
        
                // 过滤数据（假设 trainingRecords 是所有记录的列表）
                var filteredRecords = _trainingRecords.Where(record =>
                    (string.IsNullOrEmpty(contentFilter) || record.TrainingContent.Contains(contentFilter)) &&
                    (string.IsNullOrEmpty(unitFilter) || record.TrainingUnit.Contains(unitFilter)) &&
                    (string.IsNullOrEmpty(locationFilter) || record.TrainingLocation.Contains(locationFilter))
                ).ToList();

                // 更新 DataGrid 数据源
                LoadTrainingRecords(filteredRecords);
        }

        // 详情按钮点击事件
        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null)
            {
                var record = button.Tag;

                // 使用反射访问匿名类型的属性
                var trainingContent = record.GetType().GetProperty("TrainingContent")?.GetValue(record)?.ToString();
                var trainingUnit = record.GetType().GetProperty("TrainingUnit")?.GetValue(record)?.ToString();
                var trainingLocation = record.GetType().GetProperty("TrainingLocation")?.GetValue(record)?.ToString();
                var count = record.GetType().GetProperty("Count")?.GetValue(record)?.ToString();
                List<TrainingRecord> trainingRecords =  dbManager.GetTrainingRecordsByCriteria(trainingContent, trainingUnit, trainingLocation);
                List < EmployeeInfo > employeefos = new List<EmployeeInfo>();
                foreach (var trainingRecord in trainingRecords) {
                    List<Employee> employees = dbManager.GetEmployees(trainingRecord.EmployeeId);
                    EmployeeInfo employeefo = new EmployeeInfo
                    {
                        Name = employees[0].Name,
                        IDCardNumber = employees[0].IDCardNumber,
                        Education = employees[0].Education,
                        Title = employees[0].Title,
                        Position = employees[0].Position,
                        TrainingRecords = trainingRecords.Where(record =>      
                        (string.IsNullOrEmpty(employees[0].IDCardNumber) 
                        || record.EmployeeId.Contains(employees[0].IDCardNumber))
                        ).ToList(),
                    };

                    employeefos.Add(employeefo); 
                }

                    // 创建并显示文件夹选择对话框
                    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        dialog.Description = "请选择导出文件的目标文件夹";
                        dialog.ShowNewFolderButton = true;

                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            // 获取用户选择的文件夹路径
                            string selectedPath = dialog.SelectedPath;
                            var buttonPath = sender as Button;
                            if (buttonPath != null)
                            {
                                Program.outRecordsDocx(selectedPath, employeefos);
                                ShowPopu("已经导出");
                            }
                        }
                        else
                        {
                            ShowPopu("操作已取消");
                        }
                    }

              
            }
            }
                catch (Exception ex)
                {
                ShowPopu("发生致命错误消息:" + ex.Message);
            }
        }

        private void ShowPopu(string errorMessage)
        {
            PopupWindow popup = new PopupWindow(errorMessage);
            popup.Owner = this; // 设置弹出框的拥有者为主窗口
            popup.ShowDialog(); // 显示模态对话框
        }
    }


    }

    public class TrainingGrid
    {
        public string TrainingContent { get; set; }
        public string TrainingUnit { get; set; }
        public string TrainingLocation { get; set; }
        public string Count { get; set; }
    }


