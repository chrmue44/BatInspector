set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
set SCRIPT=features.R
rem set DIR=%1%
rem set RESULT=%2%
rem set SPEC_FILE=%3%
set SPEC=Eser
set DIR=C:\Users\chrmu\bat\train\%SPEC%
set RESULT=C:\Users\chrmu\bat\train\calls_%SPEC%.csv
set SPEC_FILE=C:\Users\chrmu\bat\train\species.csv
%TOOL% %SCRIPT% %DIR% %RESULT% %SPEC_FILE%
pause
