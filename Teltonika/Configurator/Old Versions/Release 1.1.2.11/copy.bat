@ECHO OFF
CLS
SETLOCAL

IF /i "%1" == "" GOTO ERROR

SET DESTINATIONPATH="FM11XX_Configurator_%1"
rd /S /Q %DESTINATIONPATH%
md %DESTINATIONPATH%

copy /y "changelog.txt" %DESTINATIONPATH%
copy /y "default.xml" %DESTINATIONPATH%
copy /y "FM11XX Configurator.exe" %DESTINATIONPATH%
copy /y "FM11XX Configurator.exe.config" %DESTINATIONPATH%
copy /y "FM11XXConfigurator.Common.dll" %DESTINATIONPATH%
copy /y "FM11XXConfigurator.Configuration.dll" %DESTINATIONPATH%
copy /y "FM11XXConfigurator.Protocol.dll" %DESTINATIONPATH%
copy /y "Profile.xsd" %DESTINATIONPATH%
copy /y "log4net.config.xml" %DESTINATIONPATH%
copy /y "log4net.dll" %DESTINATIONPATH%
copy /y "TCoreLib.dll" %DESTINATIONPATH%
copy /y "Teltonika.Windows.Forms.dll" %DESTINATIONPATH%

GOTO END

:ERROR
echo You should specify version (AA.BB.CC.DD) as parameter
echo In example %~nx0 1.0.50.0

:END
ENDLOCAL