set ROOT=D:
set ROOT=C:/Users/chrmu
set IN_DIR=%ROOT%/bat/tierStA/Eser/*.wav
set SAMPLE_RATE=312500
 

python ../py/batclass.py --resample %IN_DIR%  --sampleRate %SAMPLE_RATE% 
pause