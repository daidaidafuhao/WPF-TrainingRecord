using Simplify.Windows.Forms;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrainingRecordManager
{
    public class UdpBroadcastManager
    {
        private static readonly UdpBroadcastManager _instance = new UdpBroadcastManager();
        private readonly UdpClient _udpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private const int BROADCAST_PORT = 45678; // 修改为非特权端口
        private const string DISCOVERY_MESSAGE = "TRAINING_RECORD_MANAGER_DISCOVERY";
        private bool _isRunning;

        private UdpBroadcastManager()
        {
            try
            {
                _udpClient = new UdpClient();
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, BROADCAST_PORT));
                _udpClient.EnableBroadcast = true;
                _isRunning = false;
                Console.WriteLine($"UDP广播管理器已初始化，监听端口: {BROADCAST_PORT}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"UDP广播管理器初始化失败: {ex.Message}，将使用本机地址");
                SetLocalApiUrl();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP广播管理器发生未知错误: {ex.Message}，将使用本机地址");
                SetLocalApiUrl();
            }
        }

        private void SetLocalApiUrl()
        {
            try
            {
                // 获取本机IP地址
                string localIp = GetLocalIPAddress();
                var apiUrl = $"http://{localIp}:5115";
                ApiUrlManager.Instance.SaveApiUrl(apiUrl);
                Console.WriteLine($"已设置为本机API地址: {apiUrl}");
            }
            catch (Exception ex)
            {
                // 如果获取本机IP失败，使用localhost
                var apiUrl = "http://localhost:5115";
                ApiUrlManager.Instance.SaveApiUrl(apiUrl);
                Console.WriteLine($"获取本机IP失败，使用localhost: {apiUrl}");
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        public static UdpBroadcastManager Instance
        {
            get { return _instance; }
        }

        public void StartDiscovery()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(BroadcastLoop, _cancellationTokenSource.Token);
            Task.Run(ListenForResponses, _cancellationTokenSource.Token);
        }

        public void StopDiscovery()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task BroadcastLoop()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var discoveryData = Encoding.UTF8.GetBytes(DISCOVERY_MESSAGE);
                    await _udpClient.SendAsync(discoveryData, discoveryData.Length, 
                        new IPEndPoint(IPAddress.Broadcast, BROADCAST_PORT));

                    await Task.Delay(60000, _cancellationTokenSource.Token); // 60秒间隔
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消操作
            }
            catch (Exception ex)
            {
                Console.WriteLine($"广播错误: {ex.Message}");
            }
        }

        private async Task ListenForResponses()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("等待接收UDP响应消息...");
                        var result = await _udpClient.ReceiveAsync();
                        var response = Encoding.UTF8.GetString(result.Buffer);
                        Console.WriteLine($"收到UDP消息: {response} 来自: {result.RemoteEndPoint}");

                        if (response == DISCOVERY_MESSAGE)
                        {
                            Console.WriteLine("收到自己发出的广播消息，已忽略");
                            continue;
                        }

                        if (response.StartsWith("Server IP:"))
                        {
                            var parts = response.Split(',');
                            if (parts.Length == 2)
                            {
                                var ip = parts[0].Replace("Server IP:", "").Trim();
                                var port = parts[1].Replace("Port:", "").Trim();
                                var apiUrl = $"http://{ip}:{port}";
                                if (!string.IsNullOrEmpty(apiUrl))
                                {
                                    ApiUrlManager.Instance.SaveApiUrl(apiUrl);
                                    Console.WriteLine($"已更新API URL: {apiUrl}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"接收UDP消息时发生错误: {ex.Message}，将使用本机地址");
                        SetLocalApiUrl();
                        await Task.Delay(5000, _cancellationTokenSource.Token); // 等待5秒后继续尝试
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消操作
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP监听服务发生致命错误: {ex.Message}，将使用本机地址");
                SetLocalApiUrl();
            }
        }
    }
}