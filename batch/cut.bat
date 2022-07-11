rem set ROOT=D:
set ROOT=C:\Users\chrmu
set MODPATH=%ROOT%\prj\BatInspector\mod
del %MODPATH%\dat\*.* /Q
del %MODPATH%\img\*.* /Q
del %MODPATH%\wav\*.* /Q
python ..\py\batclass.py --cut --axes --csvcalls "%ROOT%/bat/train/calls.csv" --root %MODPATH% 
pause