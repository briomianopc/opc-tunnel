using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpcTunnelGUI.Models;

namespace OpcTunnelGUI.Services;

public class TunnelService
{
    private Process? _tunnelProcess;
    private readonly string _executablePath;
    private CancellationTokenSource? _cancellationTokenSource;

    public event EventHandler<string>? OutputReceived;
    public event EventHandler<string>? ErrorReceived;
    public event EventHandler<ConnectionStatus>? StatusChanged;

    public bool IsRunning => _tunnelProcess != null && !_tunnelProcess.HasExited;

    public TunnelService()
    {
        // 查找 ech-core.exe 的路径
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _executablePath = Path.Combine(baseDir, "ech-core.exe");
        
        // 如果在开发环境中，尝试从项目根目录查找
        if (!File.Exists(_executablePath))
        {
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            _executablePath = Path.Combine(projectRoot, "ech-core.exe");
        }
    }

    public Task<bool> StartAsync(TunnelConfig config)
    {
        if (IsRunning)
        {
            return Task.FromResult(false);
        }

        if (!File.Exists(_executablePath))
        {
            ErrorReceived?.Invoke(this, $"找不到可执行文件: {_executablePath}");
            return Task.FromResult(false);
        }

        try
        {
            var arguments = BuildArguments(config);
            
            _tunnelProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _executablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.GetEncoding("GB2312"),
                    StandardErrorEncoding = Encoding.GetEncoding("GB2312")
                }
            };

            _tunnelProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    OutputReceived?.Invoke(this, e.Data);
                    ParseStatusFromOutput(e.Data);
                }
            };

            _tunnelProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    ErrorReceived?.Invoke(this, e.Data);
                }
            };

            _tunnelProcess.Start();
            _tunnelProcess.BeginOutputReadLine();
            _tunnelProcess.BeginErrorReadLine();

            _cancellationTokenSource = new CancellationTokenSource();
            
            // 监控进程状态
            _ = Task.Run(async () =>
            {
                try
                {
                    await _tunnelProcess.WaitForExitAsync(_cancellationTokenSource.Token);
                    StatusChanged?.Invoke(this, new ConnectionStatus
                    {
                        State = ConnectionState.Disconnected,
                        Message = "连接已断开"
                    });
                }
                catch (OperationCanceledException)
                {
                    // 正常取消
                }
            });

            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Connecting,
                Message = "正在连接..."
            });

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this, $"启动失败: {ex.Message}");
            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Error,
                Message = $"错误: {ex.Message}"
            });
            return Task.FromResult(false);
        }
    }

    public async Task StopAsync()
    {
        if (_tunnelProcess == null || _tunnelProcess.HasExited)
        {
            return;
        }

        try
        {
            _cancellationTokenSource?.Cancel();
            
            // 尝试优雅关闭
            _tunnelProcess.StandardInput.WriteLine();
            
            var timeout = Task.Delay(3000);
            var processExit = _tunnelProcess.WaitForExitAsync();
            
            if (await Task.WhenAny(processExit, timeout) == timeout)
            {
                // 超时，强制终止
                _tunnelProcess.Kill(true);
            }

            _tunnelProcess.Dispose();
            _tunnelProcess = null;

            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Disconnected,
                Message = "已断开连接"
            });
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this, $"停止失败: {ex.Message}");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private string BuildArguments(TunnelConfig config)
    {
        var args = new StringBuilder();

        args.Append($"-l {config.ListenAddress} ");
        args.Append($"-f {config.ServerAddress} ");

        if (!string.IsNullOrWhiteSpace(config.ServerIP))
            args.Append($"-ip {config.ServerIP} ");

        if (!string.IsNullOrWhiteSpace(config.Token))
            args.Append($"-token {config.Token} ");

        args.Append($"-dns {config.DnsServer} ");
        args.Append($"-ech {config.EchDomain} ");

        if (config.UseFallback)
            args.Append("-fallback ");

        args.Append($"-n {config.NumConnections} ");
        args.Append($"-mode {config.TransportMode} ");

        if (config.EnableTunMode)
        {
            args.Append("-tun ");
            args.Append($"-tun-ip {config.TunIP} ");
            args.Append($"-tun-gateway {config.TunGateway} ");
            args.Append($"-tun-mask {config.TunMask} ");
            args.Append($"-tun-dns {config.TunDNS} ");
        }

        if (config.EnableSystemProxy)
            args.Append("-sysproxy ");

        return args.ToString().Trim();
    }

    private void ParseStatusFromOutput(string output)
    {
        if (output.Contains("[启动]") || output.Contains("正在连接"))
        {
            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Connecting,
                Message = "正在连接..."
            });
        }
        else if (output.Contains("连接成功") || output.Contains("已启动"))
        {
            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Connected,
                Message = "已连接",
                ConnectedAt = DateTime.Now
            });
        }
        else if (output.Contains("[错误]") || output.Contains("失败"))
        {
            StatusChanged?.Invoke(this, new ConnectionStatus
            {
                State = ConnectionState.Error,
                Message = "连接错误"
            });
        }
    }
}
