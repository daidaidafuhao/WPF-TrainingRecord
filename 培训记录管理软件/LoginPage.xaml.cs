using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using MahApps.Metro.Controls;
using TrainingRecordManager;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace 培训记录管理软件
{
    public partial class LoginPage : MetroWindow
    {
        private readonly ApiClient _apiClient;
        DatabaseManager dbManager = new DatabaseManager();
        public LoginPage()
        {
            InitializeComponent();
            _apiClient = new ApiClient();
            LoadApiSettings();
        }

        private void LoadApiSettings()
        {   
            var apiUrl = ApiUrlManager.Instance.ApiUrl;
            if (!string.IsNullOrEmpty(apiUrl))
            {
                ApiUrlTextBox.Text = apiUrl;
            }
        }

        private void ApiSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ApiSettingsFlyout.IsOpen = true;
        }

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestApiButton.IsEnabled = false;
                ApiTestResult.Text = "正在搜索API服务器...";

                var tcs = new TaskCompletionSource<string>();
                
                // 设置事件处理器
                UdpBroadcastManager.Instance.OnApiServerDiscovered += (discoveredUrl) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ApiUrlTextBox.Text = discoveredUrl;
                        ApiTestResult.Text = "已发现API服务器";
                        tcs.TrySetResult(discoveredUrl);
                        ApiUrlManager.Instance.SaveApiUrl(discoveredUrl);
                    });
                };

                // 启动监听服务
                UdpBroadcastManager.Instance.StartDiscovery();
                
                // 发送一次广播消息
                await UdpBroadcastManager.Instance.SendDiscoveryBroadcast();

                // 等待5秒钟接收响应
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    await tcs.Task.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    ApiTestResult.Text = "未发现API服务器";
                }
            }
            catch (Exception ex)
            {
                ApiTestResult.Text = $"搜索失败：{ex.Message}";
            }
            finally
            {
                TestApiButton.IsEnabled = true;
                // 清理事件订阅
                UdpBroadcastManager.Instance.OnApiServerDiscovered = null;
            }
        }

        private void SaveApiButton_Click(object sender, RoutedEventArgs e)
        {
            var apiUrl = ApiUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                ApiTestResult.Text = "请输入API地址";
                return;
            }

            ApiUrlManager.Instance.SaveApiUrl(apiUrl);
            ApiTestResult.Text = "API地址已保存";
            ApiSettingsFlyout.IsOpen = false;
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
                    TokenManager.Instance.SetTokenAndRole(response.Token,response.User.Role);



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
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class User
    {

        public string Role { get; set; }
    }
}