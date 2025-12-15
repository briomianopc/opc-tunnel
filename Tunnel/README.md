# OPC Tunnel GUI

基于 Avalonia UI 的跨平台图形界面，用于管理 OPC Tunnel 网络隧道客户端。

## 功能特性

- ✅ 现代化的图形界面
- ✅ 实时日志显示（支持中文）
- ✅ 连接状态监控
- ✅ 支持 WebSocket 和 gRPC 传输模式
- ✅ 高级配置选项（ECH、TUN 模式、系统代理等）
- ✅ 配置自动保存和加载
- ✅ 系统托盘支持（最小化到托盘）
- ✅ 托盘右键菜单（快速访问功能）
- ✅ CDN 优选 IP 和 CNAME 域名支持
- ✅ 跨平台支持（Windows、Linux、macOS）

## 技术栈

- **框架**: .NET 8.0
- **UI**: Avalonia UI 11.3.9
- **架构**: MVVM (Model-View-ViewModel)
- **依赖注入**: CommunityToolkit.Mvvm
- **主题**: Fluent Design + Material Design

## 项目结构

```
OpcTunnelGUI/
├── Models/              # 数据模型
│   ├── TunnelConfig.cs          # 隧道配置
│   └── ConnectionStatus.cs      # 连接状态
├── Services/            # 业务服务
│   └── TunnelService.cs         # 隧道管理服务
├── ViewModels/          # 视图模型
│   ├── ViewModelBase.cs
│   └── MainWindowViewModel.cs   # 主窗口视图模型
├── Views/               # 视图
│   └── MainWindow.axaml         # 主窗口界面
├── Converters/          # 值转换器
│   └── BoolToTextConverter.cs
└── Assets/              # 资源文件
```

## 构建说明

### 前置要求

- .NET 8.0 SDK 或更高版本
- ech-core.exe（核心隧道程序）
- wintun.dll（Windows TUN 驱动）

### Windows 构建

```batch
# 使用构建脚本
build.bat

# 或手动构建
cd OpcTunnelGUI
dotnet build -c Release
```

### Linux/macOS 构建

```bash
# 使用构建脚本
chmod +x build.sh
./build.sh

# 或手动构建
cd OpcTunnelGUI
dotnet build -c Release
```

### 输出目录

构建完成后，可执行文件位于：
```
OpcTunnelGUI/bin/Release/net8.0/
```

## 运行说明

### Windows

```batch
cd OpcTunnelGUI\bin\Release\net8.0
OpcTunnelGUI.exe
```

### Linux/macOS

```bash
cd OpcTunnelGUI/bin/Release/net8.0
./OpcTunnelGUI
```

**注意**: 
- TUN 模式需要管理员/root 权限
- 确保 ech-core.exe 和 wintun.dll 在同一目录

## 使用说明

### 基本配置

1. **服务器地址**: 输入 Cloudflare Workers 地址或 gRPC 服务器地址
   - WebSocket: `your-worker.workers.dev:443`
   - gRPC: `grpc://your-server:50051`

2. **身份令牌**: 输入 UUID 或 Token

3. **监听地址**: 本地代理监听地址（默认 `127.0.0.1:30000`）

4. **传输模式**: 选择 WebSocket 或 gRPC

5. **并发连接数**: 设置并发连接数（1-10）

### 高级设置

点击"显示高级设置"按钮展开：

- **优选 IP**: CDN 优选 IP 或 CNAME 域名（用于加速连接）
  - 支持直接指定 Cloudflare 边缘节点 IP
  - 支持使用 CNAME 域名绕过 DNS 污染
  - 示例: `104.16.132.229` 或 `cdn.example.com`
- **DNS 服务器**: DoH 服务器地址（用于查询 ECH 配置）
- **ECH 域名**: ECH 配置查询域名
- **禁用 ECH**: 使用普通 TLS 模式
- **启用 TUN 模式**: 全局透明代理（需要管理员权限）
- **自动设置系统代理**: 自动配置系统代理设置

### 连接操作

1. 填写必要的配置信息
2. 点击"连接"按钮启动隧道
3. 查看日志窗口了解连接状态
4. 点击"断开"按钮停止隧道

## 界面说明

### 状态指示器

- 🔴 **灰色**: 未连接
- 🟠 **橙色**: 正在连接
- 🟢 **绿色**: 已连接
- 🔴 **红色**: 连接错误

### 日志窗口

实时显示隧道程序的输出信息：
- `[输出]`: 正常输出信息
- `[错误]`: 错误信息
- 时间戳格式: `[HH:mm:ss]`

## 开发说明

### 添加新功能

1. 在 `Models/` 中定义数据模型
2. 在 `Services/` 中实现业务逻辑
3. 在 `ViewModels/` 中创建视图模型
4. 在 `Views/` 中设计界面

### MVVM 模式

使用 CommunityToolkit.Mvvm 简化 MVVM 实现：

```csharp
// 可观察属性
[ObservableProperty]
private string _statusText;

// 命令
[RelayCommand]
private async Task ConnectAsync()
{
    // 实现逻辑
}
```

### 调试

```bash
cd OpcTunnelGUI
dotnet run
```

## 依赖项

- **Avalonia**: 跨平台 UI 框架
- **Avalonia.Desktop**: 桌面平台支持
- **Avalonia.Themes.Fluent**: Fluent 主题
- **Material.Avalonia**: Material Design 主题
- **CommunityToolkit.Mvvm**: MVVM 工具包
- **Avalonia.Controls.DataGrid**: 数据网格控件

## 已知问题

1. Linux/macOS 上 TUN 模式可能需要额外配置
2. 某些 Linux 发行版可能需要安装额外的依赖
3. 日志窗口在大量输出时可能影响性能（已限制 500 条）

## 新功能 (v1.1.0)

- ✅ 配置文件自动保存/加载
- ✅ 系统托盘支持
- ✅ 托盘右键菜单
- ✅ 中文日志正确显示
- ✅ 优选 IP 和 CNAME 支持

## 待实现功能

- [ ] 连接配置预设管理
- [ ] 流量统计图表
- [ ] 自动更新检查
- [ ] 多语言支持
- [ ] 暗色主题切换
- [ ] 自动重连机制

## 许可证

与主项目保持一致

## 贡献

欢迎提交 Issue 和 Pull Request

## 联系方式

项目地址: https://github.com/briomianopc/opc-tunnel
