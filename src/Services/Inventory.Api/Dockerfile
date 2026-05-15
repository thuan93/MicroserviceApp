FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["MicroserviceApp.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]
COPY ["src/Services/Inventory.Api/", "src/Services/Inventory.Api/"]
RUN dotnet restore "src/Services/Inventory.Api/Inventory.Api.csproj"
RUN dotnet publish "src/Services/Inventory.Api/Inventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
