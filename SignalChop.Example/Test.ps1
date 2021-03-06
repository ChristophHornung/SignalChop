dotnet build ../SignalChop.sln
$aspProcess = Start-Process -PassThru -FilePath ./bin/Debug/netcoreapp3.1/SignalChop.Example -WorkingDirectory ./bin/Debug/netcoreapp3.1/
../SignalChop/bin/Debug/netcoreapp3.1/SignalChop --command-file Example.ChopCommands > ./bin/Debug/netcoreapp3.1/reply.json
$result = "./bin/Debug/netcoreapp3.1/reply.json"
$expected = "./reply.json"

if((Get-FileHash $result).hash  -ne (Get-FileHash $expected).hash)
{
"TEST FAILED"
}
else {
"TEST SUCCESS"
}

$aspProcess | Stop-Process
