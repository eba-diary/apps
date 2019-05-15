::--------------------------------------------------------------------------------------
::	Stop WindowsService
::	This section will stop the services for the local machine
::--------------------------------------------------------------------------------------
SET ERRLVL=0

:STOPSERVICE
sc stop HSZProcessor > tempfile 2>&1
@IF %ERRORLEVEL% NEQ 0 IF %ERRORLEVEL% NEQ 1062 IF %ERRORLEVEL% NEQ 1060 GOTO STOPFAILURE

:DISABLESERVICE
sc config HSZProcessor start= disabled > tempfile 2>&1
@IF %ERRORLEVEL% NEQ 0 GOTO DISABLEFAILURE


:SUCCESS
SET ERRLVL=0
GOTO Quit
 

::--------------------------------------------------------------------------------------
::	Error Handling
::	The following sections are used for error handling.  Certain sections will 
::	forward execution to these areas to handle and log the error that the 
::	script encountered.
::--------------------------------------------------------------------------------------
:STOPFAILURE
@ECHO:
@ECHO The service(s) failed to stop successfully.
@ECHO An error code of 1062 or 1060, which indicate that the service
@ECHO is already stopped or that it is not installed, are acceptable.
@ECHO:
@ECHO Results of last command before quitting are below
@ECHO =================================================
@TYPE tempfile
@SET ERRLVL=2
@GOTO Quit

:DISABLEFAILURE
@ECHO:
@ECHO The service(s) were not disabled successfully.
@ECHO:
@ECHO Results of last command before quitting are below
@ECHO =================================================
@TYPE %TEMPFILE%
@SET ERRLVL=4
@GOTO Quit


::--------------------------------------------------------------------------------------
::	Quit
::	This section is the end of the script.  It's only function is to 
::	return back to the environment the error level of the script, 
::	indicating either success or failure.
::--------------------------------------------------------------------------------------
:Quit
@del tempfile
@SET ERRORLEVEL=%ERRLVL%
@EXIT %ERRLVL%

