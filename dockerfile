FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Kopiera .csproj-filer först för att utnyttja Docker-cache
COPY UseItApp.API/*.csproj ./UseItApp.API/
COPY UseItApp.Domain/*.csproj ./UseItApp.Domain/
COPY UseItApp.Data/*.csproj ./UseItApp.Data/
COPY UseItApp.Tests/*.csproj ./UseItApp.Tests/
# Ändra dessa sökvägar efter din projektstruktur

# Kör restore separat för att förbättra caching
RUN dotnet restore ./LendingApp.API/LendingApp.API.csproj

# Kopiera resten av filerna
COPY . .

# Publicera API-projektet
RUN dotnet publish ./UseItApp.API/UseItApp.API.csproj -c Release -o out

# Bygg runtime-image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Exponera port
EXPOSE 80
EXPOSE 443

# Starta applikationen
ENTRYPOINT ["dotnet", "UseItApp.API.dll"]