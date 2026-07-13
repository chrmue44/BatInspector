SET OUT_DIR=models
SET MODEL_BIRD_DIR=birdnet
SET VENV=_venv
SET PYTHON_DIR=%1
SET BIRDNET_HASH=%2
SET LOG_DIR=%3
SET LOG_FILE=%LOG_DIR%\birdnet.inst_log
SET BIRDNET_ARCH=https://github.com/birdnet-team/BirdNET-Analyzer/archive/
SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %BIRDNET_HASH% %LOG_DIR%

@echo ******************************************************
@echo * installing AI model BirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************

cd ..
@echo install BirdNET
CD %OUT_DIR%
cd %MODEL_BIRD_DIR%
@echo fetch birdNET...
curl -L %BIRDNET_ARCH%%BIRDNET_HASH%.zip --output birdnet.zip  > %LOG_FILE% 2>&1
tar -xf birdnet.zip >> %LOG_FILE% 2>&1
del birdnet.zip >> %LOG_FILE% 2>&1
xcopy /E /H /C BirdNET-Analyzer-%BIRDNET_HASH% >> %LOG_FILE% 2>&1
rmdir /s /q BirdNET-Analyzer-%BIRDNET_HASH% >> %LOG_FILE% 2>&1

@echo create virtual environment...
%PYTHON% -m venv %VENV% >> %LOG_FILE% 2>&1
call %VENV%/Scripts/activate >> %LOG_FILE% 2>&1
@echo pip install .
@echo ******************************************************
@echo * installing AI model BirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install all needed libraries...
pip install . >> %LOG_FILE% 2>&1
pip install keras_tuner >> %LOG_FILE% 2>&1
@echo installation finished...
