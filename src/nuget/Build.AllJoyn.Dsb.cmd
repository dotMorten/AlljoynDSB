CALL "%VS140COMNTOOLS%\VsDevCmd.bat"
XCOPY ..\Output\AllJoyn.Dsb\x86-Release\AllJoyn.Dsb.dll AllJoyn.Dsb\runtimes\win10-x86\lib\uap10.0\ /Y
XCOPY ..\Output\AllJoyn.Dsb\x64-Release\AllJoyn.Dsb.dll AllJoyn.Dsb\runtimes\win10-x64\lib\uap10.0\ /Y
XCOPY ..\Output\AllJoyn.Dsb\ARM-Release\AllJoyn.Dsb.dll AllJoyn.Dsb\runtimes\win10-arm\lib\uap10.0\ /Y
XCOPY ..\Output\AllJoyn.Dsb\x86-Release\AllJoyn.Dsb.dll AllJoyn.Dsb\ref\uap10.0\ /Y
XCOPY ..\Output\AllJoyn.Dsb\x86-Release\AllJoyn.Dsb.pri AllJoyn.Dsb\ref\uap10.0\ /Y
corflags.exe /32bitreq- AllJoyn.Dsb\ref\uap10.0\AllJoyn.Dsb.dll
XCOPY dotMorten.AllJoyn.DSB.nuspec AllJoyn.Dsb\ /Y
nuget pack AllJoyn.Dsb\dotMorten.AllJoyn.DSB.nuspec
RMDIR AllJoyn.Dsb /S /Q