# Usar la imagen base oficial de .NET 8 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Usar la imagen del SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["florentia/florentia.csproj", "florentia/"]
RUN dotnet restore "florentia/florentia.csproj"
COPY . .
WORKDIR "/src/florentia"
RUN dotnet build "florentia.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "florentia.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "florentia.dll"]
