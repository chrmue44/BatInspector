set ROOT=D:
set ROOT=C:/Users/chrmu
set SCRIPT="C:/Users/chrmu/prj/BatInspector/py/batclass.py"
set IN_DIR01=%ROOT%/bat/tierStABara/Bbar/*.wav
set IN_DIR02=%ROOT%/bat/tierStABara/Eser/*.wav
set IN_DIR03=%ROOT%/bat/tierStABara/Mdau/*.wav
set IN_DIR04=%ROOT%/bat/tierStABara/Mmyo/*.wav
set IN_DIR05=%ROOT%/bat/tierStABara/Nlei/*.wav
set IN_DIR06=%ROOT%/bat/tierStABara/Nnoc/*.wav
set IN_DIR07=%ROOT%/bat/tierStABara/Pnat/*.wav
set IN_DIR08=%ROOT%/bat/tierStABara/Ppip/*.wav
set IN_DIR09=%ROOT%/bat/tierStABara/Ppyg/*.wav
set SAMPLE_RATE=312500

python %SCRIPT% --resample %IN_DIR01%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR02%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR03%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR04%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR05%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR06%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR07%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR08%  --sampleRate %SAMPLE_RATE% 
python %SCRIPT% --resample %IN_DIR09%  --sampleRate %SAMPLE_RATE% 

rem python "C:/Users/chrmu/prj/BatInspector/py/batclass.py" --resample C:/Users/chrmu/bat/2022/20220610/Records/*.wav --sampleRate 312500 --csvcalls C:/Users/chrmu/bat/2022/20220610/report.csv --root "C:/Users/chrmu/prj/BatInspector/mod_tsa" --specFile "C:/Users/chrmu/bat/tierSta/species.csv" --dataDir C:/Users/chrmu/bat/2022/20220610/ --data C:/Users/chrmu/bat/2022/20220610//Xdata000.npy
pause
