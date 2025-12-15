@echo off
chcp 65001 >nul
echo ========================================
echo    OPC Tunnel 完整构建脚本
echo ========================================
echo.

:: 第一步：编译核心程序
echo [1/3] 编译核心程序 (ech-core.exe)...
echo.
go build -o ech-core.exe -ldflags="-s -w" ech-workers.go
if errorlevel 1 (
    echo.
    echo ✗ 核心程序编译失败！
    pause
    exit /b 1
)
echo ✓ 核心程序编译成功
echo.

:: 第二步：复制依赖文件
echo [2/3] 复制依赖文件到 GUI 目录...
echo.
if not exist "Tunnel\OpcTunnelGUI\bin\Debug\net8.0" mkdir "Tunnel\OpcTunnelGUI\bin\Debug\net8.0"
if not exist "Tunnel\OpcTunnelGUI\bin\Release\net8.0" mkdir "Tunnel\OpcTunnelGUI\bin\Release\net8.0"

copy /Y ech-core.exe Tunnel\OpcTunnelGUI\bin\Debug\net8.0\ >nul
copy /Y ech-core.exe Tunnel\OpcTunnelGUI\bin\Release\net8.0\ >nul
copy /Y wintun.dll Tunnel\OpcTunnelGUI\bin\Debug\net8.0\ >nul
copy /Y wintun.dll Tunnel\OpcTunnelGUI\bin\Release\net8.0\ >nul
echo ✓ 依赖文件复制完成
echo.

:: 第三步：编译 GUI
echo [3/3] 编译 GUI 程序...
echo.
cd Tunnel\OpcTunnelGUI
dotnet build -c Release
if errorlevel 1 (
    cd ..\..
    echo.
    echo ✗ GUI 编译失败！
    pause
    exit /b 1
)
cd ..\..
echo ✓ GUI 编译成功
echo.

:: 完成
echo ========================================
echo    构建完成！
echo ========================================
echo.
echo 输出文件位置:
echo   - 核心程序: ech-core.exe
echo   - GUI 程序: Tunnel\OpcTunnelGUI\bin\Release\net8.0\OpcTunnelGUI.exe
echo.
echo 运行 GUI:
echo   cd Tunnel\OpcTunnelGUI\bin\Release\net8.0
echo   OpcTunnelGUI.exe
echo.
pause
