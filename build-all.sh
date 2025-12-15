#!/bin/bash
set -e

echo "========================================"
echo "   OPC Tunnel 完整构建脚本"
echo "========================================"
echo ""

# 第一步：编译核心程序
echo "[1/3] 编译核心程序 (ech-core.exe)..."
echo ""
GOOS=windows GOARCH=amd64 go build -o ech-core.exe -ldflags="-s -w" ech-workers.go
echo "✓ 核心程序编译成功"
echo ""

# 第二步：复制依赖文件
echo "[2/3] 复制依赖文件到 GUI 目录..."
echo ""
mkdir -p Tunnel/OpcTunnelGUI/bin/Debug/net8.0
mkdir -p Tunnel/OpcTunnelGUI/bin/Release/net8.0

cp -f ech-core.exe Tunnel/OpcTunnelGUI/bin/Debug/net8.0/
cp -f ech-core.exe Tunnel/OpcTunnelGUI/bin/Release/net8.0/
cp -f wintun.dll Tunnel/OpcTunnelGUI/bin/Debug/net8.0/ 2>/dev/null || true
cp -f wintun.dll Tunnel/OpcTunnelGUI/bin/Release/net8.0/ 2>/dev/null || true
echo "✓ 依赖文件复制完成"
echo ""

# 第三步：编译 GUI
echo "[3/3] 编译 GUI 程序..."
echo ""
cd Tunnel/OpcTunnelGUI
dotnet build -c Release
cd ../..
echo "✓ GUI 编译成功"
echo ""

# 完成
echo "========================================"
echo "   构建完成！"
echo "========================================"
echo ""
echo "输出文件位置:"
echo "  - 核心程序: ech-core.exe"
echo "  - GUI 程序: Tunnel/OpcTunnelGUI/bin/Release/net8.0/OpcTunnelGUI"
echo ""
echo "运行 GUI:"
echo "  cd Tunnel/OpcTunnelGUI/bin/Release/net8.0"
echo "  ./OpcTunnelGUI"
echo ""
