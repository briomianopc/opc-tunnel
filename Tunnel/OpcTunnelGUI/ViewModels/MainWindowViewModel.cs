using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpcTunnelGUI.Models;
using OpcTunnelGUI.Services;

namespace OpcTunnelGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly TunnelService _tunnelService;
    private readonly ConfigService _configService;

    [ObservableProperty]
    private TunnelConfig _config = new();

    [ObservableProperty]
    private ConnectionStatus _status = new();

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private string _statusText = "未连接";

    [ObservableProperty]
    private string _statusColor = "#FF9E9E9E";

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    [ObservableProperty]
    private bool _showAdvancedSettings;

    public MainWindowViewModel()
    {
        _tunnelService = new TunnelService();
        _configService = new ConfigService();
        
        _tunnelService.OutputReceived += OnOutputReceived;
        _tunnelService.ErrorReceived += OnErrorReceived;
        _tunnelService.StatusChanged += OnStatusChanged;

        _ = LoadConfigAsync();
    }

    private async Task LoadConfigAsync()
    {
        var savedConfig = await _configService.LoadConfigAsync();
        if (savedConfig != null)
        {
            Config = savedConfig;
            AddLog($"已加载配置: {_configService.GetConfigPath()}");
        }
        else
        {
            Config.ServerAddress = "your-worker.workers.dev:443";
            Config.Token = "your-uuid-token";
        }
    }

    private async Task SaveConfigAsync()
    {
        await _configService.SaveConfigAsync(Config);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(Config.ServerAddress))
        {
            AddLog("错误: 请输入服务器地址");
            return;
        }

        if (string.IsNullOrWhiteSpace(Config.Token))
        {
            AddLog("错误: 请输入身份令牌");
            return;
        }

        IsConnecting = true;
        AddLog("正在启动隧道...");

        await SaveConfigAsync();

        var success = await _tunnelService.StartAsync(Config);
        
        if (!success)
        {
            IsConnecting = false;
            AddLog("启动失败");
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        AddLog("正在断开连接...");
        await _tunnelService.StopAsync();
        IsConnected = false;
        IsConnecting = false;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        LogMessages.Clear();
    }

    [RelayCommand]
    private void ToggleAdvancedSettings()
    {
        ShowAdvancedSettings = !ShowAdvancedSettings;
    }

    public void ToggleSystemProxy(bool enable)
    {
        Config.EnableSystemProxy = enable;
        AddLog($"系统代理: {(enable ? "已启用" : "已禁用")}");
    }

    public void ToggleTunMode(bool enable)
    {
        Config.EnableTunMode = enable;
        AddLog($"TUN 模式: {(enable ? "已启用" : "已禁用")}");
    }

    private void OnOutputReceived(object? sender, string message)
    {
        AddLog($"[输出] {message}");
    }

    private void OnErrorReceived(object? sender, string message)
    {
        AddLog($"[错误] {message}");
    }

    private void OnStatusChanged(object? sender, ConnectionStatus status)
    {
        Status = status;
        StatusText = status.Message;

        switch (status.State)
        {
            case ConnectionState.Disconnected:
                IsConnected = false;
                IsConnecting = false;
                StatusColor = "#FF9E9E9E";
                break;
            case ConnectionState.Connecting:
                IsConnected = false;
                IsConnecting = true;
                StatusColor = "#FFFFA500";
                break;
            case ConnectionState.Connected:
                IsConnected = true;
                IsConnecting = false;
                StatusColor = "#FF4CAF50";
                AddLog("✓ 连接成功");
                break;
            case ConnectionState.Error:
                IsConnected = false;
                IsConnecting = false;
                StatusColor = "#FFF44336";
                break;
        }
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogMessages.Add($"[{timestamp}] {message}");

        while (LogMessages.Count > 500)
        {
            LogMessages.RemoveAt(0);
        }
    }
}
