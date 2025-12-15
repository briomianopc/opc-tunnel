# OPC Tunnel GUI 使用指南

## 系统托盘功能

### 最小化到托盘
点击窗口右上角的 **X** 按钮，程序会最小化到系统托盘，而不是退出。

### 托盘图标操作
- **单击托盘图标**: 显示主界面
- **右键托盘图标**: 打开快捷菜单

### 托盘右键菜单
- **显示主界面**: 恢复窗口显示
- **系统代理**: 快速开启/关闭系统代理
- **TUN 模式**: 快速开启/关闭 TUN 模式
- **退出**: 完全退出程序

## 配置持久化

### 自动保存
- 每次点击"连接"按钮时，配置会自动保存
- 配置文件位置: `%AppData%\OpcTunnelGUI\config.json`

### 自动加载
- 程序启动时自动加载上次保存的配置
- 如果没有保存的配置，使用默认值

### 手动管理配置
配置文件是标准的 JSON 格式，可以手动编辑：

```json
{
  "ListenAddress": "127.0.0.1:30000",
  "ServerAddress": "your-worker.workers.dev:443",
  "ServerIP": "104.16.132.229",
  "Token": "your-uuid-token",
  "DnsServer": "dns.alidns.com/dns-query",
  "EchDomain": "cloudflare-ech.com",
  "UseFallback": false,
  "NumConnections": 1,
  "TransportMode": "ws",
  "EnableTunMode": false,
  "TunIP": "10.0.85.2",
  "TunGateway": "10.0.85.1",
  "TunMask": "255.255.255.0",
  "TunDNS": "1.1.1.1",
  "EnableSystemProxy": false
}
```

## CDN 优选 IP 功能

### 什么是优选 IP？
优选 IP 是指通过测速或其他方式找到的、连接速度最快的 Cloudflare CDN 边缘节点 IP。

### 为什么需要优选 IP？
1. **绕过 DNS 污染**: 直接使用 IP 连接，避免 DNS 解析问题
2. **提高连接速度**: 选择延迟最低的节点
3. **提高稳定性**: 避免某些节点被限速或阻断

### 如何使用优选 IP？

#### 方式 1: 使用优选 IP 地址
1. 点击"显示高级设置"
2. 在"优选 IP"框中输入 IP 地址
3. 示例: `104.16.132.229`

#### 方式 2: 使用 CNAME 域名
1. 点击"显示高级设置"
2. 在"优选 IP"框中输入 CNAME 域名
3. 示例: `cdn.example.com`

### 优选 IP 获取方法

#### 方法 1: 使用测速工具
```bash
# CloudflareSpeedTest
./CloudflareST.exe

# 选择延迟最低的 IP
```

#### 方法 2: 手动测试
```bash
# 使用 ping 测试延迟
ping 104.16.132.229

# 使用 curl 测试连接
curl -I https://your-worker.workers.dev --resolve your-worker.workers.dev:443:104.16.132.229
```

#### 方法 3: 使用在线工具
- https://www.cloudflare.com/cdn-cgi/trace
- https://speed.cloudflare.com/

### 常用优选 IP 段
```
104.16.0.0/12
104.17.0.0/16
104.18.0.0/16
104.19.0.0/16
104.20.0.0/16
104.21.0.0/16
```

### CNAME 域名使用场景
当你有自己的域名并配置了 CNAME 到 Cloudflare Workers 时：

1. 在 DNS 中添加 CNAME 记录:
   ```
   cdn.yourdomain.com -> your-worker.workers.dev
   ```

2. 在 GUI 中配置:
   - 服务器地址: `your-worker.workers.dev:443`
   - 优选 IP: `cdn.yourdomain.com`

## 中文日志显示

### 编码问题
程序已自动处理中文编码，日志中的中文会正确显示。

### 日志示例
```
[13:50:32] [HTTP-CONNECT] 127.0.0.1:56744 -> region1.google-analytics.com:443
[13:50:33] [代理] 127.0.0.1:56744 已连接: region1.google-analytics.com:443
[13:50:41] [代理] 127.0.0.1:61237 已连接: monospace-pa.clients6.google.com:443
```

### 日志类型
- `[HTTP-CONNECT]`: HTTP CONNECT 请求
- `[HTTP-GET]`: HTTP GET 请求
- `[代理]`: 代理连接状态
- `[启动]`: 程序启动信息
- `[ECH]`: ECH 配置信息
- `[传输层]`: 传输层信息
- `[错误]`: 错误信息

## 系统代理配置

### 自动配置
勾选"自动设置系统代理"选项，程序会自动配置 Windows 系统代理。

### 手动配置
如果不使用自动配置，可以手动设置：

#### Windows
1. 打开"设置" -> "网络和 Internet" -> "代理"
2. 启用"使用代理服务器"
3. 地址: `127.0.0.1`
4. 端口: `30000`（或你配置的端口）

