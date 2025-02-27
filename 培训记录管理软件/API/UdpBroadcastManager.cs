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
                var errorMessage = ex.SocketErrorCode == SocketError.AccessDenied
                    ? $"UDP广播管理器初始化失败: 没有足够的权限绑定端口 {BROADCAST_PORT}。请尝试以管理员身份运行程序。"
                    : $"UDP广播管理器初始化失败: {ex.Message}\n请检查端口 {BROADCAST_PORT} 是否被占用";
                Console.WriteLine(errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
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
                    Console.WriteLine("等待接收UDP响应消息...");
                    var result = await _udpClient.ReceiveAsync();
                    var response = Encoding.UTF8.GetString(result.Buffer);
                    Console.WriteLine($"收到UDP消息: {response} 来自: {result.RemoteEndPoint}");

                    // 过滤掉自己发出的广播消息
                    if (response == DISCOVERY_MESSAGE)
                    {
                        Console.WriteLine("收到自己发出的广播消息，已忽略");
                        continue;
                    }

                    // 只处理来自服务器的响应消息
                    if (response.StartsWith("Server IP:"))
                    {
                        // 解析服务器IP和端口
                        var parts = response.Split(',');
                        if (parts.Length == 2)
                        {
                            var ip = parts[0].Replace("Server IP:", "").Trim();
                            var port = parts[1].Replace("Port:", "").Trim();

                            // 构造API URL
                            var apiUrl = $"http://{ip}:{port}";
                            if (!string.IsNullOrEmpty(apiUrl))
                            {
                                ApiUrlManager.Instance.SaveApiUrl(apiUrl);
                                Console.WriteLine($"已更新API URL: {apiUrl}");
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消操作
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收响应错误: {ex.Message}");
            }
        }
    }
}