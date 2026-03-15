FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebRTCSignalServer/WebRTCSignalServer.csproj", "WebRTCSignalServer/"]
RUN dotnet restore "WebRTCSignalServer/WebRTCSignalServer.csproj"
COPY . .
WORKDIR "/src/WebRTCSignalServer"
RUN dotnet build "WebRTCSignalServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebRTCSignalServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebRTCSignalServer.dll"]
