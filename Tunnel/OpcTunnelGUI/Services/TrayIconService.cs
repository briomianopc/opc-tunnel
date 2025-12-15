using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace OpcTunnelGUI.Services;

public class TrayIconService
{
    private TrayIcon? _trayIcon;
    private readonly Action _showMainWindow;
    private readonly Action _exitApplication;
    private readonly Action<bool> _toggleSystemProxy;
    private readonly Action<bool> _toggleTunMode;

    public TrayIconService(
        Action showMainWindow,
        Action exitApplication,
        Action<bool> toggleSystemProxy,
        Action<bool> toggleTunMode)
    {
        _showMainWindow = showMainWindow;
        _exitApplication = exitApplication;
        _toggleSystemProxy = toggleSystemProxy;
        _toggleTunMode = toggleTunMode;
    }

    public void Initialize()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        _trayIcon = new TrayIcon
        {
            ToolTipText = "OPC Tunnel",
            IsVisible = true
        };

        var menu = new NativeMenu();

        var showItem = new NativeMenuItem("显示主界面");
        showItem.Click += (_, _) => _showMainWindow();
        menu.Add(showItem);

        menu.Add(new NativeMenuItemSeparator());

        var sysProxyItem = new NativeMenuItem("系统代理");
        sysProxyItem.Click += (_, _) => _toggleSystemProxy(true);
        menu.Add(sysProxyItem);

        var tunModeItem = new NativeMenuItem("TUN 模式");
        tunModeItem.Click += (_, _) => _toggleTunMode(true);
        menu.Add(tunModeItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("退出");
        exitItem.Click += (_, _) => _exitApplication();
        menu.Add(exitItem);

        _trayIcon.Menu = menu;
        _trayIcon.Clicked += (_, _) => _showMainWindow();

        if (Application.Current != null)
        {
            var trayIcons = new TrayIcons();
            trayIcons.Add(_trayIcon);
            TrayIcon.SetIcons(Application.Current, trayIcons);
        }
    }

    public void Show()
    {
        if (_trayIcon != null)
            _trayIcon.IsVisible = true;
    }

    public void Hide()
    {
        if (_trayIcon != null)
            _trayIcon.IsVisible = false;
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
