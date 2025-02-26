using ControlzEx;
using MahApps.Metro.Controls;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using 培训记录管理软件;

namespace TrainingRecordManager
{
    public partial class SummaryPage : MetroWindow
    {
        public ObservableCollection<Employee> EmployeeRecords { get; set; }

        private bool isVisible = false;

        private DataGridRow _editingRow; // 保存正在编辑的行
        private Button _currentEditButton; // 保存当前编辑按钮

        DatabaseManager dbManager = new DatabaseManager();
        public SummaryPage()
        {
            InitializeComponent();
            EmployeeRecords = new ObservableCollection<Employee>();
            Employee.ItemsSource = EmployeeRecords;
            AddTrainingRecord();
           
        }
        public SummaryPage(Employee employee, string importTime)
        {
            InitializeComponent();
            EmployeeRecords = new ObservableCollection<Employee>();
            Employee.ItemsSource = EmployeeRecords;
            AddTrainingRecord(employee, importTime);

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
            Employee.LostFocus += Employee_LostFocus;
        }

        private async void AddTrainingRecord()
        {
            EmployeeRecords.Clear();
 
            // 获取所有在职人员
            List<Employee> allEmployees = await dbManager.GetEmployees();
            foreach (Employee employee in allEmployees)
            { EmployeeRecords.Add((employee)); }

        }

        //根据条件查询
        private async void AddTrainingRecord(Employee employee, string importTime)
        {
            EmployeeRecords.Clear();

            // 获取所有在职人员
            List<Employee> allEmployees = await dbManager.SearchEmployees(employee);

            // 获取所有导入记录的身份证号
            List<ImportHistory> importHistories = await dbManager.GetImportHistoriesByImportTime(importTime);
            int index = 1;
            // 如果 ImportHistory 为空，则直接返回所有员工
            if (importHistories == null || importHistories.Count == 0)
            {
                // 获取批次
                foreach (Employee e in allEmployees)
                {
                    e.Id=(index++).ToString();
                    EmployeeRecords.Add((e)); }
                return;
            }

            // 获取 ImportHistory 中所有的身份证号
            List<string> importCardNumbers = importHistories
                .Select(i => i.IDCardNumber)  // 获取 ImportHistory 中的身份证号列表
                .ToList();

            // 过滤 allEmployees，只保留在 importHistories 中存在的身份证号
            List<Employee> filteredEmployees = allEmployees
                .Where(emp => importCardNumbers.Contains(emp.IDCardNumber)) // 过滤条件
                .ToList();

            // 获取批次
            foreach (Employee e in filteredEmployees)
            { e.Id = (index++).ToString(); 
                EmployeeRecords.Add((e)); }
        }
        private void GoToHomePage_Click(object sender, RoutedEventArgs e)
        {
            var homePage = new HomePage();
            homePage.Show();
            this.Close(); // 关闭当前窗口（可选）
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string idCardNumber = button.Tag.ToString();
                var manualInputPage = new ManualInputPage(idCardNumber);
                manualInputPage.Show();
            }
            this.Close(); // 关闭当前窗口（可选）
        }

        private void OputDocxButton_Click(object sender, RoutedEventArgs e)
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
                        var button = sender as Button;
                        if (button != null)
                        {
                            string idCardNumber = button.Tag.ToString();
                            Program.outDocx(selectedPath, idCardNumber);
                            ShowPopu("已经导出");
                        }
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

        private void OputAllocxButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建并显示文件夹选择对话框
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "请选择导出文件的目标文件夹";
                    dialog.ShowNewFolderButton = true;

