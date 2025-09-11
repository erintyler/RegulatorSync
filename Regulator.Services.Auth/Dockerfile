FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

# Install NativeAOT build prerequisites
RUN apk update \
    && apk add --no-cache \
       clang zlib-dev

WORKDIR /src
COPY ["Regulator.Services.Auth/Regulator.Services.Auth.csproj", "Regulator.Services.Auth/"]
RUN dotnet restore "Regulator.Services.Auth/Regulator.Services.Auth.csproj"
COPY . .
WORKDIR "/src/Regulator.Services.Auth"
RUN dotnet build "./Regulator.Services.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Regulator.Services.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["Regulator.Services.Auth"]
