﻿public class Employee
{
    public int Index { get; set; }
    public string Name { get; set; }        // 姓名
    public string IDCardNumber { get; set; } // 身份证号
    public string Photo { get; set; }        // 照片
    public string Education { get; set; }    // 学历
    public string Title { get; set; }        // 职称
    public string Level { get; set; }        // 等级
    public string LevelJobType { get; set; } // 工种
    public string Position { get; set; }     // 岗位
    public string UnitName { get; set; }     // 单位名称
    public DateTime RuzhiDate { get; set; } // 入职时间
    public string
        SchoolName
    { get; set; }     // 毕业院校
    public string ZhuanYe { get; set; }     // 所学专业
}

public class TrainingRecord
{
    public int Id { get; set; } // 主键
    public string EmployeeId { get; set; } // 外键，关联 Employee
    public int SerialNumber { get; set; } // 序号
    public DateTime TrainingDate { get; set; } // 培训时间
    public string TrainingContent { get; set; } // 培训内容
    public string TrainingUnit { get; set; } // 培训单位
    public string TrainingLocation { get; set; } // 培训地点
    public string Assessment { get; set; } // 成绩/考核
    public decimal Cost { get; set; } // 费用
    public string Remarks { get; set; } // 备注
}

public class PersonnelInfo
{
    public string UnitName { get; set; } // 单位名称
    public string Name { get; set; } // 姓名
    public string IDCard { get; set; } // 身份证
    public DateTime EmploymentDate { get; set; } // 入职时间
    public string GraduatedSchool { get; set; } // 毕业院校
    public string Major { get; set; } // 所学专业
    public string Title { get; set; } // 职称
    public string Level { get; set; } // 等级级别
    public string JobType { get; set; } // 工种
}

public class TrainingInfo
{
    public string Name { get; set; } // 姓名
    public string IDCard { get; set; } // 身份证
    public DateTime TrainingDate { get; set; } // 培训时间
    public string TrainingLocation { get; set; } // 培训地点
    public string TrainingUnit { get; set; } // 培训单位
    public string TrainingContent { get; set; } // 培训内容
    public decimal Cost { get; set; } // 费用
    public string Remarks { get; set; } // 备注
}