                    if (dialog.ShowDialog()== System.Windows.Forms.DialogResult.OK)
                    {
                        // 获取用户选择的文件夹路径
                        string selectedPath = dialog.SelectedPath;

                        // 调用导出方法，并传递所选的文件夹路径
                        Program.outAllDocx(selectedPath, Employee.ItemsSource.Cast<Employee>().ToList());
                        ShowPopu("全部导出成功");
                    }
                    else
                    {
                        ShowPopu("操作已取消");
                    }
                }
            }
            catch(Exception ex)
            {
                ShowPopu("发生致命错误消息:" + ex.Message);
            }
              
        }


        private void GoToFilterAndExport_Click(object sender, RoutedEventArgs e)
        {

            var FilterAndExport = new FilterAndExport();
            FilterAndExport.Show();
            this.Close();
        }


        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            // 跳转到查看一览页面
            var queryPageye = new QueryPage("SummaryPage"); // 确保您已创建该页面
            queryPageye.Owner = this; // 设置弹出框的拥有者为主窗口
                                      // 订阅关闭事件
            queryPageye.PageClosed += (s, args) =>
            {
                // 关闭当前窗口
                this.Close();
            };
            queryPageye.ShowDialog(); // 显示模态对话框
           
        }

        private void ShowPopu(string errorMessage)
        {
            PopupWindow popup = new PopupWindow(errorMessage);
            popup.Owner = this; // 设置弹出框的拥有者为主窗口
            popup.ShowDialog(); // 显示模态对话框
        }


        // 编辑按钮点击事件
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var row = FindParent<DataGridRow>(button); // 获取当前行
            if (row == null) return;


            if (_editingRow != null && _editingRow != row)
            {
                // 如果有其他行正在编辑，退出编辑状态
                CancelEditing();
            }

            // 优先更新按钮文本，避免冲突
            if (button.Content.ToString() == "编辑")
            {
                button.Content = "保存";
                _editingRow = row;
                _currentEditButton = button;

                // 使用 Dispatcher 确保按钮状态更新后再执行编辑逻辑
                Dispatcher.InvokeAsync(() =>
                {
                    // 设置当前行可编辑
                    EnableRowEditing(row, Employee);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
            else if (button.Content.ToString() == "保存")
            {
                // 保存逻辑
                Employee.CommitEdit();
                // 退出编辑状态
                CancelEditing();
                Employee employee = EmployeeRecords.FirstOrDefault(p => p.IDCardNumber == button.Tag);
            }
        }

        // 退出编辑状态
        private void CancelEditing()
        {
            if (_editingRow != null)
            {
                SetRowEditable(_editingRow, false);
                if (_currentEditButton != null)
                {
                    _currentEditButton.Content = "编辑";
                }
                _editingRow = null;
                _currentEditButton = null;
            }
        }

        // 设置行单元格的可编辑状态
        private void SetRowEditable(DataGridRow row, bool isEditable)
        {
            foreach (var column in Employee.Columns)
            {
                var cell = GetCell(Employee, row, Employee.Columns.IndexOf(column));
                if (cell != null)
                {
                    if (isEditable)
                    {
                        cell.IsEditing = true; // 进入编辑状态
                    }
                    else
                    {
                        cell.IsEditing = false; // 退出编辑状态
                    }
                }
            }
        }

        // 获取指定行、列的单元格
        private DataGridCell GetCell(DataGrid grid, DataGridRow row, int columnIndex)
        {
            // 检查列索引是否在范围内（第2列到第9列，索引从1到8）
            if (columnIndex < 1 || columnIndex > 8)
            {
                return null;
            }

            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;

            var cell = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            if (cell == null)
            {
                grid.ScrollIntoView(row, grid.Columns[columnIndex]);
                cell = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            }

            return cell;
        }

        // 工具方法：查找子元素
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // 工具方法：查找父元素
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T parentT)
                    return parentT;

                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void EnableRowEditing(DataGridRow row, DataGrid grid)
        {
            // 手动启用整行的编辑模式
            grid.BeginEdit();

            // 遍历指定列范围的单元格，强制启用编辑模式
            for (int columnIndex = 1; columnIndex <= 8; columnIndex++)
            {
                var cell = GetCell(grid, row, columnIndex);
                if (cell != null)
                {
                    cell.IsEditing = true; // 设置为编辑模式
                }
            }
        }

        private void Employee_LostFocus(object sender, RoutedEventArgs e)
        {
            // 获取焦点切换相关的控件
            if (sender is DataGrid grid && _editingRow != null)
            {
                // 防止行退出编辑模式
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                EnableRowEditing(_editingRow, grid); // 重新激活行编辑
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
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
                dbManager.DeleteEmployeeDataByIDCard(data);

                MessageBox.Show($"删除成功：{data}");
             
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}");
            }
            AddTrainingRecord();
        }

        // 获取单元格的值
        private string GetCellValue(DataGridRow row, int columnIndex)
        {
            var cell = GetCell(Employee, row, columnIndex);
            if (cell != null)
            {
                var textBox = cell.Content as TextBox;
                if (textBox != null)
                {
                    return textBox.Text;  // 如果是TextBox，获取其文本值
                }
            }
            return string.Empty;  // 如果找不到或不是TextBox，返回空字符串
        }


    }



}
