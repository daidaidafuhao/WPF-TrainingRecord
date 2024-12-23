using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;

public class WordDocumentGenerator
{
    public void CreateTrainingRecordDocument(string filePath, EmployeeInfo employee)
    {
        // 检查文件路径的目录是否存在，如果不存在，则创建它
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 创建文档并添加内容
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            // 添加主文档部分
            MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            Body body = mainPart.Document.Body;

            // 设置页面大小为A3
            SetPageSize(body);

            // 添加员工信息
            AddEmployeeInfo(body, employee);

            // 添加培训记录表格
            AddTrainingRecordsTable(body, employee.TrainingRecords);

            // 保存文档
            mainPart.Document.Save();
        }
    }

    private void SetPageSize(Body body)
    {
        // 设置页面大小为 A3
        SectionProperties sectionProps = new SectionProperties();
        PageSize pageSize = new PageSize() { Width = 16838, Height = 11906 }; // A3纸张大小，单位是1/1440英寸
        sectionProps.Append(pageSize);

        // 添加到文档中
        body.Append(sectionProps);
    }

    // 添加员工信息
    private void AddEmployeeInfo(Body body, EmployeeInfo employee)
    {
        // 创建并居中显示标题
        Paragraph titleParagraph = new Paragraph();
        ParagraphProperties titleParagraphProperties = new ParagraphProperties();
        titleParagraphProperties.Justification = new Justification { Val = JustificationValues.Center };
        titleParagraph.ParagraphProperties = titleParagraphProperties;
        titleParagraph.Append(new Run(new Text($"{employee.Name}培训档案")));
        body.Append(titleParagraph);

        // 创建员工信息的段落
        Paragraph paragraph = new Paragraph();
        ParagraphProperties paragraphProperties = new ParagraphProperties();
        paragraphProperties.Justification = new Justification { Val = JustificationValues.Center }; // 居中对齐
        paragraph.ParagraphProperties = paragraphProperties;

        // 添加每个字段和对应的值，并插入 Tab 制表符实现间隔
        paragraph.Append(
            CreateRun("姓名: " + employee.Name),
            CreateTab(),
            CreateRun("身份证号: " + employee.IDCardNumber), // 设置字体大小
            CreateTab(),
            CreateRun("学历: " + employee.Education),
            CreateTab(),
            CreateRun("职称/等级: " + employee.Title),
            CreateTab(),
            CreateRun("岗位: " + employee.Position)
        );

        // 将段落添加到文档主体
        body.Append(paragraph);
    }

    // 创建文本运行，支持修改字体大小
    private Run CreateRun(string text, int? fontSize = null)
    {
        Run run = new Run(new Text(text));

        // 如果提供了字体大小，则设置字体大小
        if (fontSize.HasValue)
        {
            RunProperties runProperties = new RunProperties();
            runProperties.FontSize = new FontSize() { Val = (fontSize.Value * 2).ToString() }; // 字号是磅大小 * 2
            run.PrependChild(runProperties);
        }

        return run;
    }

    // 创建 Tab 制表符
    private Run CreateTab()
    {
        return new Run(new TabChar());
    }

    // 添加培训记录表格
    private void AddTrainingRecordsTable(Body body, List<TrainingRecord> records)
    {
        // 创建表格
        Table table = new Table();

        // 定义表格样式
        TableProperties tblProps = new TableProperties(
            new TableBorders(
                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 },
                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 8 }
            ),
            new TableJustification { Val = TableRowAlignmentValues.Center } // 表格居中
        );
        table.AppendChild(tblProps);

        // 添加表头
        TableRow headerRow = new TableRow();
        headerRow.Append(
            CreateTableCell("序号"),
            CreateTableCell("时间"),
            CreateTableCell("培训内容"),
            CreateTableCell("培训单位"),
            CreateTableCell("培训地点"),
            //CreateTableCell("成绩/考核"),
            CreateTableCell("费用"),
            CreateTableCell("备注")
        );
        table.Append(headerRow);

        // 添加数据行
        for (int i = 0; i < records.Count; i++)
        {
            TableRow dataRow = new TableRow();
            dataRow.Append(
                CreateTableCell((i + 1).ToString()),
                CreateTableCell(records[i].TrainingDate.ToString()),
                CreateTableCell(records[i].TrainingContent),
                CreateTableCell(records[i].TrainingUnit),
                CreateTableCell(records[i].TrainingLocation),
                //CreateTableCell(records[i].Assessment),
                CreateTableCell(records[i].Cost.ToString("C")), // 格式化为货币
                CreateTableCell(records[i].Remarks)
            );
            table.Append(dataRow);
        }

        // 将表格添加到文档主体
        body.Append(table);
    }

    // 创建表格单元格
    private TableCell CreateTableCell(string text)
    {
        TableCell cell = new TableCell();
        cell.Append(new Paragraph(new Run(new Text(text))));  // 在单元格中插入文本
        return cell;
    }
}

// 使用示例
public class Program
{
    public static void outDocx(string path, string IdNumber)
    {
        DatabaseManager dbManager = new DatabaseManager();
        List<Employee> employees = dbManager.GetEmployees(IdNumber);
        List<TrainingRecord> Records = dbManager.GetTrainingRecordsByEmployeeId(employees[0].IDCardNumber);
        EmployeeInfo employee = new EmployeeInfo
        {
            Name = employees[0].Name,
            IDCardNumber = employees[0].IDCardNumber,
            Education = employees[0].Education,
            Title = employees[0].Title,
            Position = employees[0].Position,
            TrainingRecords = Records
        };

        WordDocumentGenerator generator = new WordDocumentGenerator();
        generator.CreateTrainingRecordDocument(@path + "/"  + employees[0].Name+ employees[0].IDCardNumber + "的培训记录.docx", employee);
    }

    public static void outAllDocx(string path, List<Employee> employees)
    {
        DatabaseManager dbManager = new DatabaseManager();
        foreach(Employee employee in employees)
        {
            List<TrainingRecord> Records = dbManager.GetTrainingRecordsByEmployeeId(employee.IDCardNumber);
            EmployeeInfo employeefo = new EmployeeInfo
            {
                Name = employee.Name,
                IDCardNumber = employee.IDCardNumber,
                Education = employee.Education,
                Title = employee.Title,
                Position = employee.Position,
                TrainingRecords = Records
            };

            WordDocumentGenerator generator = new WordDocumentGenerator();
            generator.CreateTrainingRecordDocument(@path+"/" + employee.Name+ employee.IDCardNumber + "的培训记录.docx", employeefo);

        }
        
    }


    public static void outRecordsDocx(string path, List<EmployeeInfo> employeeInfos)
    {
        foreach (EmployeeInfo employeeInfo in employeeInfos)
        {
            WordDocumentGenerator generator = new WordDocumentGenerator();
            generator.CreateTrainingRecordDocument(@path + "/" + employeeInfo.Name + employeeInfo.IDCardNumber + "的培训记录.docx", employeeInfo);

        }
    }
}





// 导出需要的员工信息类
public class EmployeeInfo
{
    public string Name { get; set; }
    public string IDCardNumber { get; set; }
    public string Education { get; set; }
    public string Title { get; set; }
    public string Position { get; set; }
    public List<TrainingRecord> TrainingRecords { get; set; }
}
