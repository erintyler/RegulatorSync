FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Regulator.Services.Files/Regulator.Services.Files.csproj", "Regulator.Services.Files/"]
RUN dotnet restore "Regulator.Services.Files/Regulator.Services.Files.csproj"
COPY . .
WORKDIR "/src/Regulator.Services.Files"
RUN dotnet build "./Regulator.Services.Files.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Regulator.Services.Files.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Regulator.Services.Files.dll"]
