if NOT EXIST nuget\lib md nuget\lib
if NOT EXIST nuget\lib\net45 md nuget\lib\net45
if NOT EXIST nuget\lib\net40 md nuget\lib\net40
copy PRI.EffectiveIoC\bin\Release\PRI.EffectiveIoC.dll nuget\lib\net45
copy PRI.EffectiveIoC\bin\Release\PRI.EffectiveIoC.xml nuget\lib\net45
copy PRI.EffectiveIoC.net40\bin\Release\PRI.EffectiveIoC.dll nuget\lib\net40
copy PRI.EffectiveIoC.net40\bin\Release\PRI.EffectiveIoC.xml nuget\lib\net40
pushd nuget
..\util\nuget.exe pack EffectiveIoC.nuspec 
popd
@echo run "..\util\nuget.exe push EffectiveIoC.1.0.3.0.nupkg" to publish.
