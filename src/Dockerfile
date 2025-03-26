FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Luban/Luban.csproj", "Luban/"]
COPY ["Luban.Bson/Luban.Bson.csproj", "Luban.Bson/"]
COPY ["Luban.Core/Luban.Core.csproj", "Luban.Core/"]
COPY ["Luban.Cpp/Luban.Cpp.csproj", "Luban.Cpp/"]
COPY ["Luban.CSharp/Luban.CSharp.csproj", "Luban.CSharp/"]
COPY ["Luban.DataValidator.Builtin/Luban.DataValidator.Builtin.csproj", "Luban.DataValidator.Builtin/"]
COPY ["Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj", "Luban.DataTarget.Builtin/"]
COPY ["Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj", "Luban.DataLoader.Builtin/"]
COPY ["Luban.FlatBuffers/Luban.FlatBuffers.csproj", "Luban.FlatBuffers/"]
COPY ["Luban.Gdscript/Luban.Gdscript.csproj", "Luban.Gdscript/"]
COPY ["Luban.Golang/Luban.Golang.csproj", "Luban.Golang/"]
COPY ["Luban.Java/Luban.Java.csproj", "Luban.Java/"]
COPY ["Luban.L10N/Luban.L10N.csproj", "Luban.L10N/"]
COPY ["Luban.Lua/Luban.Lua.csproj", "Luban.Lua/"]
COPY ["Luban.MsgPack/Luban.MsgPack.csproj", "Luban.MsgPack/"]
COPY ["Luban.PHP/Luban.PHP.csproj", "Luban.PHP/"]
COPY ["Luban.Protobuf/Luban.Protobuf.csproj", "Luban.Protobuf/"]
COPY ["Luban.Python/Luban.Python.csproj", "Luban.Python/"]
COPY ["Luban.Rust/Luban.Rust.csproj", "Luban.Rust/"]
COPY ["Luban.Schema.Builtin/Luban.Schema.Builtin.csproj", "Luban.Schema.Builtin/"]
COPY ["Luban.Typescript/Luban.Typescript.csproj", "Luban.Typescript/"]
RUN dotnet restore "Luban/Luban.csproj"
COPY . .
WORKDIR "/src/Luban"
RUN dotnet build "Luban.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Luban.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Luban.dll"]
