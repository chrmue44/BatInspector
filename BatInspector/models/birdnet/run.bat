SET BIN_PATH=%1
SET WAV_DIR=%2
SET ANN_DIR=%3
SET SENSITIVITY=%4
SET MIN_CONF=%5
SET LAT=%6
SET LON=%7
SET WEEK=%8
SET LOCALE=%9
@echo *****************************************
@echo *   Starting AI model BirdNET ...       *
@echo *****************************************
call %BIN_PATH%/_venv/Scripts/activate
python -m birdnet_analyzer.analyze %WAV_DIR% --output %ANN_DIR% --lat %LAT% --lon %LON% --sensitivity %SENSITIVITY% --min_conf %MIN_CONF% --week %WEEK% --locale %LOCALE% --rtype csv
call %BIN_PATH%/_venv/Scripts/deactivate
rem pause