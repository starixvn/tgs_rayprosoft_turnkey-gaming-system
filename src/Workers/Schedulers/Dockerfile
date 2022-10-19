#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/Workers/Schedulers/Schedulers.csproj", "src/Workers/Schedulers/"]
COPY ["src/Repositories/GameProviders.Repositories/GameProviders.Repositories.csproj", "src/Repositories/GameProviders.Repositories/"]
COPY ["src/Services/Operations/Operations.csproj", "src/Services/Operations/"]
COPY ["src/Shared/Game.ApiClients/Game.ApiClients.csproj", "src/Shared/Game.ApiClients/"]
COPY ["src/Shared/Game.Models/Game.Models.csproj", "src/Shared/Game.Models/"]
RUN dotnet restore "src/Workers/Schedulers/Schedulers.csproj"
COPY . .
WORKDIR "/src/src/Workers/Schedulers"
RUN dotnet build "Schedulers.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Schedulers.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Workers.Schedulers.dll"]