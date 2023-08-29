SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET PY_INST=python-3.10.10-amd64.exe
SET REQ_VERSION=Python 3.10

FOR /F "tokens=*" %%a in ('python -V') do SET VERSION=%%a
echo.%VERSION%|findstr /C:"%REQ_VERSION%" >nul 2>&1
if not errorlevel 1 (
   goto model
) else (
  %PY_INST%
) 

rem install missing python libraries
:model
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
python -m venv %VENV%
call %VENV%/Scripts/activate
pip install batdetect2==1.0.6

