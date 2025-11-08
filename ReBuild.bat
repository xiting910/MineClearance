@echo off
chcp 65001 >nul
title Clean and Build .NET Project

echo ========================================================
echo                .NET 项目清理和构建脚本
echo ========================================================

echo.
echo [1/3] 正在搜索并删除 bin 文件夹...
for /f "delims=" %%i in ('dir /s /b /a:d "bin" 2^>nul') do (
    echo 删除: %%i
    rmdir /s /q "%%i" 2>nul
)

echo.
echo [2/3] 正在搜索并删除 obj 文件夹...
for /f "delims=" %%i in ('dir /s /b /a:d "obj" 2^>nul') do (
    echo 删除: %%i
    rmdir /s /q "%%i" 2>nul
)

echo.
echo [3/3] 正在执行 dotnet build...
dotnet build

echo.
if %errorlevel% equ 0 (
    echo ========================================================
    echo                    全部操作成功完成！
    echo ========================================================
) else (
    echo ========================================================
    echo                  出现错误，错误代码: %errorlevel%
    echo ========================================================
)

echo.
pause