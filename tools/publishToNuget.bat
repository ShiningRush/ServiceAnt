@echo off
set nugetexe=%~dp0nuget.exe
set projNumber=1

if not exist .\nupkg md .\nupkg
setlocal enabledelayedexpansion
echo 检测到如下项目：
for /f %%i in (needPublishPro.config) DO (
    echo !projNumber!、%%i
    set projPath_!projNumber!=%%i
    set /a projNumber=!projNumber!+1
)

set /p userInput=请输入需要发布的项目序号，按回车确定：
dotnet msbuild ..\src\!projPath_%userInput%! /t:pack /p:Configuration=Release /p:SourceLinkCreate=true
rem %nugetexe% pack ..\src\!projPath_%userInput%! -Build -Properties Configuration=Release -OutputDirectory .\nupkg -IncludeReferencedProjects -Symbols
move ..\src\!projPath_%userInput%!\..\bin\Release\*.nupkg .\nupkg\

rem del /q /f .\nupkg\*.nupkg
rem %nugetexe% push .\nupkg\*.nupkg -Source http://192.168.19.88:1024/nuget clear

:end
echo 上传packge完成，输入任意键继续
set/p xxxx= >nul