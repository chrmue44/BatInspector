set ROOT=D:
set SPEC_PATH=%ROOT%/bat/train/species.csv
set MOD_PATH=%ROOT%/prj/BatInspector/mod/trn/batch/
python batclass.py --run --clean --train -s %SPEC_PATH% -o %MOD_PATH% -g %ROOT%/prj/BatInspector/mod/trn/checktest.csv --specFile %SPEC_PATH%
pause