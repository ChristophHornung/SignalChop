dotnet build ../SignalChop.sln
$aspProcess = Start-Process -PassThru -FilePath ./bin/Debug/netcoreapp3.1/SignalChop.Example -WorkingDirectory ./bin/Debug/netcoreapp3.1/
../SignalChop/bin/Debug/netcoreapp3.1/SignalChop --command-file Example.ChopCommands
$aspProcess | Stop-Process
