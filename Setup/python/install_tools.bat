SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET PYTHON=C:\Program Files\Python310\python.exe
SET PIP=C:\Program Files\Python310\Scripts\pip.exe
SET PYTHON_INSTALLER=python-3.10.10-amd64.exe

rem install python 
%PYTHON_INSTALLER%

rem install missing python libraries
mkdir %OUT_DIR%
cd %OUT_DIR%
mkdir %MODEL_DIR%
cd %MODEL_DIR%
%PYTHON% -m venv %VENV%
%VENV%/Scripts/activate
%PIP% install batdetect2==1.0.6

