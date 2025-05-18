SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET BD2_VERSION=%2
SET PYTHON_DIR=%1
SET PYTHON=%PYTHON_DIR%\python

@echo params %PYTHON_DIR% %PY_INST% %REQ_VERSION% %BD2_VERSION%


@echo ******************************************************
@echo * installing AI model BatDetect2
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
@echo install batdetect2
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
%PYTHON% -m venv %VENV%
call %VENV%/Scripts/activate
@echo ******************************************************
@echo * installing AI model BatDetect2
@echo * This may take several minutes 
@echo * Be patient! Ooohmm.... 
@echo ******************************************************
pip install batdetect2=="%BD2_VERSION%"


