#!/bin/bash
set -e

echo "=== 构建 OPC Tunnel GUI ==="

# 复制 ech-core.exe 到输出目录
echo "正在复制 ech-core.exe..."
if [ -f "../ech-core.exe" ]; then
    mkdir -p OpcTunnelGUI/bin/Debug/net8.0
    mkdir -p OpcTunnelGUI/bin/Release/net8.0
    cp -f ../ech-core.exe OpcTunnelGUI/bin/Debug/net8.0/ 2>/dev/null || true
    cp -f ../ech-core.exe OpcTunnelGUI/bin/Release/net8.0/ 2>/dev/null || true
    cp -f ../wintun.dll OpcTunnelGUI/bin/Debug/net8.0/ 2>/dev/null || true
    cp -f ../wintun.dll OpcTunnelGUI/bin/Release/net8.0/ 2>/dev/null || true
    echo "✓ 已复制核心文件"
else
    echo "✗ 找不到 ech-core.exe，请先编译核心程序"
    exit 1
fi

# 构建 GUI
echo ""
echo "正在构建 GUI..."
cd OpcTunnelGUI
dotnet build -c Release

echo ""
echo "=== 构建成功! ==="
echo "输出目录: OpcTunnelGUI/bin/Release/net8.0/"
