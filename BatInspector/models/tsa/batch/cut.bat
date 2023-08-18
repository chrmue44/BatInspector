rem set ROOT=D:
set ROOT=C:\Users\chrmu
set MODPATH=%ROOT%\prj\BatInspector\mod_tsabara
mkdir %MODPATH%\dat
mkdir %MODPATH%\img
mkdir %MODPATH%\wav
del %MODPATH%\dat\*.* /Q
del %MODPATH%\img\*.* /Q
del %MODPATH%\wav\*.* /Q
python ..\py\batclass.py --cut --axes --csvcalls "%ROOT%/bat/tierStABara/calls.csv" --dataDir %MODPATH% 
pause