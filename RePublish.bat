@echo off
chcp 65001 >nul
title Clean and Build .NET Project

echo ========================================================
echo                .NET 项目清理和构建脚本
echo ========================================================

echo.
echo [1/7] 正在删除 bin 文件夹...
if exist "bin" (
    echo 删除: bin
    rmdir /s /q "bin"
)
for /f "delims=" %%i in ('dir /s /b /a:d "bin" 2^>nul') do (
    if exist "%%i" (
        echo 删除: %%i
        rmdir /s /q "%%i"
    )
)

echo.
echo [2/7] 正在删除 obj 文件夹...
if exist "obj" (
    echo 删除: obj
    rmdir /s /q "obj"
)
for /f "delims=" %%i in ('dir /s /b /a:d "obj" 2^>nul') do (
    if exist "%%i" (
        echo 删除: %%i
        rmdir /s /q "%%i"
    )
)

echo.
echo [3/7] 正在删除 publish 文件夹...
if exist "publish" (
    echo 删除: publish
    rmdir /s /q "publish"
)
for /f "delims=" %%i in ('dir /s /b /a:d "publish" 2^>nul') do (
    if exist "%%i" (
        echo 删除: %%i
        rmdir /s /q "%%i"
    )
)

echo.
echo [4/7] 正在执行 dotnet restore...
dotnet restore
if %errorlevel% neq 0 goto :error

echo.
echo [5/7] 正在执行 dotnet build...
dotnet build --no-restore
if %errorlevel% neq 0 goto :error

echo.
echo [6/7] 正在发布到 publish 文件夹...
dotnet publish srcs\MineClearance.UI\MineClearance.UI.csproj -c Release -r win-x64 --no-restore -o publish\
if %errorlevel% neq 0 goto :error

echo.
echo [7/7] 正在清理发布产物中的多余文件...
for /f "delims=" %%i in ('dir /s /b "publish\*.pdb" "publish\*.xml" 2^>nul') do (
    echo 删除: %%i
    del /q "%%i"
)

echo.
echo ========================================================
echo                    全部操作成功完成！
echo ========================================================
goto :end

:error
echo.
echo ========================================================
echo              出现错误，错误代码: %errorlevel%
echo ========================================================

:end
echo.
pause