#### 浏览器
大多数浏览器会使用系统代理设置，也可以单独配置：

**Chrome/Edge**:
1. 设置 -> 系统 -> 打开代理设置
2. 按照 Windows 步骤配置

**Firefox**:
1. 设置 -> 网络设置
2. 手动代理配置
3. SOCKS5: `127.0.0.1:30000`

## TUN 模式

### 什么是 TUN 模式？
TUN 模式创建一个虚拟网卡，捕获所有网络流量，实现全局透明代理。

### 使用 TUN 模式

#### 前置要求
- **管理员权限**: 必须以管理员身份运行程序
- **wintun.dll**: 确保 wintun.dll 在程序目录中

#### 启用步骤
1. 以管理员身份运行 GUI
2. 点击"显示高级设置"
3. 勾选"启用 TUN 模式"
4. 点击"连接"

#### TUN 模式配置
- **TUN IP**: 虚拟网卡 IP 地址（默认 10.0.85.2）
- **TUN 网关**: 网关地址（默认 10.0.85.1）
- **TUN 子网掩码**: 子网掩码（默认 255.255.255.0）
- **TUN DNS**: DNS 服务器（默认 1.1.1.1）

### TUN 模式 vs 系统代理

| 特性 | TUN 模式 | 系统代理 |
|------|---------|---------|
| 权限要求 | 管理员 | 普通用户 |
| 流量捕获 | 全部流量 | 仅支持代理的应用 |
| 配置复杂度 | 自动 | 需要配置应用 |
| 性能 | 略低 | 较高 |
| 兼容性 | 所有应用 | 部分应用 |

## 传输模式选择

### WebSocket (ws)
- **优点**: 兼容性好，适合大多数场景
- **缺点**: 性能略低于 gRPC
- **适用场景**: 
  - Cloudflare Workers 部署
  - 需要穿透防火墙
  - 网络环境复杂

### gRPC
- **优点**: 性能更好，延迟更低
- **缺点**: 需要专门的 gRPC 服务器
- **适用场景**:
  - 自建服务器
  - 高性能要求
  - 稳定网络环境

## 故障排查

### 问题 1: 中文乱码
**已修复**: v1.1.0 版本已自动处理中文编码。

### 问题 2: 程序自动关闭
**已修复**: 点击 X 按钮会最小化到托盘，不会退出程序。

要完全退出程序：
1. 右键托盘图标
2. 选择"退出"

### 问题 3: 配置丢失
**已修复**: 配置会自动保存到 `%AppData%\OpcTunnelGUI\config.json`。

如果配置丢失：
1. 检查配置文件是否存在
2. 检查文件权限
3. 手动创建配置文件

### 问题 4: 优选 IP 不生效
检查以下几点：
1. IP 地址格式是否正确
2. IP 是否可达（使用 ping 测试）
3. 服务器地址是否正确
4. 查看日志中的连接信息

### 问题 5: 托盘图标不显示
可能原因：
1. 系统托盘设置隐藏了图标
2. 程序未正确初始化

解决方法：
1. 检查 Windows 任务栏设置
2. 重启程序

## 高级技巧

### 技巧 1: 多配置管理
虽然 GUI 暂不支持多配置，但可以手动管理：

1. 复制配置文件:
   ```
   %AppData%\OpcTunnelGUI\config.json
   ```

2. 创建多个配置文件:
   ```
   config-home.json
   config-work.json
   config-mobile.json
   ```

3. 需要切换时，复制对应文件为 `config.json`

### 技巧 2: 命令行启动
虽然有 GUI，但仍可以使用命令行：

```bash
cd Tunnel\OpcTunnelGUI\bin\Release\net8.0
ech-core.exe -l 127.0.0.1:30000 -f your-worker.workers.dev:443 -token your-uuid -ip 104.16.132.229
```

### 技巧 3: 自动启动
将 GUI 添加到 Windows 启动项：

1. 按 `Win + R`
2. 输入 `shell:startup`
3. 创建快捷方式到 `OpcTunnelGUI.exe`

### 技巧 4: 性能优化
1. 使用优选 IP 减少延迟
2. 调整并发连接数（2-5 个）
3. 选择合适的传输模式
4. 使用 TUN 模式减少代理开销

## 安全建议

1. **保护配置文件**: 配置文件包含敏感信息（Token），注意保护
2. **定期更换 Token**: 定期更换身份令牌
3. **使用 ECH**: 启用 ECH 提高隐私保护
4. **检查日志**: 定期查看日志发现异常连接
5. **及时更新**: 保持程序更新到最新版本

## 获取帮助

- 📖 [完整文档](README.md)
- 🚀 [快速开始](QUICKSTART.md)
- 🔧 [功能详解](FEATURES.md)
- 📝 [更新日志](CHANGELOG.md)
- 🐛 [问题反馈](https://github.com/briomianopc/opc-tunnel/issues)
