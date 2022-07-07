rem executable R
set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
rem scritp name
set SCRIPT=C:\Users\chrmu\prj\BatInspector\R\features.R
rem set DIR=%1%
rem set RESULT=%2%
rem set SPEC_FILE=%3%
set YEAR=2022
SET MONTH=07
SET DAY=02
#directory containing the WAV files to inspect
set DIR=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%
set REPORT=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%\report.csv
set SPEC_FILE=C:\Users\chrmu\bat\train\species.csv
set MOD_PATH="C:/Users/chrmu/prj/BatInspector/py

GOTO predict
rem find all calls in recordings specified above
%TOOL% %SCRIPT% %DIR%\Records %REPORT% %SPEC_FILE%
pause


:start_cut
mkdir %DIR%\dat
mkdir %DIR%\img
mkdir %DIR%\wav

del "%DIR%dat\*.*"
del "%DIR%img\*.*"
del "%DIR%wav\*.*"

python batclass.py --cut --img --axes --csvcalls %REPORT% -o %DIR%/ 
pause

:prepare
python batclass.py --prepPredict --specFile %SPEC_FILE% -o %DIR%/
pause

:predict
python batclass.py --predict --data %DIR%/Xdata000.npy --csvcalls %REPORT% --specFile %SPEC_FILE% --dirModel %MOD_PATH%
pause