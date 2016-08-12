CALL "%VS140COMNTOOLS%\VsDevCmd.bat"
set ERRORLEVEL=

MSBUILD "..\AllJoynDSB.sln" /p:Configuration=Release /p:Platform=x86 /verbosity:quiet
if ERRORLEVEL 1 (GOTO end) 
MSBUILD "..\AllJoynDSB.sln" /p:Configuration=Release /p:Platform=x64 /verbosity:quiet
if ERRORLEVEL 1 (GOTO end) 
MSBUILD "..\AllJoynDSB.sln" /p:Configuration=Release /p:Platform=ARM /verbosity:quiet
if ERRORLEVEL 1 (GOTO end) 

CALL Build.BridgeRT.cmd
if ERRORLEVEL 1 (GOTO end) 
CALL Build.AllJoyn.Dsb.cmd

:end