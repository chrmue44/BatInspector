SET OUT_DIR=models
SET MODEL_BB_DIR=bbnet
SET VENV=_venv
SET BBNET_ARCH=https://github.com/kahst/BirdNET-Analyzer/archive/
SET PYTHON_DIR=%1
SET BBNET_HASH=%2
SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %BBNET_HASH%
@echo ******************************************************
@echo * installing AI model BattyBirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install BattyBirdNET
cd ..
cd %OUT_DIR%
cd %MODEL_BB_DIR%
curl -L %BBNET_ARCH%%BBNET_HASH%.zip --output bbnet.zip
tar -xf bbnet.zip
del bbnet.zip
xcopy /E /H /C BirdNET-Analyzer-%BBNET_HASH%
rmdir /s /q BirdNET-Analyzer-%BBNET_HASH%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo ******************************************************
@echo * installing AI model BattyBirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo pip install -r requirements.txt
pip install -r requirements.txt
