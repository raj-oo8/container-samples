# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV AzureAd__Instance="https://login.microsoftonline.com/"
ENV AzureAd__TenantId=""
ENV AzureAd__ClientId=""
ENV Logging__LogLevel__Default="Information"
ENV Logging__LogLevel__Microsoft_AspNetCore="Warning"
ENV AllowedHosts="*"
ENV Kestrel__EndpointDefaults__Protocols="Http1AndHttp2AndHttp3"
ENV CosmosDb__Endpoint=""
ENV KeyVault__AppInsightsConnectionString=""
ENV KeyVault__Url=""

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AspNet/AspNet.Grpc.Api/AspNet.Grpc.Api.csproj", "AspNet/AspNet.Grpc.Api/"]
RUN dotnet restore "./AspNet/AspNet.Grpc.Api/AspNet.Grpc.Api.csproj"
COPY . .
WORKDIR "/src/AspNet/AspNet.Grpc.Api"
RUN dotnet build "./AspNet.Grpc.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AspNet.Grpc.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AspNet.Grpc.Api.dll"]