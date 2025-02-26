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


 
    private static ApiClient _apiClient;

   


    public DatabaseManager()
    {
      
        // 初始化ApiClient
        _apiClient = new ApiClient();

      
    }
    public static void SetApiClient(ApiClient apiClient)
    {
        _apiClient= apiClient;
    }


    public async void LoadComboBoxItems(string query, ComboBox comboBox)
    {
        try
        {

            // 清空ComboBox，防止重复添加
            comboBox.Items.Clear();
            // 调用API获取员工列表，传递序列化后的JSON字符串
            List<string> response = await _apiClient.GetAsync<List<string>>("/api/Database/combobox-items?query="+ query);
            foreach (string item in response) {

                comboBox.Items.Add(item); // 假设每个查询结果只有一列
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜索员工时发生错误: {ex.Message}");
        
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






    public async Task<bool> TestApiConnection(string apiUrl)
    {
        try
        {
            var testClient = new ApiClient(apiUrl);
            await testClient.GetAsync<bool>("/api/Database/test-connection");
            return true;
        }
        catch
        {
            return false;
        }
    }


    
}








