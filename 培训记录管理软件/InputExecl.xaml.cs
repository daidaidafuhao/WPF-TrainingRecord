using MahApps.Metro.Controls;
using System.Windows;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.SQLite;

namespace TrainingRecordManager
{
    public partial class InputExecl : MetroWindow
    {
        private readonly DatabaseManager _dbManager = new DatabaseManager();

        private readonly string[] ExpectedHeaders =
        {
            "姓名", "身份证", "培训时间", "培训地点", "培训单位", "培训内容", "费用", "备注"
        };

        private readonly string[] EmployeesExpectedHeaders =
        {
            "单位名称", "姓名", "身份证", "入职时间", "毕业院校", "所学专业", "职称", "等级级别", "工种"
        };

        public InputExecl()
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

        private void ImportPersons_Click(object sender, RoutedEventArgs e)
        {
            string tempFilePath = "";
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
                if (openFileDialog.ShowDialog() == true)
                {
                    List<Employee> employeeList = new List<Employee>();
                    string filePath = openFileDialog.FileName;

                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show("文件不存在！");
                        return;
                    }

                    // 生成临时文件路径
                     tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath) + Guid.NewGuid().ToString());
                    File.Copy(filePath, tempFilePath);

                    // 调用方法读取 Excel 文件
                    employeeList = ReadEmployeesExcelFile(tempFilePath);

                    foreach (Employee employee in employeeList)
                    {
                        _dbManager.InsertOrUpdateEmployee(employee);
                    }

                    // 删除临时文件
                    File.Delete(tempFilePath);

