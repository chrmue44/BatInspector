set SPEC=Eser
set MODPATH=C:\Users\chrmu\prj\BatInspector\mod\trn\
del "%MODPATH%dat\%SPEC%*.*"
del "%MODPATH%img\%SPEC%*.*"
del "%MODPATH%wav\%SPEC%*.*"
python batclass.py -c -f "C:/Users/chrmu/bat/train/calls_%SPEC%.csv" -o %MODPATH% -i -a
pause