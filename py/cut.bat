set ROOT=D:
rem set ROOT=C:\Users\chrmu
set SPEC=Eser_Skiba
set MODPATH=%ROOT%\prj\BatInspector\mod\trn\
:del "%MODPATH%dat\%SPEC%*.*"
:del "%MODPATH%img\%SPEC%*.*"
:del "%MODPATH%wav\%SPEC%*.*"
python batclass.py -c -f "%ROOT%/bat/train/calls_%SPEC%.csv" -o %MODPATH% -i -a
pause