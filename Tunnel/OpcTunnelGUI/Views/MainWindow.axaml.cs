using System;
using System.ComponentModel;
using Avalonia.Controls;
using OpcTunnelGUI.Services;
using OpcTunnelGUI.ViewModels;

namespace OpcTunnelGUI.Views;

public partial class MainWindow : Window
{
    private TrayIconService? _trayIconService;
    private bool _isClosing = false;

    public MainWindow()
    {
        InitializeComponent();
        
        Closing += OnClosing;
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            _trayIconService = new TrayIconService(
                showMainWindow: ShowMainWindow,
                exitApplication: ExitApplication,
                toggleSystemProxy: viewModel.ToggleSystemProxy,
                toggleTunMode: viewModel.ToggleTunMode
            );
            _trayIconService.Initialize();
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_isClosing)
        {
            e.Cancel = true;
            Hide();
            _trayIconService?.Show();
        }
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _isClosing = true;
        _trayIconService?.Dispose();
        
        if (DataContext is MainWindowViewModel viewModel)
        {
            _ = viewModel.DisconnectCommand.ExecuteAsync(null);
        }
        
        Close();
    }
}
