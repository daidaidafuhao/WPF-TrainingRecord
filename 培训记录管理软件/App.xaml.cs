using System.Configuration;
using System.Data;
using System.Windows;
using TrainingRecordManager;

namespace 培训记录管理软件;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // 启动UDP广播服务
        UdpBroadcastManager.Instance.StartDiscovery();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        // 停止UDP广播服务
        UdpBroadcastManager.Instance.StopDiscovery();
    }
}

