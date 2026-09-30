# Stage 1: build the API with the full .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish API/API.csproj -c Release -o /app

# Stage 2: run it on the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Folder for the SQLite database file
USER root
RUN mkdir -p /data && chown app /data
USER app

EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]