@echo off
@echo ========================================================================
@echo This script must be run as administrator, or it will fail.
@echo ========================================================================
@echo:

sc create Sentry.data.SpamFactory binPath= "%~dp0Sentry.data.SpamFactory.exe" DisplayName= "Sentry.data.SpamFactory Windows Service" start= "auto"
@IF %ERRORLEVEL% NEQ 0 GOTO CREATEFAILURE

sc description Sentry.data.SpamFactory "Windows Service part of the Sentry.data.SpamFactory application."
@IF %ERRORLEVEL% NEQ 0 GOTO DESCFAILURE

:SUCCESS
@echo:
@echo WindowsService successfully installed.  Remember to configure the logon and recovery settings.
SET ERRLVL=0
GOTO Quit

:CREATEFAILURE
@ECHO:
@ECHO =================================================
@ECHO The service failed to install correctly.
@ECHO =================================================
@SET ERRLVL=1
@GOTO Quit

:DESCFAILURE
@ECHO:
@ECHO =================================================
@ECHO The service description could not be updated correctly.
@ECHO =================================================
@SET ERRLVL=2
@GOTO Quit

:Quit
pause
@EXIT %ERRLVL%