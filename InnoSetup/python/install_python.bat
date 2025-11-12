SET PYTHON_DIR=%1
SET PY_INST=%2

echo **************************************
echo install python
echo **************************************


@echo params %PYTHON_DIR% %PY_INST% 

SET PYTHON=%PYTHON_DIR%\python
@echo %PY_INST% /passive InstallAllUsers=1 Include_dev=0 Include_test=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
%PY_INST% /passive InstallAllUsers=1 Include_doc=0 Include_dev=0 Include_test=0 Include_launcher=0 Include_tcltk=0 DefaultAllUsersTargetDir=%PYTHON_DIR%

