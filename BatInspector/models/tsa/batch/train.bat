set ROOT=D:
set ROOT=C:/Users/chrmu
set SPEC_PATH=%ROOT%/bat/tierStAbara/species.csv
set MOD_PATH=%ROOT%/prj/BatInspector/mod_tsabara

python ../py/batclass.py --train --clean --run --root %MOD_PATH% -g %MOD_PATH%/log/checktest.csv --specFile %SPEC_PATH% --model rnn6aModel
rem python ../py/batclass.py --run --train  --epochs 30 --model rnn1aModel --root %MOD_PATH% -g %MOD_PATH%/log/checktest.csv --specFile %SPEC_PATH% 

pause