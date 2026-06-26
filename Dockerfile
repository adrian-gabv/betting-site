FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY API/API.csproj API/
RUN dotnet restore API/API.csproj
COPY API/ API/
WORKDIR /src/API
RUN dotnet publish API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]
