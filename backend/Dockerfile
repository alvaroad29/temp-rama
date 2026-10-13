# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia archivos del proyecto
COPY backend.csproj .

# Restaura dependencias
RUN dotnet restore

# Copia código fuente (ya estoy en backend)
COPY . .

# Compila la app
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copia artefactos compilados
COPY --from=build /app/publish .

# Expone el puerto (HTTPS por defecto en .NET 9)
EXPOSE 8080

# Ejecuta la app
ENTRYPOINT ["dotnet", "backend.dll"]