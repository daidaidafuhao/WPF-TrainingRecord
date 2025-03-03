using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TemplateBasedWordGenerator
{
    public void GenerateFromTemplate(string outputPath, EmployeeInfo employee)
    {
        try
        {
            // 使用TemplateManager获取模板文件路径
            TemplateManager.Initialize();
            string templatePath = TemplateManager.GetTemplatePath();

            // 检查输出文件路径的目录是否存在，如果不存在，则创建它
            string directoryPath = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 复制模板文件到目标路径
            File.Copy(templatePath, outputPath, true);

            // 打开文档进行编辑
            using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
            {
                MainDocumentPart mainPart = doc.MainDocumentPart;
                Body body = mainPart.Document.Body;

                // 替换员工信息
                ReplaceEmployeeInfo(body, employee);

                // 填充培训记录表格
                FillTrainingRecordsTable(body, employee.TrainingRecords);

                // 保存更改
                mainPart.Document.Save();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"生成文档时发生错误: {ex.Message}", ex);
        }
    }

    private void ReplaceEmployeeInfo(Body body, EmployeeInfo employee)
    {
        try
        {
            var paragraphs = body.Descendants<Paragraph>().ToList();

            foreach (var paragraph in paragraphs)
            {
                var runs = paragraph.Descendants<Run>().ToList();
                foreach (var run in runs)
                {
                    if (run.GetFirstChild<Text>() != null)
                    {
                        var text = run.GetFirstChild<Text>();
                        string content = text.Text;

                        // 替换员工信息占位符，保持原有格式
                        content = content.Replace("[姓名]", employee.Name ?? string.Empty)
                                       .Replace("[身份证号]", employee.IDCardNumber ?? string.Empty)
                                       .Replace("[学历]", employee.Education ?? string.Empty)
                                       .Replace("[职称]", employee.Title ?? string.Empty)
                                       .Replace("[岗位]", employee.Position ?? string.Empty);

                        text.Text = content;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"替换员工信息时发生错误: {ex.Message}", ex);
        }
    }

    private void FillTrainingRecordsTable(Body body, List<TrainingRecord> records)
    {
        try
        {
            // 查找第二个表格
            Table table = body.Descendants<Table>().Skip(1).FirstOrDefault();
            if (table == null) throw new InvalidOperationException("未在模板中找到表格");

            // 获取表格的所有行
            var rows = table.Descendants<TableRow>().ToList();
            if (rows.Count < 2) throw new InvalidOperationException("表格格式不正确，需要至少包含表头行");

            // 保存表头行和表格样式
            var headerRow = rows[0];
            var tableProperties = table.GetFirstChild<TableProperties>()?.CloneNode(true) as TableProperties;

            // 删除除表头外的所有行
            while (table.Descendants<TableRow>().Count() > 1)
            {
                table.RemoveChild(table.Descendants<TableRow>().Last());
            }

            // 添加培训记录数据
            for (int i = 0; i < records.Count; i++)
            {
                TableRow newRow = new TableRow();

                // 复制表头行的样式和属性
                var trProperties = headerRow.GetFirstChild<TableRowProperties>()?.CloneNode(true) as TableRowProperties;
                if (trProperties != null)
                    newRow.AppendChild(trProperties);

                // 添加单元格，保持原有格式
                newRow.Append(
                    CreateTableCell((i + 1).ToString(), headerRow),
                    CreateTableCell(records[i].TrainingDate.ToString("yyyy年MM月dd日"), headerRow),
                    CreateTableCell(records[i].TrainingContent ?? string.Empty, headerRow),
                    CreateTableCell(records[i].TrainingUnit ?? string.Empty, headerRow),
                    CreateTableCell(records[i].TrainingLocation ?? string.Empty, headerRow),
                    CreateTableCell(records[i].Cost.ToString("F2") + "元", headerRow),
                    CreateTableCell(records[i].Remarks ?? string.Empty, headerRow)
                );

                table.Append(newRow);
            }

            // 确保表格样式一致性
            if (tableProperties != null)
            {
                table.RemoveAllChildren<TableProperties>();
                table.PrependChild(tableProperties);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"填充培训记录表格时发生错误: {ex.Message}", ex);
        }
    }

    private TableCell CreateTableCell(string text, TableRow templateRow)
    {
        var templateCell = templateRow.GetFirstChild<TableCell>();
        var newCell = new TableCell();

        // 复制单元格属性
        if (templateCell != null)
        {
            var tcProperties = templateCell.GetFirstChild<TableCellProperties>()?.CloneNode(true) as TableCellProperties;
            if (tcProperties != null)
                newCell.AppendChild(tcProperties);
        }

        // 创建段落并设置文本
        var paragraph = new Paragraph(
            new Run(
                new Text(text ?? string.Empty)
            )
        );

        // 复制段落属性
        if (templateCell != null)
        {
            var templateParagraph = templateCell.GetFirstChild<Paragraph>();
            if (templateParagraph != null)
            {
                var pProperties = templateParagraph.GetFirstChild<ParagraphProperties>()?.CloneNode(true) as ParagraphProperties;
                if (pProperties != null)
                    paragraph.PrependChild(pProperties);
            }
        }

        newCell.Append(paragraph);
        return newCell;
    }

    // 使用示例
    public static async void GenerateDocFromTemplate(string outputPath, string idNumber)
    {
        DatabaseManager dbManager = new DatabaseManager();
        List<Employee> employees = await dbManager.GetEmployees(idNumber);
        if (employees.Count == 0) return;

        List<TrainingRecord> records = await dbManager.GetTrainingRecordsByEmployeeId(employees[0].IDCardNumber);
        EmployeeInfo employee = new EmployeeInfo
        {
            Name = employees[0].Name,
            IDCardNumber = employees[0].IDCardNumber,
            Education = employees[0].Education,
            Title = employees[0].Title,
            Position = employees[0].Position,
            TrainingRecords = records
        };

        var generator = new TemplateBasedWordGenerator();
        generator.GenerateFromTemplate(outputPath, employee);
    }

    public static async void GenerateAllDocsFromTemplate(string outputFolderPath, List<Employee> employees)
    {
        DatabaseManager dbManager = new DatabaseManager();
        foreach (Employee employee in employees)
        {
            List<TrainingRecord> records = await dbManager.GetTrainingRecordsByEmployeeId(employee.IDCardNumber);
            EmployeeInfo employeeInfo = new EmployeeInfo
            {
                Name = employee.Name,
                IDCardNumber = employee.IDCardNumber,
                Education = employee.Education,
                Title = employee.Title,
                Position = employee.Position,
                TrainingRecords = records
            };

            string outputPath = Path.Combine(outputFolderPath, $"{employee.Name}{employee.IDCardNumber}的培训记录.docx");
            var generator = new TemplateBasedWordGenerator();
            generator.GenerateFromTemplate(outputPath, employeeInfo);
        }
    }
}