XCOPY ..\Output\BridgeRT\Win32-Release\BridgeRT.winmd BridgeRT\lib\uap10.0\ /Y
XCOPY ..\Output\BridgeRT\Win32-Release\BridgeRT.dll BridgeRT\runtimes\win10-x86\native\ /Y
XCOPY ..\Output\BridgeRT\x64-Release\BridgeRT.dll BridgeRT\runtimes\win10-x64\native\ /Y
XCOPY ..\Output\BridgeRT\ARM-Release\BridgeRT.dll BridgeRT\runtimes\win10-arm\native\ /Y
XCOPY dotMorten.AllJoyn.DSB.Native.nuspec BridgeRT\ /Y
XCOPY dotMorten.AllJoyn.DSB.Native.targets BridgeRT\build\native\ /Y
nuget pack BridgeRT\dotMorten.AllJoyn.DSB.Native.nuspec
RMDIR BridgeRT /S /Q