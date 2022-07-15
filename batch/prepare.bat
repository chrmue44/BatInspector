set ROOT=C:/Users/chrmu
set MOD_PATH=%ROOT%/prj/BatInspector/mod_tsa
mkdir %MOD_PATH%\log
python ../py/batclass.py --prepTrain --specFile %ROOT%/bat/tierSta/species.csv --root %MOD_PATH%
pause