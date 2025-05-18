SET OUT_DIR=models
SET MODEL_BIRD_DIR=birdnet
SET VENV=_venv
SET PYTHON_DIR=%1
SET BIRDNET_HASH=%2
SET BIRDNET_ARCH=https://github.com/birdnet-team/BirdNET-Analyzer/archive/
SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %BIRDNET_HASH%
@echo ******************************************************
@echo * installing AI model BirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
:model_birdnet
cd ..
@echo install BirdNET
CD %OUT_DIR%
cd %MODEL_BIRD_DIR%
curl -L %BIRDNET_ARCH%%BIRDNET_HASH%.zip --output birdnet.zip
tar -xf birdnet.zip
del birdnet.zip
xcopy /E /H /C BirdNET-Analyzer-%BIRDNET_HASH%
rmdir /s /q BirdNET-Analyzer-%BIRDNET_HASH%

%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install -r requirements.txt
@echo ******************************************************
@echo * installing AI model BirdNET
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
pip install .
pip install keras_tuner

