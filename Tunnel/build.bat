@echo off
chcp 65001 >nul
echo === 构建 OPC Tunnel GUI ===

:: 复制 ech-core.exe 到输出目录
echo 正在复制 ech-core.exe...
if exist "..\ech-core.exe" (
    copy /Y "..\ech-core.exe" "OpcTunnelGUI\bin\Debug\net8.0\" >nul
    copy /Y "..\ech-core.exe" "OpcTunnelGUI\bin\Release\net8.0\" >nul 2>nul
    copy /Y "..\wintun.dll" "OpcTunnelGUI\bin\Debug\net8.0\" >nul
    copy /Y "..\wintun.dll" "OpcTunnelGUI\bin\Release\net8.0\" >nul 2>nul
    echo ✓ 已复制核心文件
) else (
    echo ✗ 找不到 ech-core.exe，请先编译核心程序
    pause
    exit /b 1
)

:: 构建 GUI
echo.
echo 正在构建 GUI...
cd OpcTunnelGUI
dotnet build -c Release
if errorlevel 1 (
    echo ✗ 构建失败
    cd ..
    pause
    exit /b 1
)

echo.
echo === 构建成功! ===
echo 输出目录: OpcTunnelGUI\bin\Release\net8.0\
cd ..
pause
