SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET PYTHON_DIR=%1
SET PY_INST=%2
SET REQ_VERSION=%3
FOR /F "tokens=*" %%a in ('python -V') do SET VERSION=%%a

echo.%VERSION%|findstr /C:"%REQ_VERSION%" >nul 2>&1
if not errorlevel 1 (
  SET PYTHON=python
  goto model
) else (
  SET PYTHON=%PYTHON_DIR%\python
  %PY_INST% /passive InstallAllUsers=1 DefaultAllUsersTargetDir=%PYTHON_DIR%
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
pip install batdetect2==1.0.6
pip install noisereduce==3.0.0

