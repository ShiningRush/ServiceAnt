@echo off
set nugetexe=.\nuget.exe
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
%nugetexe% pack ..\src\!projPath_%userInput%! -Build -Properties Configuration=Release -OutputDirectory .\nupkg -IncludeReferencedProjects -Symbols
move ..\src\!projPath_%userInput%!\..\bin\Release\*.nupkg .\nupkg\

%nugetexe% push .\*.nupkg -s https://www.nuget.org/api/v2/package 4917e7f9-0370-40c2-8074-f4f23b85ef41
del /q /f .\nupkg\*.nupkg

:end
echo �ϴ�packge��ɣ��������������
set/p xxxx= >nul