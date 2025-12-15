# GUI 参数与内核参数映射表

## 完整参数对照

本文档详细说明 GUI 界面中的每个配置项如何映射到核心程序 `ech-core.exe` 的命令行参数。

## 基本配置

### 1. 监听地址
- **GUI 字段**: `监听地址`
- **配置属性**: `Config.ListenAddress`
- **内核参数**: `-l`
- **默认值**: `127.0.0.1:30000`
- **说明**: 本地代理服务器监听地址，支持 SOCKS5 和 HTTP 代理
- **示例**: 
  ```bash
  -l 127.0.0.1:30000
  ```

### 2. 服务器地址
- **GUI 字段**: `服务器地址`
- **配置属性**: `Config.ServerAddress`
- **内核参数**: `-f`
- **格式**: 
  - WebSocket: `domain:port` (例如: `your-worker.workers.dev:443`)
  - gRPC: `grpc://host:port` (例如: `grpc://your-server:50051`)
- **说明**: 远程服务器地址
- **示例**:
  ```bash
  -f your-worker.workers.dev:443
  -f grpc://your-server:50051
  ```

### 3. 身份令牌
- **GUI 字段**: `身份令牌`
- **配置属性**: `Config.Token`
- **内核参数**: `-token`
- **格式**: UUID 字符串
- **说明**: 
  - WebSocket 模式: 用于 Cloudflare Workers 身份验证
  - gRPC 模式: 作为 metadata 传递的 UUID
- **示例**:
  ```bash
  -token d342d11e-d424-4583-b36e-524ab1f0afa4
  ```

### 4. 传输模式
- **GUI 字段**: `传输模式` (下拉选择)
- **配置属性**: `Config.TransportMode`
- **内核参数**: `-mode`
- **可选值**: 
  - `ws` - WebSocket 模式
  - `grpc` - gRPC 模式
- **默认值**: `ws`
- **说明**: 选择与服务器通信的协议
- **示例**:
  ```bash
  -mode ws
  -mode grpc
  ```

### 5. 并发连接数
- **GUI 字段**: `并发连接数` (数字输入框)
- **配置属性**: `Config.NumConnections`
- **内核参数**: `-n`
- **范围**: 1-10
- **默认值**: `1`
- **说明**: 同时建立的连接数，提高并发性能
- **示例**:
  ```bash
  -n 1
  -n 5
  ```

## 高级配置

### 6. 优选 IP (CDN 加速)
- **GUI 字段**: `优选 IP` (高级设置)
- **配置属性**: `Config.ServerIP`
- **内核参数**: `-ip`
- **格式**: 
  - IP 地址: `104.16.132.229`
  - CNAME 域名: `cdn.example.com`
- **说明**: 
  - 指定 Cloudflare CDN 边缘节点 IP
  - 绕过 DNS 解析，直接连接优选节点
  - 支持使用 CNAME 域名
  - 用于加速连接和绕过 DNS 污染
- **示例**:
  ```bash
  -ip 104.16.132.229
  -ip cdn.example.com
  ```

### 7. DNS 服务器
- **GUI 字段**: `DNS 服务器` (高级设置)
- **配置属性**: `Config.DnsServer`
- **内核参数**: `-dns`
- **默认值**: `dns.alidns.com/dns-query`
- **说明**: DoH (DNS over HTTPS) 服务器，用于查询 ECH 配置
- **常用值**:
  - `dns.alidns.com/dns-query` (阿里 DNS)
  - `dns.google/dns-query` (Google DNS)
  - `cloudflare-dns.com/dns-query` (Cloudflare DNS)
- **示例**:
  ```bash
  -dns dns.alidns.com/dns-query
  ```

### 8. ECH 域名
- **GUI 字段**: `ECH 域名` (高级设置)
- **配置属性**: `Config.EchDomain`
- **内核参数**: `-ech`
- **默认值**: `cloudflare-ech.com`
- **说明**: 用于查询 ECH 配置的域名
- **示例**:
  ```bash
  -ech cloudflare-ech.com
  ```

### 9. 禁用 ECH (Fallback 模式)
- **GUI 字段**: `禁用 ECH (Fallback 模式)` (复选框)
- **配置属性**: `Config.UseFallback`
- **内核参数**: `-fallback`
- **类型**: 布尔值
- **默认值**: `false` (不禁用)
- **说明**: 
  - 勾选后使用普通 TLS 1.3 连接
  - 不使用 ECH 加密 SNI
  - 适用于不支持 ECH 的环境
- **示例**:
  ```bash
  -fallback  # 禁用 ECH
  # 不传递参数则启用 ECH
  ```

### 10. 启用 TUN 模式
- **GUI 字段**: `启用 TUN 模式` (复选框)
- **配置属性**: `Config.EnableTunMode`
- **内核参数**: `-tun`
- **类型**: 布尔值
- **默认值**: `false`
- **说明**: 
  - 创建虚拟网卡，全局透明代理
  - 需要管理员权限
  - 需要 wintun.dll
