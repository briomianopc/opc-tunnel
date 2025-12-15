@echo off
chcp 65001 >nul
echo === 启动 OPC Tunnel GUI ===

:: 检查是否已构建
if not exist "OpcTunnelGUI\bin\Debug\net8.0\OpcTunnelGUI.exe" (
    echo 未找到可执行文件，正在构建...
    call build.bat
    if errorlevel 1 exit /b 1
)

:: 运行程序
echo.
echo 正在启动...
cd OpcTunnelGUI\bin\Debug\net8.0
start OpcTunnelGUI.exe
cd ..\..\..
echo.
echo GUI 已启动
