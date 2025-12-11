# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Kiwi2Shop.identity/Kiwi2Shop.identity.csproj", "Kiwi2Shop.identity/"]
COPY ["Kiwi2Shop.ServiceDefaults/Kiwi2Shop.ServiceDefaults.csproj", "Kiwi2Shop.ServiceDefaults/"]
COPY ["Kiwi2Shop.Shared/Kiwi2Shop.Shared.csproj", "Kiwi2Shop.Shared/"]
RUN dotnet restore "./Kiwi2Shop.identity/Kiwi2Shop.identity.csproj"
COPY . .
WORKDIR "/src/Kiwi2Shop.identity"
RUN dotnet build "./Kiwi2Shop.identity.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kiwi2Shop.identity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kiwi2Shop.identity.dll"]