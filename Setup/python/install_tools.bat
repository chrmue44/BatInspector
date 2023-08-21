SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv

rem install missing python libraries
cd ..
cd %OUT_DIR%
cd %MODEL_DIR%
python -m venv %VENV%
call %VENV%/Scripts/activate
pip install batdetect2==1.0.6

