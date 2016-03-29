@ECHO OFF

ECHO Clearing Pacakges Directory
IF EXIST Packages (RMDIR Packages /s /q) 
MKDIR Packages

ECHO Packaging
nuget pack AppPromo.nuspec -OutputDirectory Packages
