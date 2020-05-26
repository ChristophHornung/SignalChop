dotnet restore
dotnet build
Remove-Item Release -Recurse -Force -EA SilentlyContinue
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r win-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/win-x64
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r win-arm -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/win-arm
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r win-arm64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/win-arm64
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/linux-x64
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r linux-arm -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/linux-arm
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r linux-arm64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/linux-arm64
dotnet publish .\SignalChop\Crosberg.SignalChop.csproj -r osx-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -o Release/osx-x64