                    // 显示导入结果（可选）
                    MessageBox.Show($"成功导入 {employeeList.Count} 条员工数据！");
                }
            }
            catch (Exception ex)
            {  // 删除临时文件
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
               
                MessageBox.Show("未知错误" + ex.Message);
            }
        }

        private void ImportTrainingRecords_Click(object sender, RoutedEventArgs e)
        {
            string tempFilePath = "";
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
                if (openFileDialog.ShowDialog() == true)
                {
                    List<TrainingRecord> trainingRecords = new List<TrainingRecord>();
                    List<Employee> employeeList = new List<Employee>();
                    string filePath = openFileDialog.FileName;

                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show("文件不存在！");
                        return;
                    }

                    // 生成临时文件路径
                    tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath) + Guid.NewGuid().ToString());
                    File.Copy(filePath, tempFilePath);

                    // 调用方法读取 Excel 文件
                    (trainingRecords, employeeList) = ReadTrainingRecordExcelFile(tempFilePath);

                    // 开启事务，确保插入的原子性
                    using (var connection = new SQLiteConnection($"Data Source={_dbManager.DatabaseFilePath};Version=3;"))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction()) // 开始事务
                        {
                            try
                            {
                                // 插入 TrainingRecord 数据
                                foreach (TrainingRecord trainingRecord in trainingRecords)
                                {
                                    _dbManager.InsertTrainingRecordOrUpdate(trainingRecord, connection, transaction);
                                }

                                // 插入 Employee 数据
                                foreach (Employee employee in employeeList)
                                {
                                    _dbManager.InsertOrUpdateEmployee(employee, connection, transaction);
                                }

                                transaction.Commit(); // 提交事务
                                MessageBox.Show($"成功导入 {trainingRecords.Count} 条培训记录和 {employeeList.Count} 条员工数据！");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback(); // 回滚事务
                                MessageBox.Show("导入失败，已回滚数据：" + ex.Message);
                            }
                        }
                    }

                    // 删除临时文件
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                // 删除临时文件
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                MessageBox.Show("未知错误：" + ex.Message);
            }
        }

        // 读取员工 Excel 文件
        private List<Employee> ReadEmployeesExcelFile(string filePath)
        {
            var employees = new List<Employee>();

            // 读取 Excel 文件
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets["Sheet1"]; // 读取第一个工作表

                    // 检查第一行是否符合预期格式
                    for (int col = 1; col <= EmployeesExpectedHeaders.Length; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (header != EmployeesExpectedHeaders[col - 1])
                        {
                            MessageBox.Show($"表头格式错误: 第 {col} 列应为 '{EmployeesExpectedHeaders[col - 1]}', 实际为 '{header}'");
                            throw new Exception($"表头格式错误: 第 {col} 列应为 '{EmployeesExpectedHeaders[col - 1]}', 实际为 '{header}'");
                        }
                    }

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // 从第2行开始读取（跳过表头）
                    {
                        if (worksheet.Cells[row, 3].Text == null || worksheet.Cells[row, 3].Text == "")
                        {
                            throw new FormatException($"请检查数据，{row}行的身份证不能为空");
                        }

                        var employee = new Employee
                        {
                            UnitName = worksheet.Cells[row, 1].Text?.Trim() ?? "",          // A列
                            Name = worksheet.Cells[row, 2].Text?.Trim() ?? "",              // B列
                            IDCardNumber = worksheet.Cells[row, 3].Text?.Trim() ?? "",      // C列
                            RuzhiDate = ConvertToDateTime(worksheet.Cells[row, 4].Text?.Trim() ?? ""),  // D列
                            Education = worksheet.Cells[row, 5].Text?.Trim() ?? "",         // E列
                            SchoolName = worksheet.Cells[row, 5].Text?.Trim() ?? "",        // E列
                            ZhuanYe = worksheet.Cells[row, 6].Text?.Trim() ?? "",           // F列
                            Title = worksheet.Cells[row, 7].Text?.Trim() ?? "",             // G列
                            Level = worksheet.Cells[row, 8].Text?.Trim() ?? "",             // H列
                            LevelJobType = worksheet.Cells[row, 9].Text?.Trim() ?? ""              // H列
                        };

                        employees.Add(employee);
                    }
                }
            }
            return employees;
        }

        // 读取培训记录 Excel 文件
        private (List<TrainingRecord>, List<Employee>) ReadTrainingRecordExcelFile(string filePath)
        {
            var trainingRecords = new List<TrainingRecord>();
            var employees = new List<Employee>();

            // 读取 Excel 文件
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets["Sheet1"]; // 读取第一个工作表

                    // 检查第一行是否符合预期格式
                    for (int col = 1; col <= ExpectedHeaders.Length; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (header != ExpectedHeaders[col - 1])
                        {
                            MessageBox.Show($"表头格式错误: 第 {col} 列应为 '{ExpectedHeaders[col - 1]}', 实际为 '{header}'");
                            throw new Exception($"表头格式错误: 第 {col} 列应为 '{ExpectedHeaders[col - 1]}', 实际为 '{header}'");
                        }
                    }

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // 从第2行开始读取（跳过表头）
                    {
                        if (worksheet.Cells[row, 3].Text == null || worksheet.Cells[row, 3].Text == "")
                        {
                            throw new FormatException($"请检查数据，{row}行的身份证不能为空");
                        }

                        var trainingRecord = new TrainingRecord
                        {
                            EmployeeId = worksheet.Cells[row, 2].Text?.Trim() ?? "",                     // B列
                            TrainingDate = ConvertToDateTime(worksheet.Cells[row, 3].Text?.Trim() ?? ""), // C列
                            TrainingLocation = worksheet.Cells[row, 4].Text?.Trim() ?? "",               // D列
                            TrainingUnit = worksheet.Cells[row, 5].Text?.Trim() ?? "",                   // E列
                            TrainingContent = worksheet.Cells[row, 6].Text?.Trim() ?? "",                // F列
                            Cost = ConvertToDecimal(worksheet.Cells[row, 7].Text?.Trim() ?? "0"),        // G列
                            Remarks = worksheet.Cells[row, 8].Text?.Trim() ?? ""                         // H列
                        };

                        var employee = new Employee
                        {
                            Name = worksheet.Cells[row, 1].Text?.Trim() ?? "",              // A列
                            IDCardNumber = worksheet.Cells[row, 2].Text?.Trim() ?? "",      // B列   
                        };

                        trainingRecords.Add(trainingRecord);
                        employees.Add(employee);
                    }
                }
            }
            return (trainingRecords, employees);
        }

        public DateTime ConvertToDateTime(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date;
            }
            else
            {
                MessageBox.Show($"无法将培训时间转换为日期格式: {dateString}");
                throw new FormatException($"无法将培训时间转换为日期格式: {dateString}");
            }
        }

        public decimal ConvertToDecimal(string costString)
        {
            if (decimal.TryParse(costString, out decimal cost))
            {
                return cost;
            }
            else
            {
                MessageBox.Show($"无法将费用转换为数字格式: {costString}");
                throw new FormatException($"无法将费用转换为数字格式: {costString}");
            }
        }


        private void GoToHomePage_Click(object sender, RoutedEventArgs e)
        {
            var homePage = new HomePage();
            homePage.Show();
            this.Close(); // 关闭当前窗口（可选）
        }
    }
}
