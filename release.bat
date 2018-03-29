@ECHO OFF

SET param_ver=%1
SET param_label=%2
SET ver_joined=%1-%2

IF "%param_ver%" == "" (
    ECHO Specify release version. 1>&2
    EXIT 1
)

IF "%param_label%" == "" (
    SET ver_joined=%1
)

ECHO Releasing the application with version %ver_joined%

msbuild /p:Configuration=Release /p:AssemblyVersion=%param_ver% /p:AssemblyVersionSuffix=%param_label%
nuget pack build/NPolyglot.Core.nuspec -properties configuration=Release;version=%ver_joined% -output build/
