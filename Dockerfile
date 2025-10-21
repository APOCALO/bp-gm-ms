# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Exponer el puerto en el contenedor
EXPOSE 8080

# Copiar archivos de proyecto
COPY *.sln ./
COPY Domain/*.csproj ./Domain/
COPY Application/*.csproj ./Application/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Web.Api/*.csproj ./Web.Api/
COPY Web.Api.Test/*.csproj ./Web.Api.Test/

# Restaurar dependencias
RUN dotnet restore

# Copiar todo el código fuente
COPY . .

# Compilar el proyecto
WORKDIR "/src/Web.Api"
RUN dotnet build "Web.Api.csproj" -c Release -o /app/build

# Publicar la aplicación para producción
RUN dotnet publish "Web.Api.csproj" -c Release -o /app/publish --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Establecer el punto de entrada
ENTRYPOINT ["dotnet", "Web.Api.dll"]
