using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using MahApps.Metro.Controls;
using TrainingRecordManager;

namespace 培训记录管理软件
{
    public partial class LoginPage : MetroWindow
    {
        private readonly ApiClient _apiClient;

        public LoginPage()
        {
            InitializeComponent();
            _apiClient = new ApiClient(DatabaseManager.ApiBaseUrl);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginButton.IsEnabled = false;
                ErrorMessage.Text = string.Empty;

                var loginData = new
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordBox.Password
                };

                // 序列化登录数据
                string jsonData = JsonSerializer.Serialize(loginData);

                // 发送登录请求
                var response = await _apiClient.PostAsync<LoginResponse>("/api/Users/login", jsonData);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                   
                    TokenManager.Instance.SetToken(response.Token);
                 

                    // 创建并显示主页窗口
                    var homePage = new HomePage();
                    homePage.Show();
                    
                    // 关闭登录窗口
                    this.Close();
                }
                else
                {
                    ErrorMessage.Text = "登录失败：无效的用户名或密码";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"登录失败：{ex.Message}";
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }
}