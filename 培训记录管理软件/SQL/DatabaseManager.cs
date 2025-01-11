using System;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Transactions;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows;
using System.Text;

public class DatabaseManager
{
    private string _databaseFilePath;


    private static string directory = @"C:\Path\To\Your\Directory"; // 指定文件目录
    private static string databaseFileName = "TrainingRecords4.db";  // 指定数据库文件名

    public  string DatabaseFilePath { get => _databaseFilePath; set => _databaseFilePath = value; }
    public static string Directory { get => directory; set => directory = value; }
    public static string DatabaseFileName { get => databaseFileName; set => databaseFileName = value; }

    public DatabaseManager()
    {
        // 指定数据库文件的完整路径
        DatabaseFilePath = Path.Combine(Directory, DatabaseFileName);

        // 确保目录存在
        System.IO.Directory.CreateDirectory(Directory);

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


    public  List<Employee> SearchEmployees(Employee employeeFilter)
    {
        var queryBuilder = new StringBuilder("SELECT * FROM Employee WHERE 1=1");
        var parameters = new List<SQLiteParameter>();

        if (!string.IsNullOrWhiteSpace(employeeFilter.Name))
        {
            queryBuilder.Append(" AND Name LIKE @Name");
            parameters.Add(new SQLiteParameter("@Name", $"%{employeeFilter.Name}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.IDCardNumber))
        {
            queryBuilder.Append(" AND IDCardNumber LIKE @IDCardNumber");
            parameters.Add(new SQLiteParameter("@IDCardNumber", $"%{employeeFilter.IDCardNumber}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.Photo))
        {
            queryBuilder.Append(" AND Photo LIKE @Photo");
            parameters.Add(new SQLiteParameter("@Photo", $"%{employeeFilter.Photo}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.Education))
        {
            queryBuilder.Append(" AND Education LIKE @Education");
            parameters.Add(new SQLiteParameter("@Education", $"%{employeeFilter.Education}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.Title))
        {
            queryBuilder.Append(" AND Title LIKE @Title");
            parameters.Add(new SQLiteParameter("@Title", $"%{employeeFilter.Title}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.Level))
        {
            queryBuilder.Append(" AND Level LIKE @Level");
            parameters.Add(new SQLiteParameter("@Level", $"%{employeeFilter.Level}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.LevelJobType))
        {
            queryBuilder.Append(" AND LevelJobType LIKE @LevelJobType");
            parameters.Add(new SQLiteParameter("@LevelJobType", $"%{employeeFilter.LevelJobType}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.Position))
        {
            queryBuilder.Append(" AND Position LIKE @Position");
            parameters.Add(new SQLiteParameter("@Position", $"%{employeeFilter.Position}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.UnitName))
        {
            queryBuilder.Append(" AND UnitName LIKE @UnitName");
            parameters.Add(new SQLiteParameter("@UnitName", $"%{employeeFilter.UnitName}%"));
        }

        if (employeeFilter.RuzhiDateStart != DateTime.MinValue && employeeFilter.RuzhiDateEnd != DateTime.MinValue)
        {
            queryBuilder.Append(" AND RuzhiDate BETWEEN @RuzhiDateStart AND @RuzhiDateEnd");
            parameters.Add(new SQLiteParameter("@RuzhiDateStart", employeeFilter.RuzhiDateStart.ToString("yyyy-MM-dd")));
            parameters.Add(new SQLiteParameter("@RuzhiDateEnd", employeeFilter.RuzhiDateEnd.ToString("yyyy-MM-dd")));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.SchoolName))
        {
            queryBuilder.Append(" AND SchoolName LIKE @SchoolName");
            parameters.Add(new SQLiteParameter("@SchoolName", $"%{employeeFilter.SchoolName}%"));
        }

        if (!string.IsNullOrWhiteSpace(employeeFilter.ZhuanYe))
        {
            queryBuilder.Append(" AND ZhuanYe LIKE @ZhuanYe");
            parameters.Add(new SQLiteParameter("@ZhuanYe", $"%{employeeFilter.ZhuanYe}%"));
        }

        var results = new List<Employee>();

        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();
            using (var command = new SQLiteCommand(queryBuilder.ToString(), connection))
            {
                command.Parameters.AddRange(parameters.ToArray());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new Employee
                        {
                            Index = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            IDCardNumber = reader.GetString(reader.GetOrdinal("IDCardNumber")),
                            Photo = reader.IsDBNull(reader.GetOrdinal("Photo")) ? null : reader.GetString(reader.GetOrdinal("Photo")),
                            Education = reader.IsDBNull(reader.GetOrdinal("Education")) ? null : reader.GetString(reader.GetOrdinal("Education")),
                            Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? null : reader.GetString(reader.GetOrdinal("Title")),
                            Level = reader.IsDBNull(reader.GetOrdinal("Level")) ? null : reader.GetString(reader.GetOrdinal("Level")),
                            LevelJobType = reader.IsDBNull(reader.GetOrdinal("LevelJobType")) ? null : reader.GetString(reader.GetOrdinal("LevelJobType")),
                            Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? null : reader.GetString(reader.GetOrdinal("Position")),
                            UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName")),
                            RuzhiDate = reader.GetDateTime(reader.GetOrdinal("RuzhiDate")),
                            SchoolName = reader.IsDBNull(reader.GetOrdinal("SchoolName")) ? null : reader.GetString(reader.GetOrdinal("SchoolName")),
                            ZhuanYe = reader.IsDBNull(reader.GetOrdinal("ZhuanYe")) ? null : reader.GetString(reader.GetOrdinal("ZhuanYe"))
                        });
                    }
                }
            }
        }

        return results;
    }


    public List<Employee> GetEmployees(string searchTerm = null)
    {
        var employees = new List<Employee>();
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            string query = "SELECT * FROM Employee";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += " WHERE Name LIKE @searchTerm OR IDCardNumber LIKE @searchTerm";
            }

            using (var command = new SQLiteCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            Index = reader.GetInt32(0), // Id
                            Name = reader.GetString(1), // Name
                            IDCardNumber = reader.GetString(2), // IDCardNumber
                            Photo = reader.IsDBNull(3) ? null : reader.GetString(3), // Photo
                            Education = reader.IsDBNull(4) ? null : reader.GetString(4), // Education
                            Title = reader.IsDBNull(5) ? null : reader.GetString(5), // Title
                            Level = reader.IsDBNull(6) ? null : reader.GetString(6), // Level
                            LevelJobType = reader.IsDBNull(7) ? null : reader.GetString(7), // LevelJobType
                            Position = reader.IsDBNull(8) ? null : reader.GetString(8), // Position
                             UnitName = reader.IsDBNull(9) ? null : reader.GetString(9), // Position
                              RuzhiDate = DateTime.Parse(reader.IsDBNull(10) ? null : reader.GetString(10)), // Position
                              SchoolName = reader.IsDBNull(11) ? null : reader.GetString(11), // Position
                              ZhuanYe = reader.IsDBNull(12) ? null : reader.GetString(12) // Position
                        };
                        employees.Add(employee);
                    }
                }
            }
        }
        return employees;
    }


    // 插入新员工记录
    public void InsertEmployee(Employee employee)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = "INSERT INTO Employee (Name, IDCardNumber, Photo, Education, Title, Level, LevelJobType, Position,UnitName,RuzhiDate,SchoolName,ZhuanYe) " +
                               "VALUES (@Name, @IDCardNumber, @Photo, @Education, @Title, @Level, @LevelJobType, @Position,@UnitName,@RuzhiDate,@SchoolName,@ZhuanYe)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", employee.Name);
                    command.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                    command.Parameters.AddWithValue("@Photo", (object)employee.Photo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Education", (object)employee.Education ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Title", (object)employee.Title ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Level", (object)employee.Level ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LevelJobType", (object)employee.LevelJobType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UnitName", (object)employee.UnitName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RuzhiDate", (object)employee.RuzhiDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SchoolName", (object)employee.SchoolName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ZhuanYe", (object)employee.ZhuanYe ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (SQLiteException ex)
        {
            string errorMessage;

            switch (ex.ErrorCode)
            {
                case (int)SQLiteErrorCode.Constraint:
                    errorMessage = "插入员工记录时发生错误: 违反唯一约束。";
                    break;
                case (int)SQLiteErrorCode.Misuse:
                    errorMessage = "插入员工记录时发生错误: 使用不当，请检查插入的字段。";
                    break;
                case (int)SQLiteErrorCode.NotFound:
                    errorMessage = "插入员工记录时发生错误: 找不到指定的表或字段。";
                    break;
                default:
                    errorMessage = "插入员工记录时发生错误: " + ex.Message;
                    break;
            }

            throw new Exception(errorMessage, ex);
        }
    }

    // 插入或更新新员工记录
    public void InsertOrUpdateEmployee(Employee employee)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                // 如果身份证号存在，就更新；否则插入新的记录
                string query = @"
            INSERT INTO Employee (Name, IDCardNumber, Photo, Education, Title, Level, LevelJobType, Position, UnitName, RuzhiDate, SchoolName, ZhuanYe)
            VALUES (@Name, @IDCardNumber, @Photo, @Education, @Title, @Level, @LevelJobType, @Position, @UnitName, @RuzhiDate, @SchoolName, @ZhuanYe)
            ON CONFLICT(IDCardNumber) DO UPDATE SET
                Name = excluded.Name,
                Photo = excluded.Photo,
                Education = excluded.Education,
                Title = excluded.Title,
                Level = excluded.Level,
                LevelJobType = excluded.LevelJobType,
                Position = excluded.Position,
                UnitName = excluded.UnitName,
                RuzhiDate = excluded.RuzhiDate,
                SchoolName = excluded.SchoolName,
                ZhuanYe = excluded.ZhuanYe;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", employee.Name);
                    command.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                    command.Parameters.AddWithValue("@Photo", (object)employee.Photo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Education", (object)employee.Education ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Title", (object)employee.Title ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Level", (object)employee.Level ?? DBNull.Value);
                    command.Parameters.AddWithValue("@LevelJobType", (object)employee.LevelJobType ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UnitName", (object)employee.UnitName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RuzhiDate", (object)employee.RuzhiDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SchoolName", (object)employee.SchoolName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ZhuanYe", (object)employee.ZhuanYe ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
                ReorderEmployeeIdsOptimized(connection);
            }
        }
        catch (SQLiteException ex)
        {
            string errorMessage;

            switch (ex.ErrorCode)
            {
                case (int)SQLiteErrorCode.Constraint:
                    errorMessage = "插入或更新员工记录时发生错误: 违反唯一约束。";
                    break;
                case (int)SQLiteErrorCode.Misuse:
                    errorMessage = "插入或更新员工记录时发生错误: 使用不当，请检查插入的字段。";
                    break;
                case (int)SQLiteErrorCode.NotFound:
                    errorMessage = "插入或更新员工记录时发生错误: 找不到指定的表或字段。";
                    break;
                default:
                    errorMessage = "插入或更新员工记录时发生错误: " + ex.Message;
                    break;
            }

            throw new Exception(errorMessage, ex);
        }
    }

    // 插入新员工记录，如果身份证号已存在就不插入
    public void InsertEmployeeIfNotExists(Employee employee)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                // 查询是否存在相同的身份证号
                string checkQuery = "SELECT COUNT(1) FROM Employee WHERE IDCardNumber = @IDCardNumber";

                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                    var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                    if (exists)
                    {
                        Console.WriteLine("记录已存在，未执行插入操作。");
                        return; // 如果记录存在，直接返回
                    }
                }

                // 如果不存在，执行插入操作
                string insertQuery = "INSERT INTO Employee (Name, IDCardNumber, Photo, Education, Title, Level, LevelJobType, Position, UnitName, RuzhiDate, SchoolName, ZhuanYe) " +
                                     "VALUES (@Name, @IDCardNumber, @Photo, @Education, @Title, @Level, @LevelJobType, @Position, @UnitName, @RuzhiDate, @SchoolName, @ZhuanYe)";

                using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Name", employee.Name);
                    insertCommand.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                    insertCommand.Parameters.AddWithValue("@Photo", (object)employee.Photo ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Education", (object)employee.Education ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Title", (object)employee.Title ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Level", (object)employee.Level ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@LevelJobType", (object)employee.LevelJobType ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@UnitName", (object)employee.UnitName ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@RuzhiDate", (object)employee.RuzhiDate ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@SchoolName", (object)employee.SchoolName ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@ZhuanYe", (object)employee.ZhuanYe ?? DBNull.Value);

                    insertCommand.ExecuteNonQuery();
                    Console.WriteLine("新员工记录插入成功。");
                }
            }
        }
        catch (SQLiteException ex)
        {
            throw new Exception("插入员工记录时发生错误: " + ex.Message, ex);
        }
    }

    // 插入新员工记录，如果身份证号已存在就只更新名字（支持事务）
    public void InsertOrUpdateEmployee(Employee employee, SQLiteConnection connection, SQLiteTransaction transaction)
    {
        try
        {
            // 查询是否存在相同的身份证号
            string checkQuery = "SELECT COUNT(1) FROM Employee WHERE IDCardNumber = @IDCardNumber";

            using (var checkCommand = new SQLiteCommand(checkQuery, connection, transaction))
            {
                checkCommand.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (exists)
                {
                    // 如果记录存在，更新名字
                    string updateQuery = "UPDATE Employee SET Name = @Name WHERE IDCardNumber = @IDCardNumber";

                    using (var updateCommand = new SQLiteCommand(updateQuery, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Name", employee.Name);
                        updateCommand.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                        updateCommand.ExecuteNonQuery();
                        Console.WriteLine("记录已存在，已更新名字。");
                    }
                    return;
                }
            }

            // 如果不存在，执行插入操作
            string insertQuery = "INSERT INTO Employee (Name, IDCardNumber, Photo, Education, Title, Level, LevelJobType, Position, UnitName, RuzhiDate, SchoolName, ZhuanYe) " +
                                 "VALUES (@Name, @IDCardNumber, @Photo, @Education, @Title, @Level, @LevelJobType, @Position, @UnitName, @RuzhiDate, @SchoolName, @ZhuanYe)";

            using (var insertCommand = new SQLiteCommand(insertQuery, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@Name", employee.Name);
                insertCommand.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber);
                insertCommand.Parameters.AddWithValue("@Photo", (object)employee.Photo ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Education", (object)employee.Education ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Title", (object)employee.Title ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Level", (object)employee.Level ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@LevelJobType", (object)employee.LevelJobType ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@UnitName", (object)employee.UnitName ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@RuzhiDate", (object)employee.RuzhiDate ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@SchoolName", (object)employee.SchoolName ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@ZhuanYe", (object)employee.ZhuanYe ?? DBNull.Value);

                insertCommand.ExecuteNonQuery();
                Console.WriteLine("新员工记录插入成功。");
            }
        }
        catch (SQLiteException ex)
        {
            throw new Exception("插入或更新员工记录时发生错误: " + ex.Message, ex);
        }
    }


    public void UpdateEmployeeAndTrainingRecord(Employee employee,string oldId)
    {
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())  // 开始事务
            {
                try
                {
                    // 先更新员工表中的身份证号
                    string updateEmployeeQuery = @"
                    UPDATE Employee
                    SET 
                        Name = @Name,
                        IDCardNumber = @IDCardNumber,
                        Photo = @Photo,
                        Education = @Education,
                        Title = @Title,
                        Level = @Level,
                        LevelJobType = @LevelJobType,
                        Position = @Position,
                        UnitName = @UnitName,
                        RuzhiDate = @RuzhiDate,
                        SchoolName = @SchoolName,
                        ZhuanYe = @ZhuanYe
                    WHERE IDCardNumber = @OldIDCardNumber";

                    using (var command = new SQLiteCommand(updateEmployeeQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OldIDCardNumber", oldId);  // 原身份证号
                        command.Parameters.AddWithValue("@Name", employee.Name);
                        command.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber); // 新身份证号
                        command.Parameters.AddWithValue("@Photo", (object)employee.Photo ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Education", (object)employee.Education ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Title", (object)employee.Title ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Level", (object)employee.Level ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LevelJobType", (object)employee.LevelJobType ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                        command.Parameters.AddWithValue("@UnitName", (object)employee.UnitName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RuzhiDate", (object)employee.RuzhiDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@SchoolName", (object)employee.SchoolName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ZhuanYe", (object)employee.ZhuanYe ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }

                    // 然后更新培训记录表中的身份证号
                    string updateTrainingRecordQuery = @"
                    UPDATE TrainingRecord
                    SET EmployeeId =  @IDCardNumber
                    WHERE EmployeeId =  @OldIDCardNumber";

                    using (var command = new SQLiteCommand(updateTrainingRecordQuery, connection))
                    {
                        command.Parameters.AddWithValue("@OldIDCardNumber", oldId);  // 原身份证号
                        command.Parameters.AddWithValue("@IDCardNumber", employee.IDCardNumber); // 新身份证号
                        command.ExecuteNonQuery();
                    }

                    // 提交事务，确保更新操作是原子性的
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // 如果发生任何错误，回滚事务
                    transaction.Rollback();
                    throw new Exception("更新员工记录和培训记录时发生错误", ex);
                }
            }
        }
    }

    // 插入培训记录
    public void InsertTrainingRecord(TrainingRecord trainingRecord)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                // 计算序号
                int serialNumber = GetNextSerialNumber(connection, trainingRecord.EmployeeId);

                string query = @"
                INSERT INTO TrainingRecord (EmployeeId, SerialNumber, TrainingDate, TrainingContent, TrainingUnit, TrainingLocation, Assessment, Cost, Remarks)
                VALUES (@EmployeeId, @SerialNumber, @TrainingDate, @TrainingContent, @TrainingUnit, @TrainingLocation, @Assessment, @Cost, @Remarks)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", trainingRecord.EmployeeId);
                    command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    command.Parameters.AddWithValue("@TrainingDate", trainingRecord.TrainingDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@TrainingContent", trainingRecord.TrainingContent);
                    command.Parameters.AddWithValue("@TrainingUnit", trainingRecord.TrainingUnit);
                    command.Parameters.AddWithValue("@TrainingLocation", trainingRecord.TrainingLocation);
                    command.Parameters.AddWithValue("@Assessment", trainingRecord.Assessment ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Cost", trainingRecord.Cost);
                    command.Parameters.AddWithValue("@Remarks", trainingRecord.Remarks ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
        catch (SQLiteException ex)
        {
            var (fieldName, errorMessage) = AnalyzeError(ex.Message);
            string finalErrorMessage = fieldName != null ? $"{fieldName}: {errorMessage}" : "发生未知错误。";
            throw new Exception(finalErrorMessage, ex);
        }
    }


    // 插入或更新培训记录
    public void InsertTrainingRecordOrUpdate(TrainingRecord trainingRecord)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                if (trainingRecord.Id == 0)
                {
                    // 直接插入新记录
                    int serialNumber = GetNextSerialNumber( connection,trainingRecord.EmployeeId);

                    string insertQuery = @"
                INSERT INTO TrainingRecord (EmployeeId, SerialNumber, TrainingDate, TrainingContent, TrainingUnit, TrainingLocation, Assessment, Cost, Remarks)
                VALUES (@EmployeeId, @SerialNumber, @TrainingDate, @TrainingContent, @TrainingUnit, @TrainingLocation, @Assessment, @Cost, @Remarks)";

                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeId", trainingRecord.EmployeeId);
                        command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                        command.Parameters.AddWithValue("@TrainingDate", trainingRecord.TrainingDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@TrainingContent", trainingRecord.TrainingContent);
                        command.Parameters.AddWithValue("@TrainingUnit", trainingRecord.TrainingUnit);
                        command.Parameters.AddWithValue("@TrainingLocation", trainingRecord.TrainingLocation);
                        command.Parameters.AddWithValue("@Assessment", trainingRecord.Assessment ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", trainingRecord.Cost);
                        command.Parameters.AddWithValue("@Remarks", trainingRecord.Remarks ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    // 更新已有的培训记录
                    string updateQuery = @"
                UPDATE TrainingRecord
                SET EmployeeId = @EmployeeId,
                    SerialNumber = @SerialNumber,
                    TrainingDate = @TrainingDate,
                    TrainingContent = @TrainingContent,
                    TrainingUnit = @TrainingUnit,
                    TrainingLocation = @TrainingLocation,
                    Assessment = @Assessment,
                    Cost = @Cost,
                    Remarks = @Remarks
                WHERE Id = @Id";

                    using (var command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", trainingRecord.Id);
                        command.Parameters.AddWithValue("@EmployeeId", trainingRecord.EmployeeId);
                        command.Parameters.AddWithValue("@SerialNumber", trainingRecord.SerialNumber);
                        command.Parameters.AddWithValue("@TrainingDate", trainingRecord.TrainingDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@TrainingContent", trainingRecord.TrainingContent);
                        command.Parameters.AddWithValue("@TrainingUnit", trainingRecord.TrainingUnit);
                        command.Parameters.AddWithValue("@TrainingLocation", trainingRecord.TrainingLocation);
                        command.Parameters.AddWithValue("@Assessment", trainingRecord.Assessment ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", trainingRecord.Cost);
                        command.Parameters.AddWithValue("@Remarks", trainingRecord.Remarks ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (SQLiteException ex)
        {
            var (fieldName, errorMessage) = AnalyzeError(ex.Message);
            string finalErrorMessage = fieldName != null ? $"{fieldName}: {errorMessage}" : "发生未知错误。";
            throw new Exception(finalErrorMessage, ex);
        }
    }


    public void InsertTrainingRecordOrUpdate(TrainingRecord trainingRecord, SQLiteConnection connection, SQLiteTransaction transaction)
    {
        try
        {
            // 将空字符串视为null
            

            // 查询现有记录，排除Id，检查其他字段是否已经存在
            string checkQuery = @"
            SELECT Id
            FROM TrainingRecord
            WHERE EmployeeId = @EmployeeId
              AND TrainingDate = @TrainingDate
              AND TrainingContent = @TrainingContent
              AND TrainingUnit = @TrainingUnit
              AND TrainingLocation = @TrainingLocation
              AND Cost = @Cost
              AND Remarks = @Remarks"; 

            using (var checkCommand = new SQLiteCommand(checkQuery, connection, transaction))
            {
                string cleanedEmployeeId = trainingRecord.EmployeeId.Trim(); // 去除前后空格
                cleanedEmployeeId = cleanedEmployeeId.Replace("\n", "").Replace("\r", ""); // 去掉换行或回车
                checkCommand.Parameters.AddWithValue("@EmployeeId", cleanedEmployeeId);
                checkCommand.Parameters.AddWithValue("@TrainingDate", trainingRecord.TrainingDate.ToString("yyyy-MM-dd"));
                checkCommand.Parameters.AddWithValue("@TrainingContent", trainingRecord.TrainingContent ?? (object)DBNull.Value);
                checkCommand.Parameters.AddWithValue("@TrainingUnit", trainingRecord.TrainingUnit ?? (object)DBNull.Value);
                checkCommand.Parameters.AddWithValue("@TrainingLocation", trainingRecord.TrainingLocation ?? (object)DBNull.Value);
                checkCommand.Parameters.AddWithValue("@Cost", trainingRecord.Cost);
                checkCommand.Parameters.AddWithValue("@Remarks", trainingRecord.Remarks ?? (object)DBNull.Value);

                var existingRecordId = checkCommand.ExecuteScalar();

                if (existingRecordId != null)
                {
                    // 如果记录存在且数据完全相同，不进行任何操作
                    return;  // 数据已经存在，直接返回
                }
                else
                {
                    // 如果记录不存在，则插入新记录
                    int serialNumber = GetNextSerialNumber(connection, trainingRecord.EmployeeId);

                    string insertQuery = @"
                    INSERT INTO TrainingRecord (EmployeeId, SerialNumber, TrainingDate, TrainingContent, TrainingUnit, TrainingLocation, Assessment, Cost, Remarks)
                    VALUES (@EmployeeId, @SerialNumber, @TrainingDate, @TrainingContent, @TrainingUnit, @TrainingLocation, @Assessment, @Cost, @Remarks)";

                    using (var command = new SQLiteCommand(insertQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@EmployeeId", trainingRecord.EmployeeId);
                        command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                        command.Parameters.AddWithValue("@TrainingDate", trainingRecord.TrainingDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@TrainingContent", trainingRecord.TrainingContent ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TrainingUnit", trainingRecord.TrainingUnit ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TrainingLocation", trainingRecord.TrainingLocation ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Assessment", trainingRecord.Assessment ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", trainingRecord.Cost);
                        command.Parameters.AddWithValue("@Remarks", trainingRecord.Remarks ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (SQLiteException ex)
        {
            throw new Exception("操作培训记录时发生错误: " + ex.Message, ex);
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


    // 根据employeeId
    public List<TrainingRecord> GetTrainingRecordsByEmployeeId(String employeeId)
    {
        var trainingRecords = new List<TrainingRecord>();

        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            string query = "SELECT * FROM TrainingRecord WHERE EmployeeId = @EmployeeId ORDER BY SerialNumber";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var trainingRecord = new TrainingRecord
                        {
                            Id = reader.GetInt32(0),
                            SerialNumber = reader.GetInt32(1), // SerialNumber
                            TrainingDate = DateTime.Parse(reader.GetString(2)), // TrainingDate
                            TrainingContent = reader.GetString(3), // TrainingContent
                            TrainingUnit = reader.GetString(4), // TrainingUnit
                            TrainingLocation = reader.GetString(5), // TrainingLocation
                            Assessment = reader.IsDBNull(6) ? null : reader.GetString(6), // Assessment
                            Cost = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7), // Cost
                            Remarks = reader.IsDBNull(8) ? null : reader.GetString(8) // Remarks
                        };
                        trainingRecords.Add(trainingRecord);
                    }
                }
            }
        }

        return trainingRecords;
    }


    // 根据employeeId
    public List<TrainingRecord> GetALLTrainingRecords()
    {
        var trainingRecords = new List<TrainingRecord>();

        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            string query = "SELECT * FROM TrainingRecord  ORDER BY SerialNumber";

            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var trainingRecord = new TrainingRecord
                        {
                            Id = reader.GetInt32(0),
                            SerialNumber = reader.GetInt32(1), // SerialNumber
                            TrainingDate = DateTime.Parse(reader.GetString(2)), // TrainingDate
                            TrainingContent = reader.GetString(3), // TrainingContent
                            TrainingUnit = reader.GetString(4), // TrainingUnit
                            TrainingLocation = reader.GetString(5), // TrainingLocation
                            Assessment = reader.IsDBNull(6) ? null : reader.GetString(6), // Assessment
                            Cost = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7), // Cost
                            Remarks = reader.IsDBNull(8) ? null : reader.GetString(8), // Remarks
                            EmployeeId= reader.IsDBNull(9) ? null : reader.GetString(9) // EmployeeId

                        };
                        trainingRecords.Add(trainingRecord);
                    }
                }
            }
        }

        return trainingRecords;
    }

    public List<TrainingRecord> GetTrainingRecordsByCriteria(string trainingContent, string trainingUnit, string trainingLocation)
    {
        var trainingRecords = new List<TrainingRecord>();

        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            // 使用参数化查询避免 SQL 注入
            string query = "SELECT * FROM TrainingRecord WHERE 1=1";

            // 添加条件
            if (!string.IsNullOrEmpty(trainingContent))
            {
                query += " AND TrainingContent = @TrainingContent";
            }
            if (!string.IsNullOrEmpty(trainingUnit))
            {
                query += " AND TrainingUnit = @TrainingUnit";
            }
            if (!string.IsNullOrEmpty(trainingLocation))
            {
                query += " AND TrainingLocation = @TrainingLocation";
            }

            // 排序
            query += " ORDER BY SerialNumber";

            using (var command = new SQLiteCommand(query, connection))
            {
                // 添加查询参数
                if (!string.IsNullOrEmpty(trainingContent))
                {
                    command.Parameters.AddWithValue("@TrainingContent", trainingContent );
                }
                if (!string.IsNullOrEmpty(trainingUnit))
                {
                    command.Parameters.AddWithValue("@TrainingUnit", trainingUnit );
                }
                if (!string.IsNullOrEmpty(trainingLocation))
                {
                    command.Parameters.AddWithValue("@TrainingLocation",  trainingLocation );
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var trainingRecord = new TrainingRecord
                        {
                            Id = reader.GetInt32(0),
                            EmployeeId= reader.GetString(9),
                            SerialNumber = reader.GetInt32(1), // SerialNumber
                            TrainingDate = DateTime.Parse(reader.GetString(2)), // TrainingDate
                            TrainingContent = reader.GetString(3), // TrainingContent
                            TrainingUnit = reader.GetString(4), // TrainingUnit
                            TrainingLocation = reader.GetString(5), // TrainingLocation
                            Assessment = reader.IsDBNull(6) ? null : reader.GetString(6), // Assessment
                            Cost = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7), // Cost
                            Remarks = reader.IsDBNull(8) ? null : reader.GetString(8) // Remarks
                        };
                        trainingRecords.Add(trainingRecord);
                    }
                }
            }
        }

        return trainingRecords;
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


    public void InsertEmployeePhotoByIDCard(string idCardNumber, string imagePath)
    {
        try
        {
            // 读取图片文件并转换为字节数组
            byte[] imageBytes = File.ReadAllBytes(imagePath);
         
            using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                connection.Open();

                // 插入或更新图片的 SQL 查询
                string query = "INSERT OR REPLACE INTO PhotoTable (IDCardNumber, Photo) VALUES (@IDCardNumber, @Photo)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    // 设置查询参数
                    command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                    command.Parameters.AddWithValue("@Photo", imageBytes);

                    // 执行插入或更新操作
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("图片已插入或更新数据库。");
                    }
                    else
                    {
                        Console.WriteLine("没有插入或更新任何数据。");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            var (fieldName, errorMessage) = AnalyzeError(ex.Message);
            string finalErrorMessage = fieldName != null ? $"{fieldName}: {errorMessage}" : "发生未知错误。";
            throw new Exception(finalErrorMessage, ex);
        }
    }
   public byte[] GetPhotoByIDCard(string idCardNumber)
{
    try
    {
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            // 查询数据库中的照片
            string query = "SELECT Photo FROM PhotoTable WHERE IDCardNumber = @IDCardNumber";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);

                // 执行查询，获取结果
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    // 返回存储在数据库中的字节数组
                    return (byte[])result;
                }
                else
                {
                    Console.WriteLine("未找到该身份证号对应的照片。");
                    return null;  // 如果找不到记录，返回null
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("查询照片时发生错误：" + ex.Message);
        return null;
    }

}
    // 查询所有导入履历记录
    public List<ImportHistory> GetAllImportHistories()
    {
        string query = "SELECT * FROM ImportHistory;";
        var result = new List<ImportHistory>();

        using (var connection = new SQLiteConnection(($"Data Source={DatabaseFilePath};Version=3;")))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ImportHistory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IDCardNumber = reader["IDCardNumber"].ToString(),
                            ImportCount = reader["ImportCount"].ToString(),
                            ImportTime = reader["ImportTime"].ToString()
                        });
                    }
                }
            }
        }

        return result;
    }

    // 根据身份证号码查询导入履历记录
    public List<ImportHistory> GetImportHistoriesByIDCard(string idCardNumber)
    {
        string query = "SELECT * FROM ImportHistory WHERE IDCardNumber = @IDCardNumber;";
        var result = new List<ImportHistory>();

        using (var connection = new SQLiteConnection(($"Data Source={DatabaseFilePath};Version=3;")))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ImportHistory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IDCardNumber = reader["IDCardNumber"].ToString(),
                            ImportCount = reader["ImportCount"].ToString(),
                            ImportTime = reader["ImportTime"].ToString()
                        });
                    }
                }
            }
        }

        return result;
    }


    // 根据importTime查询导入履历记录
    public List<ImportHistory> GetImportHistoriesByImportTime(string importTime)
    {
        string query = "SELECT * FROM ImportHistory WHERE ImportTime = @importTime;";
        var result = new List<ImportHistory>();

        using (var connection = new SQLiteConnection(($"Data Source={DatabaseFilePath};Version=3;")))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@importTime", importTime);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ImportHistory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IDCardNumber = reader["IDCardNumber"].ToString(),
                            ImportCount = reader["ImportCount"].ToString(),
                            ImportTime = reader["ImportTime"].ToString()
                        });
                    }
                }
            }
        }

        return result;
    }
    // 增加导入履历记录（如果身份证已经存在，则更新 ImportCount；否则插入新记录）
    public void InsertOrUpdateImportHistory(ImportHistory importHistory)
    {
        string checkQuery = "SELECT COUNT(1) FROM ImportHistory WHERE IDCardNumber = @IDCardNumber;";
        string insertQuery = @"
    INSERT INTO ImportHistory 
    (IDCardNumber, ImportCount, ImportTime) 
    VALUES 
    (@IDCardNumber, @ImportCount, @ImportTime);";
        string updateQuery = @"
    UPDATE ImportHistory 
    SET ImportCount = @ImportCount 
    WHERE IDCardNumber = @IDCardNumber;";

        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();

            // 检查是否存在
            using (var checkCommand = new SQLiteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                long count = (long)checkCommand.ExecuteScalar();

                if (count > 0)
                {
                    // 更新 ImportCount
                    using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                        updateCommand.Parameters.AddWithValue("@ImportCount", importHistory.ImportCount);
                        updateCommand.ExecuteNonQuery();
                        Console.WriteLine($"更新了 IDCardNumber {importHistory.IDCardNumber} 的 ImportCount。");
                    }
                }
                else
                {
                    // 插入新记录
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                        insertCommand.Parameters.AddWithValue("@ImportCount", importHistory.ImportCount);
                        insertCommand.Parameters.AddWithValue("@ImportTime", importHistory.ImportTime);
                        insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"成功插入 IDCardNumber {importHistory.IDCardNumber} 的导入记录。");
                    }
                }
            }
        }
    }



    // 删除导入履历记录（根据主键 Id）
    public void DeleteImportHistory(int id)
    {
        string query = "DELETE FROM ImportHistory WHERE Id = @Id;";

        using (var connection = new SQLiteConnection(($"Data Source={DatabaseFilePath};Version=3;")))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }
    }

    // 更新导入履历记录
    public void UpdateImportHistory(ImportHistory importHistory)
    {
        string query = @"
            UPDATE ImportHistory
            SET 
                IDCardNumber = @IDCardNumber,
                ImportCount = @ImportCount,
                ImportTime = @ImportTime
            WHERE 
                Id = @Id;";

        using (var connection = new SQLiteConnection(($"Data Source={DatabaseFilePath};Version=3;")))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", importHistory.Id);
                command.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                command.Parameters.AddWithValue("@ImportCount", importHistory.ImportCount);
                command.Parameters.AddWithValue("@ImportTime", importHistory.ImportTime);
                command.ExecuteNonQuery();
            }
        }
    }


    public void InsertOrUpdateImportHistory(ImportHistory importHistory, SQLiteConnection connection, SQLiteTransaction transaction)
    {
        string checkQuery = "SELECT COUNT(1) FROM ImportHistory WHERE IDCardNumber = @IDCardNumber;";
        string insertQuery = @"
INSERT INTO ImportHistory 
(IDCardNumber, ImportCount, ImportTime) 
VALUES 
(@IDCardNumber, @ImportCount, @ImportTime);";
        string updateQuery = @"
UPDATE ImportHistory 
SET ImportCount = @ImportCount 
WHERE IDCardNumber = @IDCardNumber;";

        // 使用传入的连接和事务执行操作
        using (var checkCommand = new SQLiteCommand(checkQuery, connection, transaction))
        {
            checkCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
            long count = (long)checkCommand.ExecuteScalar();

            if (count > 0)
            {
                // 更新 ImportCount
                using (var updateCommand = new SQLiteCommand(updateQuery, connection, transaction))
                {
                    updateCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                    updateCommand.Parameters.AddWithValue("@ImportCount", importHistory.ImportCount);
                    updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"更新了 IDCardNumber {importHistory.IDCardNumber} 的 ImportCount。");
                }
            }
            else
            {
                // 插入新记录
                using (var insertCommand = new SQLiteCommand(insertQuery, connection, transaction))
                {
                    insertCommand.Parameters.AddWithValue("@IDCardNumber", importHistory.IDCardNumber);
                    insertCommand.Parameters.AddWithValue("@ImportCount", importHistory.ImportCount);
                    insertCommand.Parameters.AddWithValue("@ImportTime", importHistory.ImportTime);
                    insertCommand.ExecuteNonQuery();
                    Console.WriteLine($"成功插入 IDCardNumber {importHistory.IDCardNumber} 的导入记录。");
                }
            }
        }
    }
    // 删除所有信息
    public void DeleteEmployeeDataByIDCard(string idCardNumber)
    {
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 删除培训记录表中与员工相关的记录
                    string deleteTrainingRecordsQuery = @"
                    DELETE FROM TrainingRecord
                    WHERE EmployeeId = (SELECT Id FROM Employee WHERE IDCardNumber = @IDCardNumber);";

                    using (var command = new SQLiteCommand(deleteTrainingRecordsQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        command.ExecuteNonQuery();
                    }

                    // 删除员工记录
                    string deleteEmployeeQuery = @"
                    DELETE FROM Employee
                    WHERE IDCardNumber = @IDCardNumber;";

                    using (var command = new SQLiteCommand(deleteEmployeeQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        command.ExecuteNonQuery();
                    }

                    // 删除图片表中与员工相关的记录
                    string deletePhotoQuery = @"
                    DELETE FROM PhotoTable
                    WHERE IDCardNumber = @IDCardNumber;";

                    using (var command = new SQLiteCommand(deletePhotoQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        command.ExecuteNonQuery();
                    }

                    // 删除图片表中与员工相关的记录
                    string deleteImportHistoryQuery = @"
                    DELETE FROM ImportHistory
                    WHERE IDCardNumber = @IDCardNumber;";

                    using (var command = new SQLiteCommand(deleteImportHistoryQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IDCardNumber", idCardNumber);
                        command.ExecuteNonQuery();
                    }
                    ReorderEmployeeIdsOptimized(connection, transaction);
                    // 提交事务
                    transaction.Commit();
                    Console.WriteLine("身份证号为 {0} 的所有数据已删除成功。", idCardNumber);
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    Console.WriteLine("删除数据时发生错误: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
    public void DeleteTrainingRecordBySerialNumber(string serialNumber, string employeeId)
    {
        using (var connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 删除指定的培训记录
                    string deleteTrainingRecordsQuery = @"
                DELETE FROM TrainingRecord
                WHERE SerialNumber = @SerialNumber AND EmployeeId = @EmployeeId;";
                    using (var deleteCommand = new SQLiteCommand(deleteTrainingRecordsQuery, connection, transaction))
                    {
                        deleteCommand.Parameters.AddWithValue("@SerialNumber", serialNumber);
                        deleteCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
                        deleteCommand.ExecuteNonQuery();
                    }

                    // 更新剩余的培训记录的 SerialNumber
                    string updateSerialNumbersQuery = @"
                WITH OrderedRecords AS (
                    SELECT Id, ROW_NUMBER() OVER (ORDER BY SerialNumber) AS NewSerialNumber
                    FROM TrainingRecord
                    WHERE EmployeeId = @EmployeeId
                )
                UPDATE TrainingRecord
                SET SerialNumber = OrderedRecords.NewSerialNumber
                FROM OrderedRecords
                WHERE TrainingRecord.Id = OrderedRecords.Id;";
                    using (var updateCommand = new SQLiteCommand(updateSerialNumbersQuery, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
                        updateCommand.ExecuteNonQuery();
                    }

                    // 提交事务
                    transaction.Commit();
                    Console.WriteLine("序号 {0} 的数据已删除成功，并重新排序。", serialNumber);
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    Console.WriteLine("删除数据时发生错误: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
    // 有事务
    public void ReorderEmployeeIdsOptimized(SQLiteConnection connection, SQLiteTransaction transaction)
{
    // 查询所有剩余的记录，按 ID 排序
    var query = "SELECT Id FROM Employee ORDER BY Id";
    var command = new SQLiteCommand(query, connection);
    
    // 获取所有 ID，按顺序更新
    List<int> ids = new List<int>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            ids.Add(reader.GetInt32(0));
        }
    }


        try
        {
            int newId = 1;
            foreach (var currentId in ids)
            {
                // 更新记录的 ID
                if (currentId != newId)
                {
                    var updateCommand = new SQLiteCommand("UPDATE Employee SET Id = @NewId WHERE Id = @CurrentId", connection,transaction);
                    updateCommand.Parameters.AddWithValue("@NewId", newId);
                    updateCommand.Parameters.AddWithValue("@CurrentId", currentId);
                    updateCommand.ExecuteNonQuery();
                   
                }
                newId++;
            }
        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    // 无事务
    public void ReorderEmployeeIdsOptimized(SQLiteConnection connection)
    {
        // 查询所有剩余的记录，按 ID 排序
        var query = "SELECT Id FROM Employee ORDER BY Id";
        var command = new SQLiteCommand(query, connection);

        // 获取所有 ID，按顺序更新
        List<int> ids = new List<int>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
        }


        try
        {
            int newId = 1;
            foreach (var currentId in ids)
            {
                // 更新记录的 ID
                if (currentId != newId)
                {
                    var updateCommand = new SQLiteCommand("UPDATE Employee SET Id = @NewId WHERE Id = @CurrentId", connection);
                    updateCommand.Parameters.AddWithValue("@NewId", newId);
                    updateCommand.Parameters.AddWithValue("@CurrentId", currentId);
                    updateCommand.ExecuteNonQuery();

                }
                newId++;
            }
        }

        catch (Exception ex)
        {

            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}








