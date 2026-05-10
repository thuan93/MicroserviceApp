FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["MicroserviceApp.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]
COPY ["src/ApiGateways/OcelotApiGw/", "src/ApiGateways/OcelotApiGw/"]
RUN dotnet restore "src/ApiGateways/OcelotApiGw/OcelotApiGw.csproj"
RUN dotnet publish "src/ApiGateways/OcelotApiGw/OcelotApiGw.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OcelotApiGw.dll"]
