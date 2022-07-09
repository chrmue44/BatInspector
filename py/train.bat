set ROOT=D:
set ROOT=C:/Users/chrmu
set SPEC_PATH=%ROOT%/bat/train/species.csv
set MOD_PATH=%ROOT%/prj/BatInspector/mod/trn/batch/
set MOD_FILE_PATH=%ROOT%/prj/BatInspector/py
python batclass.py --run --clean --train -s %SPEC_PATH% -o %MOD_PATH% -g %ROOT%/prj/BatInspector/mod/trn/checktest.csv --specFile %SPEC_PATH% --dirModel %MOD_FILE_PATH%
pause