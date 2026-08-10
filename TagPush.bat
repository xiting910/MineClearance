@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

cd /d "%~dp0"

set "PROPS_FILE=Directory.Build.props"
set "CHANGELOG_FILE=CHANGELOG.md"
set "REMOTE=origin"

echo ========================================================
echo              MineClearance 打 tag 并推送脚本
echo ========================================================

rem 1. 检查当前目录是否为 Git 仓库
echo.
echo [1/7] 检查 Git 仓库...
git rev-parse --git-dir >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] 当前目录不是 Git 仓库
    goto :end
)
echo 当前目录是 Git 仓库.

rem 2. 检查工作区是否干净 (含未跟踪文件, 发布前源码必须完整)
echo.
echo [2/7] 检查工作区状态...
set "DIRTY="
for /f "delims=" %%a in ('git status --porcelain') do set "DIRTY=1"
if defined DIRTY (
    echo [ERROR] 工作区不干净, 存在未提交的修改或未跟踪文件, 请先提交并清理:
    echo.
    git status --short
    goto :end
)
echo 工作区干净.

rem 3. 从 Directory.Build.props 读取 Version 值
echo.
echo [3/7] 读取版本号...
set "VER="
for /f "delims=" %%a in ('findstr /c:"<Version>" "%PROPS_FILE%"') do set "RAW=%%a"
set "RAW=%RAW:*<Version>=%"
set "RAW=%RAW:</Version>=%"
set "VER=%RAW: =%"

rem 校验版本号格式 (x.y.z)
echo %VER%|findstr /r "^[0-9][0-9]*[.][0-9][0-9]*[.][0-9][0-9]*$" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] 从 %PROPS_FILE% 读取版本号失败或格式不正确, 当前值: [%VER%]
    goto :end
)

set "TAG=v%VER%"
echo 版本号: %VER%
echo 目标 tag: %TAG%

rem 4. 检查 tag 是否已存在
echo.
echo [4/7] 检查 tag 是否已存在...
set "EXISTING="
for /f "delims=" %%t in ('git tag -l "%TAG%"') do set "EXISTING=%%t"
if defined EXISTING (
    echo [ERROR] Tag %TAG% 已存在, 可能已经发布过, 请检查后手动处理
    goto :end
)
echo Tag %TAG% 不存在, 可以创建.

rem 5. 检查 CHANGELOG.md 是否已有对应版本条目
echo.
echo [5/7] 检查 CHANGELOG 条目...
findstr /c:"## [%VER%] -" "%CHANGELOG_FILE%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] %CHANGELOG_FILE% 中未找到版本条目 [%VER%], 请先更新 CHANGELOG.md
    goto :end
)
echo 找到 CHANGELOG 版本条目: ## [%VER%]

rem 6. 创建 tag
echo.
echo [6/7] 创建 tag 并推送...
git tag "%TAG%"
if %errorlevel% neq 0 (
    echo [ERROR] 创建 tag %TAG% 失败
    goto :end
)
echo Tag %TAG% 已创建.

rem 7. 推送 tag 到远程仓库
echo.
echo [7/7] 推送 tag 到远程仓库 %REMOTE%...
git push %REMOTE% "%TAG%"
if %errorlevel% neq 0 (
    echo [ERROR] 推送 tag %TAG% 失败, tag 已在本机创建, 可手动执行: git push %REMOTE% %TAG%
    goto :end
)
echo Tag %TAG% 已推送到 %REMOTE%.

echo.
echo ========================================================
echo       全部完成! GitHub Actions 将自动构建并发布
echo ========================================================

:end
echo.
pause
exit /b 0
