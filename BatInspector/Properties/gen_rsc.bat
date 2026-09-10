set TOOL="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools"
SET RES=MyResources

@echo off
set QUELLE=%RES%.resx
set ZIEL=%RES%.Designer.cs
set DATEINAME=%RES%

cd %1\Properties
%TOOL%\resgen %RES%.resx /str:c#,BatInspector.Properties,%RES%,%RES%.Designer.cs /publicClass

xcopy "%QUELLE%" "%ZIEL%" /D /L /Y | find /I "%DATEINAME%" >nul

if %errorlevel% equ 0 (
    echo Quelldatei ist neuer. Programm wird gestartet...
    %TOOL%\resgen %RES%.resx /str:c#,BatInspector.Properties,%RES%,%RES%.Designer.cs /publicClass
) else (
    echo Zieldatei ist aktuell. Kein Start erforderlich.
)

