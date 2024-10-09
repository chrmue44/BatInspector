SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET MODEL_BB_DIR=bbnet
SET VENV=_venv
SET BBNET_ARCH=https://github.com/kahst/BirdNET-Analyzer/archive/
SET PYTHON_DIR=%1
SET PY_INST=%2
SET REQ_VERSION=%3
SET BD2_VERSION=%4
SET BBNET_HASH=%5

@echo params %PYTHON_DIR% %PY_INST% %REQ_VERSION% %BD2_VERSION%
goto model_bbnet

FOR /F "tokens=*" %%a in ('python -V') do SET VERSION=%%a

echo.%VERSION%|findstr /C:"%REQ_VERSION%" >nul 2>&1
if not errorlevel 1 (
  SET PYTHON=python
  goto model_bd2
) else (
  SET PYTHON=%PYTHON_DIR%\python
  @echo %PY_INST% /passive InstallAllUsers=1 Include_dev=0 Include_test=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
  %PY_INST% /passive InstallAllUsers=1 Include_doc=0 Include_dev=0 Include_test=0 Include_tcltk=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
) 
:model_bd2

rem install missing python libraries
@echo ******************************************************
@echo * installing AI model. This may take several minutes *
@echo * Be patient! Ooohmm....                             *
@echo ******************************************************
@echo install batdetect2
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install -r requirements.txt
pip install -r requirements.txt

:model_bbnet
@echo install BattyBirdNET
cd %MODEL_BB_DIR%
curl -L %BBNET_ARCH%%BBNET_HASH%.zip --output bbnet.zip
tar -xf bbnet.zip
del bbnet.zip
xcopy /E /H /C BirdNET-Analyzer-%BBNET_HASH%
rmdir /s /q BirdNET-Analyzer-%BBNET_HASH%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install -r requirements.txt
pip install -r requirements.txt