- **示例**:
  ```bash
  -tun  # 启用 TUN 模式
  # 不传递参数则不启用
  ```

### 11. TUN IP 地址
- **GUI 字段**: `TUN IP` (高级设置，TUN 模式下)
- **配置属性**: `Config.TunIP`
- **内核参数**: `-tun-ip`
- **默认值**: `10.0.85.2`
- **说明**: 虚拟网卡的 IP 地址
- **示例**:
  ```bash
  -tun-ip 10.0.85.2
  ```

### 12. TUN 网关地址
- **GUI 字段**: `TUN 网关` (高级设置，TUN 模式下)
- **配置属性**: `Config.TunGateway`
- **内核参数**: `-tun-gateway`
- **默认值**: `10.0.85.1`
- **说明**: 虚拟网卡的网关地址
- **示例**:
  ```bash
  -tun-gateway 10.0.85.1
  ```

### 13. TUN 子网掩码
- **GUI 字段**: `TUN 子网掩码` (高级设置，TUN 模式下)
- **配置属性**: `Config.TunMask`
- **内核参数**: `-tun-mask`
- **默认值**: `255.255.255.0`
- **说明**: 虚拟网卡的子网掩码
- **示例**:
  ```bash
  -tun-mask 255.255.255.0
  ```

### 14. TUN DNS 服务器
- **GUI 字段**: `TUN DNS` (高级设置，TUN 模式下)
- **配置属性**: `Config.TunDNS`
- **内核参数**: `-tun-dns`
- **默认值**: `1.1.1.1`
- **说明**: TUN 模式下使用的 DNS 服务器
- **示例**:
  ```bash
  -tun-dns 1.1.1.1
  ```

### 15. 自动设置系统代理
- **GUI 字段**: `自动设置系统代理` (复选框)
- **配置属性**: `Config.EnableSystemProxy`
- **内核参数**: `-sysproxy`
- **类型**: 布尔值
- **默认值**: `false`
- **说明**: 
  - 自动配置 Windows 系统代理
  - 程序退出时自动恢复
  - TUN 模式下此选项被忽略
- **示例**:
  ```bash
  -sysproxy  # 启用系统代理
  # 不传递参数则不启用
  ```

## 完整命令行示例

### 示例 1: 基本 WebSocket 连接
**GUI 配置**:
```
监听地址: 127.0.0.1:30000
服务器地址: your-worker.workers.dev:443
身份令牌: d342d11e-d424-4583-b36e-524ab1f0afa4
传输模式: WebSocket (ws)
并发连接数: 1
```

**生成的命令行**:
```bash
ech-core.exe -l 127.0.0.1:30000 -f your-worker.workers.dev:443 -token d342d11e-d424-4583-b36e-524ab1f0afa4 -dns dns.alidns.com/dns-query -ech cloudflare-ech.com -n 1 -mode ws
```

### 示例 2: 使用优选 IP 的 WebSocket 连接
**GUI 配置**:
```
监听地址: 127.0.0.1:30000
服务器地址: your-worker.workers.dev:443
身份令牌: d342d11e-d424-4583-b36e-524ab1f0afa4
传输模式: WebSocket (ws)
并发连接数: 2
优选 IP: 104.16.132.229  (高级设置)
```

**生成的命令行**:
```bash
ech-core.exe -l 127.0.0.1:30000 -f your-worker.workers.dev:443 -ip 104.16.132.229 -token d342d11e-d424-4583-b36e-524ab1f0afa4 -dns dns.alidns.com/dns-query -ech cloudflare-ech.com -n 2 -mode ws
```

### 示例 3: gRPC 模式连接
**GUI 配置**:
```
监听地址: 127.0.0.1:30000
服务器地址: grpc://your-server:50051
身份令牌: your-uuid-token
传输模式: gRPC
并发连接数: 3
```

**生成的命令行**:
```bash
ech-core.exe -l 127.0.0.1:30000 -f grpc://your-server:50051 -token your-uuid-token -dns dns.alidns.com/dns-query -ech cloudflare-ech.com -n 3 -mode grpc
```

### 示例 4: TUN 模式 (全局代理)
**GUI 配置**:
```
服务器地址: your-worker.workers.dev:443
身份令牌: d342d11e-d424-4583-b36e-524ab1f0afa4
传输模式: WebSocket (ws)
启用 TUN 模式: ✓ (高级设置)
TUN IP: 10.0.85.2
TUN 网关: 10.0.85.1
TUN 子网掩码: 255.255.255.0
TUN DNS: 1.1.1.1
```

**生成的命令行**:
```bash
ech-core.exe -l 127.0.0.1:30000 -f your-worker.workers.dev:443 -token d342d11e-d424-4583-b36e-524ab1f0afa4 -dns dns.alidns.com/dns-query -ech cloudflare-ech.com -n 1 -mode ws -tun -tun-ip 10.0.85.2 -tun-gateway 10.0.85.1 -tun-mask 255.255.255.0 -tun-dns 1.1.1.1
```

