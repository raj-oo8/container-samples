# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV AzureAd__Instance="https://login.microsoftonline.com/"
ENV AzureAd__Domain=""
ENV AzureAd__TenantId=""
ENV AzureAd__ClientId=""
ENV AzureAd__CallbackPath="/signin-oidc"
ENV AzureAd__ClientSecret=""
ENV Logging__LogLevel__Default="Information"
ENV Logging__LogLevel__Microsoft_AspNetCore="Warning"
ENV AllowedHosts="*"
ENV DownstreamApi__BaseUrl=""
ENV DownstreamApi__Scopes__0=""
ENV IsPreviewEnabled=false

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AspNet/AspNet.Mvc/AspNet.Mvc.csproj", "AspNet/AspNet.Mvc/"]
RUN dotnet restore "./AspNet/AspNet.Mvc/AspNet.Mvc.csproj"
COPY . .
WORKDIR "/src/AspNet/AspNet.Mvc"
RUN dotnet build "./AspNet.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AspNet.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNet.Mvc.dll"]