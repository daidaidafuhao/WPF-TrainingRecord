using System;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Transactions;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

public class DatabaseManager
{
    private string _databaseFilePath;


    private static string directory = @"C:\Path\To\Your\Directory"; // 指定文件目录
    private static string databaseFileName = "TrainingRecords4.db";  // 指定数据库文件名
    private static string apiBaseUrl = "http://localhost:5115"; // API基础地址
    private static ApiClient _apiClient;

    public string DatabaseFilePath { get => _databaseFilePath; set => _databaseFilePath = value; }
    public static string Directory { get => directory; set => directory = value; }
    public static string DatabaseFileName { get => databaseFileName; set => databaseFileName = value; }
    public static string ApiBaseUrl { get => apiBaseUrl; set => apiBaseUrl = value; }

    public DatabaseManager()
    {
        // 指定数据库文件的完整路径
        DatabaseFilePath = Path.Combine(Directory, DatabaseFileName);

        // 确保目录存在
        System.IO.Directory.CreateDirectory(Directory);

        // 初始化ApiClient
        _apiClient = new ApiClient(ApiBaseUrl);

        // 检查数据库文件是否存在，如果不存在则创建
        CreateDatabaseIfNotExists();
    }

    private void CreateDatabaseIfNotExists()
    {
        // 如果数据库文件不存在，则创建一个新的数据库文件
        if (!File.Exists(DatabaseFilePath))
        {
            SQLiteConnection.CreateFile(DatabaseFilePath);
            Console.WriteLine("Database file created at: " + DatabaseFilePath);
            CreateTables();
        }
        else
        {
            Console.WriteLine("Database file already exists at: " + DatabaseFilePath);
        }
    }

