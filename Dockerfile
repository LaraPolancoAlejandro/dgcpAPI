# Usa el SDK de .NET 7.0 como imagen base
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Establece el directorio de trabajo en /src
WORKDIR /src

# Copia el proyecto .csproj y restaura las dependencias
COPY ["dgcp.api/dgcp.api.csproj", "dgcp.api/"]
RUN dotnet restore "dgcp.api/dgcp.api.csproj"

# Copia el resto de los archivos del proyecto
COPY . .
WORKDIR "/src/dgcp.api"

# Publica la aplicación
RUN dotnet publish "dgcp.api.csproj" -c Release -o /app/publish

# Usa ASP.NET 7.0 como imagen base
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

# Establece el directorio de trabajo en /app
WORKDIR /app

# Copia los archivos publicados desde la imagen de construcción
COPY --from=build /app/publish .

EXPOSE 8080

# Inicia la aplicación
ENTRYPOINT ["dotnet", "dgcp.api.dll"]
