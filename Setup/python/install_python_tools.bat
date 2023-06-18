SET BATDETECT=bd2.zip
SET OUT_DIR=models
SET MODEL_DIR=bd2
SET VENV=_venv
SET POWERSHELL=C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe
SET PYTHON=C:\Program Files\Python310\python.exe
SET PIP=C:\Program Files\Python310\Scripts\pip.exe
SET PYTHON_INSTALLER=python-3.10.10-amd64.exe

rem install python 
%PYTHON_INSTALLER%

rem install missing python libraries
mkdir %OUT_DIR%
cd %OUT_DIR%

%PYTHON% -m venv %VENV%
%VENV%/Scripts/activate
%PIP% install -r requirements.txt

rem unzip model files
%POWERSHELL% -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('%BATDETECT%', '%MODEL_DIR%'); }"


