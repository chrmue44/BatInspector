rem executable R
set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
rem scritp name
set SCRIPT=C:\Users\chrmu\prj\BatInspector\R\features.R

set SAMPLE_RATE=312800
set YEAR=2022
SET MONTH=07
SET DAY=02
rem directory where the model data is stored
set MOD_PATH=C:/Users/chrmu/prj/BatInspector/py
rem directory containing the WAV files to inspect
set DIR=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%
rem #### set DIR=C:/Users/chrmu/prj/BatInspector/mod/trn
rem report file containing information about the wav files to inspect
set REPORT=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%\report.csv
rem ####set REPORT=%DIR%/checkt.csv
set SPEC_FILE=C:/Users/chrmu/bat/train/species.csv
rem name of the data file for the prediction
set DAT_FILE=%DIR%/Xdata000.npy
rem ####set DAT_FILE=%DIR%/Xtest000.npy

rem goto prepare
rem find all calls in recordings specified above
%TOOL% %SCRIPT% %DIR%\Records %REPORT% %SPEC_FILE% %SAMPLE_RATE%


:start_cut
mkdir %DIR%\dat
mkdir %DIR%\img
mkdir %DIR%\wav

del "%DIR%\dat\*.*" /Q
del "%DIR%\img\*.*" /Q
del "%DIR%\wav\*.*" /Q

goto all_in_one

python batclass.py --cut --img --axes --csvcalls %REPORT% -o %DIR%/ 
pause

:prepare
python batclass.py --prepPredict --specFile %SPEC_FILE% -o %DIR%/
pause

:predict
python batclass.py --predict --data %DAT_FILE% --csvcalls %REPORT% --specFile %SPEC_FILE% --dirModel %MOD_PATH%
pause

:all_in_one
python batclass.py --cut --prepPredict --predict --csvcalls %REPORT% -o %DIR%/ --specFile %SPEC_FILE% --data %DAT_FILE% --dirModel %MOD_PATH%
pause
