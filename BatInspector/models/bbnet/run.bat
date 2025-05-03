SET BIN_PATH=%1
SET WAV_DIR=%2
SET ANN_DIR=%3
SET SENS=%4
SET MIN_CONF=%5
SET MODEL=%6
@echo *****************************************
@echo *   Starting AI model BattyBirdNET ...  *
@echo *****************************************
call %BIN_PATH%/_venv/Scripts/activate
@echo python bat_ident.py --i %WAV_DIR% --o %ANN_DIR% --sensitivity %SENS% --min_conf %MIN_CONF%  --classifier %BIN_PATH%\checkpoints\bats\v1.0\%MODEL%.tflite
python bat_ident.py --i %WAV_DIR% --o %ANN_DIR% --sensitivity %SENS% --min_conf %MIN_CONF%  --classifier %BIN_PATH%\checkpoints\bats\v1.0\%MODEL%.tflite
call %BIN_PATH%/_venv/Scripts/deactivate
rem pause