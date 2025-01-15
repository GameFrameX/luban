
dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r win-x64 --self-contained -o ./publish/windows -p:PublishSingleFile=true
dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r win-arm64 --self-contained -o ./publish/windows-arm64 -p:PublishSingleFile=true

dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r linux-x64 --self-contained -o ./publish/linux-x64 -p:PublishSingleFile=true
dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r linux-arm64 --self-contained -o ./publish/linux-arm64 -p:PublishSingleFile=true

dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r osx-x64 --self-contained -o ./publish/mac-intel -p:PublishSingleFile=true
dotnet publish -c Release /p:DebugSymbols=false /p:DebugType=none -r osx-arm64 --self-contained -o ./publish/mac-arm64 -p:PublishSingleFile=true
