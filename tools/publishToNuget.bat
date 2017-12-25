@echo off
set nugetexe=.\nuget.exe
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
%nugetexe% pack ..\src\!projPath_%userInput%! -Build -Properties Configuration=Release -OutputDirectory .\nupkg -IncludeReferencedProjects -Symbols
move ..\src\!projPath_%userInput%!\..\bin\Release\*.nupkg .\nupkg\

%nugetexe% push .\*.nupkg -s https://www.nuget.org/api/v2/package 4917e7f9-0370-40c2-8074-f4f23b85ef41
del /q /f .\nupkg\*.nupkg

:end
echo 上传packge完成，输入任意键继续
set/p xxxx= >nul