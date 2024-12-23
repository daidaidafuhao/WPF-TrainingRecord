using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MahApps.Metro.Controls;
using System.Windows;
using 培训记录管理软件;

namespace TrainingRecordManager
{
    public partial class HomePage : MetroWindow
    {
        DatabaseManager dbManager = new DatabaseManager();

        public HomePage()
        {
            InitializeComponent();
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

        private void ImportFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 跳转到文件导入页面
            var importPage = new InputExecl(); // 确保您已创建该页面
            importPage.Show();
            this.Close(); // 关闭当前页面（可选）
        }

        private void ManualInputButton_Click(object sender, RoutedEventArgs e)
        {
            // 跳转到手动录入页面
            var manualInputPage = new ManualInputPage(); // 确保您已创建该页面
            manualInputPage.Show();
            this.Close(); // 关闭当前页面（可选）
        }

        private void ViewSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            // 跳转到查看一览页面
            var summaryPage = new SummaryPage(); // 确保您已创建该页面
            summaryPage.Show();
            this.Close(); // 关闭当前页面（可选）
        }
        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建并显示文件夹选择对话框
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "请选择导出文件的目标文件夹";
                    dialog.ShowNewFolderButton = true;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // 获取用户选择的文件夹路径
                        string selectedPath = dialog.SelectedPath;

                        List<PersonnelInfo> PersonnelInfolist = new List<PersonnelInfo>();
                        List<TrainingInfo> TrainingInfolist = new List<TrainingInfo>();

                        dbManager.GetEmployees().ForEach(e =>
                        {
                            PersonnelInfo ersonnelInfo = new PersonnelInfo
                            {

                                UnitName = e.UnitName, // 单位名称
                                Name = e.Name, // 姓名
                                IDCard = e.IDCardNumber,// 身份证
                                EmploymentDate = e.RuzhiDate,// 入职时间
                                GraduatedSchool = e.SchoolName, // 毕业院校
                                Major = e.ZhuanYe,  //所学专业
                                Title = e.Title,  // 职称
                                Level = e.Level, // 等级级别
                                JobType = e.LevelJobType // 工种
                            };
                            PersonnelInfolist.Add(ersonnelInfo);
                        });
                        dbManager.GetALLTrainingRecords().ForEach(e =>
                        {
                            if (PersonnelInfolist.FirstOrDefault(p => p.IDCard .Equals(e.EmployeeId)) != null)
                            {
                                TrainingInfo trainingInfo = new TrainingInfo
                                {
                                    Name = PersonnelInfolist.FirstOrDefault(p => p.IDCard == e.EmployeeId).Name,           // 姓名
                                    IDCard = e.EmployeeId,         // 身份证
                                    TrainingDate = e.TrainingDate,  // 培训时间
                                    TrainingLocation = e.TrainingLocation,   // 培训地点
                                    TrainingUnit = e.TrainingUnit,      // 培训单位
                                    TrainingContent = e.TrainingContent,   // 培训内容
                                    Cost = e.Cost,     // 费用
                                    Remarks = e.Remarks,          // 备注
                                };
                                TrainingInfolist.Add(trainingInfo);
                            }
                        });


                        // 导出人员信息
                        ExportToExcel(selectedPath + "/PersonnelInfo.xlsx", PersonnelInfolist, new List<string>
    {
        "单位名称", "姓名", "身份证号", "入职时间", "毕业院校", "所学专业", "职称", "等级", "工种"
    });

                        // 导出培训记录
                        ExportToExcel(selectedPath + "/TrainingInfo.xlsx", TrainingInfolist, new List<string>
    {
        "姓名", "身份证号", "培训时间", "培训地点", "培训单位", "培训内容", "费用", "备注"
    });
                        ShowPopu("已经导出");
                    }

                    else
                    {
                        ShowPopu("操作已取消");
                    }
                }

            }
            catch (Exception ex)
            {
                ShowPopu("发生致命错误消息:" + ex.Message);
            }

        }

        public static void ExportToExcel<T>(string filePath, List<T> data, List<string> headers)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var sheetData = new SheetData();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = typeof(T).Name
                };
                sheets.Append(sheet);

                // Add header row
                var headerRow = new Row();
                foreach (var header in headers)
                {
                    headerRow.AppendChild(CreateTextCell(header));
                }
                sheetData.AppendChild(headerRow);

                // Add data rows
                foreach (var item in data)
                {
                    var dataRow = new Row();
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        var value = prop.GetValue(item)?.ToString() ?? string.Empty;
                        dataRow.AppendChild(CreateTextCell(value));
                    }
                    sheetData.AppendChild(dataRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        private static Cell CreateTextCell(string text)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(text)
            };
        }

        private void ShowPopu(string errorMessage)
        {
            PopupWindow popup = new PopupWindow(errorMessage);
            popup.Owner = this; // 设置弹出框的拥有者为主窗口
            popup.ShowDialog(); // 显示模态对话框
        }
    }


}



