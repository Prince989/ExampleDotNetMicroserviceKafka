FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SearchService.Api/SearchService.Api.csproj", "src/SearchService.Api/"]
COPY ["src/SearchService.Application/SearchService.Application.csproj", "src/SearchService.Application/"]
COPY ["src/SearchService.Domain/SearchService.Domain.csproj", "src/SearchService.Domain/"]
COPY ["src/SearchService.Infrastructure/SearchService.Infrastructure.csproj", "src/SearchService.Infrastructure/"]
RUN dotnet restore "src/SearchService.Api/SearchService.Api.csproj"
COPY . .
WORKDIR "/src/src/SearchService.Api"
RUN dotnet build "./SearchService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SearchService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]
