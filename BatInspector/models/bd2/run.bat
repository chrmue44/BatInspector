SET BIN_PATH=%1
SET WAV_DIR=%2
SET ANN_DIR=%3
SET THRESH=%4
SET OPTIONS=
call %BIN_PATH%/_venv/Scripts/activate
batdetect2 detect %WAV_DIR% %ANN_DIR% %THRESH% --spec_features
call %BIN_PATH%/_venv/Scripts/deactivate
rem pause