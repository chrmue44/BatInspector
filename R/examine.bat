set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
set SCRIPT=features.R
rem set DIR=%1%
rem set RESULT=%2%
rem set SPEC=%3%
set DIR=C:\Users\chrmu\bat\train\vmur
set RESULT=C:\Users\chrmu\bat\train\calls_vmur.csv
set SPEC=C:\Users\chrmu\bat\train\species.csv
%TOOL% %SCRIPT% %DIR% %RESULT% %SPEC%
