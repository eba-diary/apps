::--------------------------------------------------------------------------------------
::	Start Service
::	This section will start the services for the local machine / server
::
::	This script sets up a logfile name to capture the results and then executes
::	the WindowsService_Start_run.cmd
::	The format of the logfile name is WindowsService_Start-<Timestamp>.log.
::--------------------------------------------------------------------------------------

:INITIALIZATION
::Format Timestamp (For Log File)
FOR /F "TOKENS=1-4 DELIMS=/- " %%A IN ('date/T') DO SET d=%%B%%C%%D  
SET d=%d:~4,4%%d:~0,4%
for /F "tokens=1-4 delims=:., " %%a in ("%TIME%") do set t=%%a%%b
if "%t:~3,1%"=="" set t=0%t%
 
::Create the name of the Log File
set RUNPATH=%~dp0
echo %RUNPATH%
set LOGPATH=%RUNPATH%Logs
md "%LOGPATH%"
SET LOGFILE=%LOGPATH%\WindowsService_Start-%d%_%t%.log

cmd /c "%RUNPATH%WindowsService_Start_run.cmd" > "%LOGFILE%" 2>&1
@EXIT %ERRORLEVEL%
