using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
namespace ExportTo
{
    public class Export
    {
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
    }

}