FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8002
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ProductService.Api/ProductService.Api.csproj", "src/ProductService.Api/"]
COPY ["src/ProductService.Application/ProductService.Application.csproj", "src/ProductService.Application/"]
COPY ["src/ProductService.Domain/ProductService.Domain.csproj", "src/ProductService.Domain/"]
COPY ["src/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "src/ProductService.Infrastructure/"]
RUN dotnet restore "src/ProductService.Api/ProductService.Api.csproj"
COPY . .
WORKDIR "/src/src/ProductService.Api"
RUN dotnet build "./ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
