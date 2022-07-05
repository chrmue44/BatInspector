set SPEC_PATH=C:/Users/chrmu/bat/train/species.csv
set MOD_PATH=C:/Users/chrmu/prj/BatInspector/mod/trn/batch/
python batclass.py --run --clean --train -s %SPEC_PATH% -o %MOD_PATH% -g C:/Users/chrmu/prj/BatInspector/mod/trn/checktest.csv --specFile "C:/Users/chrmu/bat/train/species.csv"
pause