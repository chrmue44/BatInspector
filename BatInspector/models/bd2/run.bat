SET BIN_PATH=%1
SET WAV_DIR=%2
SET ANN_DIR=%3
SET THRESH=%4
SET MODEL=%5
@echo *****************************************
@echo *   Starting AI model batdetect 2 ...   *
@echo *****************************************
call %BIN_PATH%/_venv/Scripts/activate
batdetect2 detect %WAV_DIR% %ANN_DIR% %THRESH% --spec_features --model_path %MODEL%
call %BIN_PATH%/_venv/Scripts/deactivate
rem pause