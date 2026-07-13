SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET BD2_VERSION=%2
SET PYTHON_DIR=%1
SET LOG_DIR=%3
SET LOG_FILE=%LOG_DIR%\bat_detect2.inst_log
SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %PY_INST% %REQ_VERSION% %BD2_VERSION% %LOG_DIR%

@echo ******************************************************
@echo * installing AI model BatDetect2
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install batdetect2
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
@echo create virtual environment...
%PYTHON% -m venv %VENV% > %LOG_FILE% 2>&1
call %VENV%/Scripts/activate >> %LOG_FILE% 2>&1

@echo ******************************************************
@echo * installing AI model BatDetect2
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install batdetect2 model...
pip install batdetect2=="%BD2_VERSION%" >> %LOG_FILE% 2>&1
@echo install noise reduction...
pip install noisereduce==3.0.3 >> %LOG_FILE% 2>&1
@echo installation finished...