    public void LoadComboBoxItems(string query, ComboBox comboBox)
    {
        try
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        // 清空ComboBox，防止重复添加
                        comboBox.Items.Clear();

                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader[0].ToString()); // 假设每个查询结果只有一列
                        }

                        reader.Close();
                    }
                       
                }
                  
               
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("加载数据时出错: " + ex.Message);
        }
    }

    private void CreateTables()
    {
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            // 创建在职人员表
            string createEmployeeTable = @"
            CREATE TABLE IF NOT EXISTS Employee (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                IDCardNumber TEXT NOT NULL UNIQUE,
                Photo TEXT,
                Education TEXT,
                Title TEXT,
                Level TEXT,
                LevelJobType TEXT,
                Position TEXT,
                UnitName TEXT,
                RuzhiDate TEXT,
                SchoolName TEXT,
                ZhuanYe TEXT
            );";

            // 创建培训记录表
            string createTrainingRecordTable = @"
            CREATE TABLE IF NOT EXISTS TrainingRecord (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SerialNumber INTEGER NOT NULL,
                TrainingDate TEXT NOT NULL,
                TrainingContent TEXT NOT NULL,
                TrainingUnit TEXT NOT NULL,
                TrainingLocation TEXT NOT NULL,
                Assessment TEXT,
                Cost REAL,
                Remarks TEXT,
                EmployeeId TEXT,
                FOREIGN KEY (EmployeeId) REFERENCES Employee(Id)
            );";

            // 创建图片表
            string createPhotoTable = @"
            CREATE TABLE IF NOT EXISTS PhotoTable (
                IDCardNumber TEXT NOT NULL UNIQUE,
                Photo BLOB
            );";

            // 创建导入履历表
            string createImportHistoryTable = @"
            CREATE TABLE IF NOT EXISTS ImportHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IDCardNumber TEXT NOT NULL,
                ImportCount INTEGER NOT NULL,
                ImportTime TEXT NOT NULL,
                FOREIGN KEY (IDCardNumber) REFERENCES Employee(IDCardNumber)
            );";

            using (var command = new SQLiteCommand(createEmployeeTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createTrainingRecordTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createPhotoTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createImportHistoryTable, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public async Task<List<Employee>> SearchEmployees(Employee employeeFilter)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(employeeFilter, jsonOptions);

            // 调用API获取员工列表，传递序列化后的JSON字符串
            var response = await _apiClient.PostAsync<List<Employee>>("/api/Database/employees/search", jsonFilter);
            return response ?? new List<Employee>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜索员工时发生错误: {ex.Message}");
            return new List<Employee>();
        }
    }

    public async Task<List<Employee>> GetEmployees(string searchTerm = null)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 调用API获取员工列表
            var response = await _apiClient.GetAsync<List<Employee>>("/api/Database/employees?searchTerm="+ searchTerm);
            return response ?? new List<Employee>();
        }
        catch (Exception ex)
        {

            MessageBox.Show($"获取员工列表时发生错误: {ex.Message}");
            return new List<Employee>();
        }
    }


    // 插入新员工记录
    public async void InsertEmployee(Employee employee)
    {
        try
        {
             // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(employee, jsonOptions);

            // 调用API插入员工记录
            await _apiClient.PostAsync<String>("/api/Database/addEmployees", jsonFilter);
        }
        catch (Exception ex)
        {

            MessageBox.Show($"插入员工记录时发生错误: {ex.Message}");
        }
    }

    // 插入或更新新员工记录
    public async void InsertOrUpdateEmployee(List<Employee> employee)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(employee, jsonOptions);

            // 调用API插入或更新员工记录
            await _apiClient.PostAsync<String>("api/Database/employees/BatchInsertOrUpdateEmployees/", jsonFilter);
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "插入员工记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "插入员工记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "插入员工记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
        }
    }


   

    public async void UpdateEmployeeAndTrainingRecord(Employee employee,string oldId)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(employee, jsonOptions);

            // 调用API插入或更新员工记录
            await _apiClient.PutAsync<String>("api/Database/employees/UpdateEmployeeAndTrainingRecord/" + oldId, jsonFilter);
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "更新后的员工信息时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "更新后的员工信息时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "更新后的员工信息时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
        }
    }

    // 插入或更新培训记录
    public async Task< List<TrainingRecord>> InsertTrainingRecordOrUpdate(List<TrainingRecord> trainingRecord)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(trainingRecord, jsonOptions);

            // 调用API插入或更新培训记录
            List<TrainingRecord> trainingRecords = await _apiClient.PutAsync<List<TrainingRecord>>("api/Database/BatchInsertTrainingRecordOrUpdate/", jsonFilter);
            return trainingRecords;
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "更新培训记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "更新培训记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "更新培训记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
            List<TrainingRecord> trainingRecords = new List<TrainingRecord>();
            return trainingRecords;
        }
    }

    public static string GetCommandTextWithParameters(SQLiteCommand command)
    {
        string commandText = command.CommandText;

        foreach (SqlParameter param in command.Parameters)
        {
            string paramValue = param.Value == null ? "NULL" : $"'{param.Value}'";
            commandText = commandText.Replace(param.ParameterName, paramValue);
        }

        return commandText;
    }
    // 获取下一个序号
    private int GetNextSerialNumber(SQLiteConnection connection, string employeeId)
    {
        // 使用参数化查询来避免 SQL 注入风险
        using (var command = new SQLiteCommand("SELECT COUNT(*) FROM TrainingRecord WHERE EmployeeId = @EmployeeId", connection))
        {
            // 添加参数，确保 EmployeeId 被安全地传递
            command.Parameters.AddWithValue("@EmployeeId", employeeId);

            // 执行查询并返回下一个序号
            return Convert.ToInt32(command.ExecuteScalar()) + 1;
        }
    }


    // 获取指定员工的培训记录
    public async Task<List<TrainingRecord>> GetTrainingRecordsByEmployeeId(String employeeId="")
    {
        try
        {


            // 获取指定员工的培训记录
            List<TrainingRecord> trainingRecords = await _apiClient.GetAsync<List<TrainingRecord>>("api/Database/training-records/"+ employeeId);
            return trainingRecords;
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "获取指定员工的培训记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "获取指定员工的培训记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "获取指定员工的培训记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
            List<TrainingRecord> trainingRecords = new List<TrainingRecord>();
            return trainingRecords;
        }
    }


    // 获取员工的培训记录
    public async Task<List<TrainingRecord>> GetALLTrainingRecords()
    {
        try
        {
            // 获取员工的培训记录
            List<TrainingRecord> trainingRecords = await _apiClient.GetAsync<List<TrainingRecord>>("api/Database/training-records/");
            return trainingRecords;
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "员工的培训记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "员工的培训记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "员工的培训记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
            List<TrainingRecord> trainingRecords = new List<TrainingRecord>();
            return trainingRecords;
        }
    }

    public async Task <List<TrainingRecord>> GetTrainingRecordsByCriteria(string trainingContent, string trainingUnit, string trainingLocation)
    {
        try
        {
            // 获取员工的培训记录
            List<TrainingRecord> trainingRecords = await _apiClient.GetAsync<List<TrainingRecord>>("api/Database/training-records/search?content="+ trainingContent+"&unit="+ trainingUnit + "&location="+ trainingLocation);
            return trainingRecords;
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "员工的培训记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "员工的培训记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "员工的培训记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
            List<TrainingRecord> trainingRecords = new List<TrainingRecord>();
            return trainingRecords;
        }
    }



    public (string FieldName, string ErrorMessage) AnalyzeError(string errorMessage)
    {
        if (errorMessage.Contains("UNIQUE constraint failed"))
        {
            // 从错误消息中提取字段名称
            var fieldName = ExtractFieldName(errorMessage, "UNIQUE constraint failed");
            return (fieldName, "该字段的值必须唯一。");
        }
        else if (errorMessage.Contains("NOT NULL constraint failed"))
        {
            var fieldName = ExtractFieldName(errorMessage, "NOT NULL constraint failed");
            return (fieldName, "该字段不能为空。");
        }
        // 可以添加更多的错误解析逻辑
        return (null, "发生未知错误。");
    }

    private string ExtractFieldName(string errorMessage, string constraintType)
    {
        // 示例: 从 "NOT NULL constraint failed: TrainingRecord.TrainingUnit" 中提取字段名
        var startIndex = errorMessage.IndexOf(constraintType) + constraintType.Length + 2; // +2 以跳过 ": "
        var endIndex = errorMessage.IndexOf(" ", startIndex); // 找到下一个空格
        if (endIndex == -1) endIndex = errorMessage.Length; // 如果没有找到，取到字符串末尾

        var fullFieldName = errorMessage.Substring(startIndex, endIndex - startIndex);
        return fullFieldName.Split('.').Last(); // 返回字段名称部分
    }


    public async void InsertEmployeePhotoByIDCard(string idCardNumber, string imagePath)
    {
        try
        {
            // 读取图片文件并转换为字节数组
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(imageBytes, jsonOptions);
            // 获取员工的培训记录
           await _apiClient.PostAsync<String>("api/Database/employees/photo/" + idCardNumber, jsonFilter);
            
        }
        catch (Exception ex)
        {
            var (fieldName, errorMessage) = AnalyzeError(ex.Message);
            string finalErrorMessage = fieldName != null ? $"{fieldName}: {errorMessage}" : "发生未知错误。";
            throw new Exception(finalErrorMessage, ex);
        }
    }
       public async Task <byte[]> GetPhotoByIDCard(string idCardNumber)
    {
        try
        {

                // 根据身份证号获取员工照片
                byte[] bytes =  await _apiClient.GetAsync<byte[]>("api/Database/employees/GetEmployeePhoto/" + idCardNumber);
            
                return bytes;
        }
        catch (Exception ex)
        {
            MessageBox.Show("查询照片时发生错误：" + ex.Message);
            return null;
        }

    }


   

    // 根据importTime查询导入履历记录
    public async Task <List<ImportHistory>> GetImportHistoriesByImportTime(string importTime)
    {
        try
        {
            importTime = importTime == null ? "1q2w3e4r" : importTime;
            // 根据身份证号获取员工照片
            List < ImportHistory > ImportHistorys= await _apiClient.GetAsync<List<ImportHistory>>("api/Database/importhistories/byTime/" + importTime);

            return ImportHistorys;
        }
        catch (Exception ex)
        {
            MessageBox.Show("根据importTime查询导入履历记录错误：" + ex.Message);
            return null;
        }
    }
    // 增加导入履历记录（如果身份证已经存在，则更新 ImportCount；否则插入新记录）
    public async void InsertOrUpdateImportHistory(List<ImportHistory> importHistory)
    {
        try
        {
            // 设置JSON序列化选项，保持原有的大小写格式
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };

            // 将包装后的对象序列化为JSON字符串
            string jsonFilter = JsonSerializer.Serialize(importHistory, jsonOptions);

            // 调用API插入或更新员工记录
            await _apiClient.PostAsync<String>("/api/Database/importhistories/BatchInsertOrUpdateImportHistories", jsonFilter);
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "导入履历记录时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "导入履历记录时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "导入履历记录时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
        }
    }

    // 删除所有信息
    public async void DeleteEmployeeDataByIDCard(string idCardNumber)
    {
        try
        {

            // 调用API删除所有信息
            await _apiClient.DeleteAsync<String>("/api/Database/employees/DeleteEmployeeByIdCard/" + idCardNumber);
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "删除所有信息时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "删除所有信息时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "删除所有信息时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
        }
    }
    public async void DeleteTrainingRecordBySerialNumber(string serialNumber, string employeeId)
    {
        try
        {

            // 调用API插入或更新员工记录
            await _apiClient.DeleteAsync<String>("/api/Database/training-records/DeleteTrainingRecord/" + employeeId + "/"+ serialNumber);
        }
        catch (Exception ex)
        {
            string errorMessage;

            if (ex.Message.Contains("唯一约束"))
            {
                errorMessage = "删除信息时发生错误: 违反唯一约束。";
            }
            else if (ex.Message.Contains("字段"))
            {
                errorMessage = "删除信息时发生错误: 使用不当，请检查插入的字段。";
            }
            else
            {
                errorMessage = "删除信息时发生错误: " + ex.Message;
            }

            MessageBox.Show(errorMessage);
        }
    }
    
}








