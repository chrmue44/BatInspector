SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET PYTHON_DIR=%1
SET PY_INST=%2
SET REQ_VERSION=%3
SET BD2_VERSION=%4

@echo params %PYTHON_DIR% %PY_INST% %REQ_VERSION% %BD2_VERSION%

FOR /F "tokens=*" %%a in ('python -V') do SET VERSION=%%a

echo.%VERSION%|findstr /C:"%REQ_VERSION%" >nul 2>&1
if not errorlevel 1 (
  SET PYTHON=python
  goto model
) else (
  SET PYTHON=%PYTHON_DIR%\python
  @echo %PY_INST% /passive InstallAllUsers=1 Include_dev=0 Include_test=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
  %PY_INST% /passive InstallAllUsers=1 Include_doc=0 Include_dev=0 Include_test=0 Include_tcltk=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
) 
:model

rem install missing python libraries
@echo ******************************************************
@echo * installing AI model. This may take several minutes *
@echo * Be patient! Ooohmm....                             *
@echo ******************************************************
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install batdetect2==%BD2_VERSION%
pip install batdetect2==%BD2_VERSION%
@echo pip install noisereduce==3.0.0
pip install noisereduce==3.0.0
