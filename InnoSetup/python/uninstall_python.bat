SET PYTHON_DIR=%1
SET PY_INST=%2
SET REQ_VERSION=%3
FOR /F "tokens=*" %%a in ('python -V') do SET VERSION=%%a

echo.%VERSION%|findstr /C:"%REQ_VERSION%" >nul 2>&1
if not errorlevel 1 (
  goto end
) else (
  %PY_INST% /passive /uninstall
) 

:end

