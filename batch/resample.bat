set ROOT=D:
set ROOT=C:/Users/chrmu
set IN_DIR=%ROOT%/bat/tierStA/Pnat/*.wav
set SAMPLE_RATE=312500

rem python ../py/batclass.py --resample %IN_DIR%  --sampleRate %SAMPLE_RATE% 

python "C:/Users/chrmu/prj/BatInspector/py/batclass.py" --resample C:/Users/chrmu/bat/2022/20220610/Records/*.wav --sampleRate 312500 --csvcalls C:/Users/chrmu/bat/2022/20220610/report.csv --root "C:/Users/chrmu/prj/BatInspector/mod_tsa" --specFile "C:/Users/chrmu/bat/tierSta/species.csv" --dataDir C:/Users/chrmu/bat/2022/20220610/ --data C:/Users/chrmu/bat/2022/20220610//Xdata000.npy
pause
