#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/Apis/TgsApi/TgsApi.csproj", "src/Apis/TgsApi/"]
COPY ["src/Repositories/GameProviders.Repositories/GameProviders.Repositories.csproj", "src/Repositories/GameProviders.Repositories/"]
COPY ["src/Services/GameOperations/GameOperations.csproj", "src/Services/GameOperations/"]
COPY ["src/Services/Operations/Operations.csproj", "src/Services/Operations/"]
COPY ["src/Services/PluggableServices/PluggableServices.csproj", "src/Services/PluggableServices/"]
COPY ["src/Shared/Game.ApiClients/Game.ApiClients.csproj", "src/Shared/Game.ApiClients/"]
COPY ["src/Shared/Game.Models/Game.Models.csproj", "src/Shared/Game.Models/"]
RUN dotnet restore "src/Apis/TgsApi/TgsApi.csproj"
COPY . .
WORKDIR "/src/src/Apis/TgsApi"
RUN dotnet build "TgsApi.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "TgsApi.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Apis.TgsApi.dll"]