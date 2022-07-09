set ROOT=D:
set ROOT=C:/Users/chrmu
set SPEC_PATH=%ROOT%/bat/train/species.csv
set MOD_PATH=%ROOT%/prj/BatInspector/mod

python ../py/batclass.py --run --clean --train --root %MOD_PATH% -g %ROOT%/prj/BatInspector/mod/checktest.csv --specFile %SPEC_PATH% 
pause