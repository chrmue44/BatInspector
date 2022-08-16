set ROOT=C:/Users/chrmu
set MOD_PATH=%ROOT%/prj/BatInspector/mod_tsabara
mkdir %MOD_PATH%\log
python ../py/batclass.py --prepTrain --specFile %ROOT%/bat/tierStabara/species.csv --root %MOD_PATH%
pause