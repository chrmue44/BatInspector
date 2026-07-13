SET OUT_DIR=models
SET MODEL_BB_DIR=bbnet
SET VENV=_venv
SET BBNET_ARCH=https://github.com/kahst/BirdNET-Analyzer/archive/
SET PYTHON_DIR=%1
SET BBNET_HASH=%2
SET LOG_DIR=%3
SET LOG_FILE=%LOG_DIR%\bbnet.inst_log

SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %BBNET_HASH% %LOG_DIR%

@echo ******************************************************
@echo * installing AI model BattyBirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install BattyBirdNET
cd ..
cd %OUT_DIR%
cd %MODEL_BB_DIR%
@echo fetching battx BirdNET...
curl -L %BBNET_ARCH%%BBNET_HASH%.zip --output bbnet.zip > %LOG_FILE% 2>&1
tar -xf bbnet.zip >> %LOG_FILE% 2>&1
del bbnet.zip >> %LOG_FILE% 2>&1
xcopy /E /H /C BirdNET-Analyzer-%BBNET_HASH% >> %LOG_FILE% 2>&1
rmdir /s /q BirdNET-Analyzer-%BBNET_HASH% >> %LOG_FILE% 2>&1
@echo create virtual environment...
%PYTHON% -m venv %VENV% >> %LOG_FILE% 2>&1
call %VENV%/Scripts/activate >> %LOG_FILE% 2>&1
@echo ******************************************************
@echo * installing AI model BattyBirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo pip install -r requirements.txt >> %LOG_FILE% 2>&1
pip install -r requirements.txt >> %LOG_FILE% 2>&1
@echo installation complete

