rem set ROOT=D:
set ROOT=C:\Users\chrmu
set MODPATH=%ROOT%\prj\BatInspector\mod\trn\
:del "%MODPATH%dat\%SPEC%*.*"
:del "%MODPATH%img\%SPEC%*.*"
:del "%MODPATH%wav\%SPEC%*.*"
python batclass.py --cut  --csvcalls "%ROOT%/bat/train/calls.csv" -o %MODPATH% 
pause