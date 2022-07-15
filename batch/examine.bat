set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
set SCRIPT=C:\Users\chrmu\prj\BatInspector\R\features.R
rem set DIR=%1%
rem set RESULT=%2%
rem set SPEC_FILE=%3%
set DIR=C:\Users\chrmu\bat\tierSta\
set RESULT=C:\Users\chrmu\bat\tierSta\calls.csv
set SPEC_FILE=C:\Users\chrmu\bat\tierSta\species.csv
set SAMPLE_RATE=312500
%TOOL% %SCRIPT% %DIR% %RESULT% %SPEC_FILE% %SAMPLE_RATE%
pause
