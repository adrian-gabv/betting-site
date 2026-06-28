FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY src/BettingSite.Domain/BettingSite.Domain.csproj           src/BettingSite.Domain/
COPY src/BettingSite.Application/BettingSite.Application.csproj  src/BettingSite.Application/
COPY src/BettingSite.Infrastructure/BettingSite.Infrastructure.csproj src/BettingSite.Infrastructure/
COPY src/BettingSite.API/BettingSite.API.csproj                  src/BettingSite.API/

RUN dotnet restore src/BettingSite.API/BettingSite.API.csproj

COPY src/ src/

RUN dotnet publish src/BettingSite.API/BettingSite.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BettingSite.API.dll"]
