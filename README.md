# 档案管理系统

这是一个基于WPF开发的档案管理系统，主要用于培训记录的管理、查询和导出。

## 目录结构

```
培训记录管理软件/
├── API/                    # API接口相关文件
│   ├── ApiClient.cs        # API客户端实现
│   └── TokenManager.cs     # Token管理器
├── SQL/                    # 数据库相关文件
│   └── DatabaseManager.cs  # 数据库管理器
├── 对象/                   # 数据模型
│   └── 对象.cs            # 数据对象定义
├── 模板/                   # 导出模板
│   ├── Class1.cs          # 模板处理类
│   └── 导出全部文件.cs     # 文件导出实现
└── UI页面
    ├── LoginPage.xaml     # 登录页面
    ├── HomePage.xaml      # 主页
    ├── QueryPage.xaml     # 查询页面
    ├── InputExecl.xaml    # Excel导入页面
    └── SummaryPage.xaml   # 统计页面
```

## 环境要求

- .NET 8.0
- Windows 7 或更高版本
- Visual Studio 2022 或更高版本

### NuGet包依赖

- DocumentFormat.OpenXml (3.1.1)
- EPPlus.Core.CoreCompat (1.5.6)
- MahApps.Metro (2.4.10)
- Simplify.Windows.Forms (1.1.3)
- System.Data.SQLite (1.0.119)
- System.Drawing.Common (8.0.10)

## 编译步骤

1. 克隆项目到本地：
   ```
   git clone <repository-url>
   cd 档案管理
   ```

2. 使用Visual Studio打开解决方案文件 `档案管理.sln`

3. 还原NuGet包：
   - 在Visual Studio中右键解决方案
   - 选择"还原NuGet包"

4. 编译项目：
   - 选择"Release"配置
   - 点击"生成解决方案"或按F6

5. 运行程序：
   - 按F5启动调试
   - 或在bin/Release目录下找到可执行文件运行

## API服务器配置

1. 在`API/ApiClient.cs`中配置API服务器地址
2. 确保TokenManager.cs中的认证配置正确

## 注意事项

- 首次运行前请确保数据库配置正确
- 导出功能需要正确配置模板文件
- 建议使用管理员权限运行程序