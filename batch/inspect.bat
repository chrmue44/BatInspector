rem executable R
set TOOL="C:\Program Files\R\R-4.2.0/bin/Rscript.exe"
rem scritp name
set ROOT=C:\Users\chrmu
rem set ROOT=D:\
set R_SCRIPT=%ROOT%\prj\BatInspector\R\features.R
set PY_SCRIPT=%ROOT%\prj\BatInspector\py\batclass.py

set SAMPLE_RATE=312800
set YEAR=2022
SET MONTH=07
SET DAY=02
rem directory where the model data is stored
set MOD_PATH=C:/Users/chrmu/prj/BatInspector/mod_tsa
rem directory containing the WAV files to inspect
set DIR=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%
rem #### set DIR=C:/Users/chrmu/prj/BatInspector/mod/trn
rem report file containing information about the wav files to inspect
set REPORT=C:\Users\chrmu\bat\%YEAR%\%YEAR%%MONTH%%DAY%\report.csv
rem ####set REPORT=%DIR%/checkt.csv
set SPEC_FILE=C:/Users/chrmu/bat/tierSta/species.csv
rem name of the data file for the prediction
set DAT_FILE=%DIR%/Xdata000.npy
rem ####set DAT_FILE=%DIR%/Xtest000.npy

goto start_cut
rem find all calls in recordings specified above
%TOOL% %_RSCRIPT% %DIR%\Records %REPORT% %SPEC_FILE% %SAMPLE_RATE%


:start_cut
mkdir %DIR%\bat
mkdir %DIR%\dat
mkdir %DIR%\img
mkdir %DIR%\wav
mkdir %DIR%\log

del "%DIR%\bat\*.*" /Q
del "%DIR%\dat\*.*" /Q
del "%DIR%\img\*.*" /Q
del "%DIR%\wav\*.*" /Q
del "%DIR%\log\*.*" /Q

goto all_in_one

python %PY_SCRIPT% --cut --csvcalls %REPORT% --root %DIR%
pause

:prepare
python %PY_SCRIPT% --prepPredict --specFile %SPEC_FILE% --root %DIR%
pause

:predict
python %PY_SCRIPT% --predict --data %DAT_FILE% --csvcalls %REPORT% --specFile %SPEC_FILE% 
pause

:all_in_one
python %PY_SCRIPT% --cut --prepPredict --predict --csvcalls %REPORT% --root %MOD_PATH% --dataDir %DIR% --specFile %SPEC_FILE% --data %DAT_FILE% 
pause
