SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET MODEL_BB_DIR=bbnet
SET MODEL_BIRD_DIR=birdnet
SET VENV=_venv
SET BBNET_ARCH=https://github.com/kahst/BirdNET-Analyzer/archive/
SET PYTHON_DIR=%1
SET PY_INST=%2
SET REQ_VERSION=%3
SET BD2_VERSION=%4
SET BBNET_HASH=%5
SET BIRDNET_HASH=%6
SET BIRDNET_ARCH=https://github.com/birdnet-team/BirdNET-Analyzer/archive/

@echo params %PYTHON_DIR% %PY_INST% %REQ_VERSION% %BD2_VERSION%

SET PYTHON=%PYTHON_DIR%\python
@echo %PY_INST% /passive InstallAllUsers=1 Include_dev=0 Include_test=0 DefaultAllUsersTargetDir=%PYTHON_DIR%
%PY_INST% /passive InstallAllUsers=1 Include_doc=0 Include_dev=0 Include_test=0 Include_tcltk=0 DefaultAllUsersTargetDir=%PYTHON_DIR%


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
pip install batdetect2=="%BD2_VERSION%"
rem @echo pip install -r requirements.txt
rem pip install -r requirements.txt

:model_bbnet
cd ..
@echo install BattyBirdNET
cd %MODEL_BB_DIR%
curl -L %BBNET_ARCH%%BBNET_HASH%.zip --output bbnet.zip
tar -xf bbnet.zip
del bbnet.zip
xcopy /E /H /C BirdNET-Analyzer-%BBNET_HASH%
pause
rmdir /s /q BirdNET-Analyzer-%BBNET_HASH%
pause
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install -r requirements.txt
pip install -r requirements.txt

:model_birdnet
cd ..
@echo install BirdNET
cd %MODEL_BIRD_DIR%
curl -L %BIRDNET_ARCH%%BIRDNET_HASH%.zip --output birdnet.zip
tar -xf birdnet.zip
del birdnet.zip
xcopy /E /H /C BirdNET-Analyzer-%BIRDNET_HASH%
rmdir /s /q BirdNET-Analyzer-%BIRDNET_HASH%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo pip install -r requirements.txt
pip install .
pip install keras_tuner