### 示例 5: 禁用 ECH + 系统代理
**GUI 配置**:
```
监听地址: 127.0.0.1:30000
服务器地址: your-worker.workers.dev:443
身份令牌: d342d11e-d424-4583-b36e-524ab1f0afa4
传输模式: WebSocket (ws)
禁用 ECH: ✓ (高级设置)
自动设置系统代理: ✓ (高级设置)
```

**生成的命令行**:
```bash
ech-core.exe -l 127.0.0.1:30000 -f your-worker.workers.dev:443 -token d342d11e-d424-4583-b36e-524ab1f0afa4 -dns dns.alidns.com/dns-query -ech cloudflare-ech.com -fallback -n 1 -mode ws -sysproxy
```

## 参数构建逻辑

GUI 中的参数构建逻辑位于 `Services/TunnelService.cs` 的 `BuildArguments` 方法：

```csharp
private string BuildArguments(TunnelConfig config)
{
    var args = new StringBuilder();

    // 基本参数（必需）
    args.Append($"-l {config.ListenAddress} ");
    args.Append($"-f {config.ServerAddress} ");

    // 优选 IP（可选）
    if (!string.IsNullOrWhiteSpace(config.ServerIP))
        args.Append($"-ip {config.ServerIP} ");

    // 身份令牌（可选但推荐）
    if (!string.IsNullOrWhiteSpace(config.Token))
        args.Append($"-token {config.Token} ");

    // DNS 和 ECH 配置
    args.Append($"-dns {config.DnsServer} ");
    args.Append($"-ech {config.EchDomain} ");

    // ECH Fallback
    if (config.UseFallback)
        args.Append("-fallback ");

    // 并发连接数
    args.Append($"-n {config.NumConnections} ");
    
    // 传输模式
    args.Append($"-mode {config.TransportMode} ");

    // TUN 模式
    if (config.EnableTunMode)
    {
        args.Append("-tun ");
        args.Append($"-tun-ip {config.TunIP} ");
        args.Append($"-tun-gateway {config.TunGateway} ");
        args.Append($"-tun-mask {config.TunMask} ");
        args.Append($"-tun-dns {config.TunDNS} ");
    }

    // 系统代理
    if (config.EnableSystemProxy)
        args.Append("-sysproxy ");

    return args.ToString().Trim();
}
```

## 参数验证

### 必需参数
- `-l` (监听地址)
- `-f` (服务器地址)

### 推荐参数
- `-token` (身份令牌) - 用于身份验证

### 可选参数
所有其他参数都是可选的，有合理的默认值。

## 特殊说明

### 1. 优选 IP 的作用
优选 IP (`-ip`) 参数是 CDN 加速的关键：
- **绕过 DNS**: 直接使用 IP 连接，避免 DNS 污染
- **加速连接**: 选择延迟最低的 Cloudflare 边缘节点
- **提高稳定性**: 避免某些节点被限速或阻断

### 2. TUN 模式 vs 系统代理
- **TUN 模式**: 
  - 需要管理员权限
  - 捕获所有网络流量
  - 无需配置应用程序
  - 性能略低
  
- **系统代理**:
  - 普通用户权限
  - 仅支持代理的应用
  - 需要应用程序支持代理
  - 性能较高

### 3. ECH 的重要性
ECH (Encrypted Client Hello) 用于加密 TLS 握手中的 SNI 信息：
- **隐私保护**: 防止 SNI 泄露访问的域名
- **绕过审查**: 某些网络审查依赖 SNI 识别
- **推荐启用**: 除非遇到兼容性问题

### 4. 传输模式选择
- **WebSocket (ws)**:
  - 兼容性好
  - 适合 Cloudflare Workers
  - 可穿透大多数防火墙
  
- **gRPC**:
  - 性能更好
  - 延迟更低
  - 需要专门的 gRPC 服务器

## 配置文件格式

GUI 保存的配置文件 (`%AppData%\OpcTunnelGUI\config.json`) 格式：

```json
{
  "ListenAddress": "127.0.0.1:30000",
  "ServerAddress": "your-worker.workers.dev:443",
  "ServerIP": "104.16.132.229",
  "Token": "d342d11e-d424-4583-b36e-524ab1f0afa4",
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

## 故障排查

### 参数不生效
1. 检查 GUI 中的配置是否正确
2. 查看日志中的实际命令行
3. 验证参数格式是否正确

### 优选 IP 不工作
1. 确认 IP 地址可达 (ping 测试)
2. 确认服务器地址正确
3. 查看日志中的连接信息

### TUN 模式启动失败
1. 确认以管理员身份运行
2. 确认 wintun.dll 存在
3. 检查 TUN 配置参数

## 参考资料

- [核心程序源码](../ech-workers.go) - 查看所有参数定义
- [GUI 使用指南](USAGE.md) - 详细使用说明
- [快速开始](QUICKSTART.md) - 快速上手指南
