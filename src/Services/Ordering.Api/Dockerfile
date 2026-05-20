FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["MicroserviceApp.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]
COPY ["src/Services/Ordering.Api/", "src/Services/Ordering.Api/"]
RUN dotnet restore "src/Services/Ordering.Api/Ordering.Api.csproj"
RUN dotnet publish "src/Services/Ordering.Api/Ordering.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
