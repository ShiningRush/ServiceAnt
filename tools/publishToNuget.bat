@echo off
set nugetexe=%~dp0nuget.exe
set projNumber=1

if not exist .\nupkg md .\nupkg
setlocal enabledelayedexpansion
echo ��⵽������Ŀ��
for /f %%i in (needPublishPro.config) DO (
    echo !projNumber!��%%i
    set projPath_!projNumber!=%%i
    set /a projNumber=!projNumber!+1
)

set /p userInput=��������Ҫ��������Ŀ��ţ����س�ȷ����
dotnet msbuild ..\src\!projPath_%userInput%! /t:pack /p:Configuration=Release /p:SourceLinkCreate=true
rem %nugetexe% pack ..\src\!projPath_%userInput%! -Build -Properties Configuration=Release -OutputDirectory .\nupkg -IncludeReferencedProjects -Symbols
move ..\src\!projPath_%userInput%!\..\bin\Release\*.nupkg .\nupkg\

rem del /q /f .\nupkg\*.nupkg
rem %nugetexe% push .\nupkg\*.nupkg -Source http://192.168.19.88:1024/nuget clear

:end
echo �ϴ�packge��ɣ��������������
set/p xxxx= >nul