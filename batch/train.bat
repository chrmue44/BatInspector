set ROOT=D:
set ROOT=C:/Users/chrmu
set SPEC_PATH=%ROOT%/bat/tierStA/species.csv
set MOD_PATH=%ROOT%/prj/BatInspector/mod_tsa

rem python ../py/batclass.py --train --clean --run --root %MOD_PATH% -g %MOD_PATH%/log/checktest.csv --specFile %SPEC_PATH% 
python ../py/batclass.py --train --run --epochs 30 --root %MOD_PATH% -g %MOD_PATH%/log/checktest.csv --specFile %SPEC_PATH% 

pause