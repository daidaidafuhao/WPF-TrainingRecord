using System;
using System.IO;

public class TemplateManager
{
    private static readonly string TemplateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
    private static readonly string DefaultTemplateName = "导出模板.docx";

    public static string DefaultTemplatePath => Path.Combine(TemplateDirectory, DefaultTemplateName);

    public static void Initialize()
    {
        try
        {
            // 确保模板目录存在
            if (!Directory.Exists(TemplateDirectory))
            {
                Directory.CreateDirectory(TemplateDirectory);
            }

            // 检查默认模板是否存在
            if (!File.Exists(DefaultTemplatePath))
            {
                // 从原始位置复制模板文件
                string sourceTemplatePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "模板",
                    DefaultTemplateName);

                if (File.Exists(sourceTemplatePath))
                {
                    File.Copy(sourceTemplatePath, DefaultTemplatePath, true);
                }
                else
                {
                    throw new FileNotFoundException("默认模板文件未找到", sourceTemplatePath);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"初始化模板管理器时发生错误: {ex.Message}", ex);
        }
    }

    public static bool ValidateTemplate()
    {
        return File.Exists(DefaultTemplatePath);
    }

    public static string GetTemplatePath()
    {
        if (!ValidateTemplate())
        {
            throw new FileNotFoundException("模板文件不存在", DefaultTemplatePath);
        }
        return DefaultTemplatePath;
    }
}