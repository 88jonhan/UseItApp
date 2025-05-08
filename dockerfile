FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Kopiera csproj och återställ beroenden
COPY UseItApp.API/*.csproj ./UseItApp.API/
COPY *.sln .
RUN dotnet restore

# Kopiera resten av koden
COPY . .

# Publicera
RUN dotnet publish UseItApp.API/UseItApp.API.csproj -c Release -o /app/publish

# Bygg slutlig image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UseItApp.API.dll"]