# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV AzureAd__Instance="https://login.microsoftonline.com/"
ENV AzureAd__Domain="rjsse8outlook.onmicrosoft.com"
ENV AzureAd__TenantId="b892c9c4-f38d-4359-84dc-3382f8dba4ed"
ENV AzureAd__ClientId="457cdf20-48af-4e0c-a708-45ae08ba7a09"
ENV AzureAd__CallbackPath="/signin-oidc"
ENV AzureAd__ClientSecret="your-client-secret"
ENV Logging__LogLevel__Default="Information"
ENV Logging__LogLevel__Microsoft_AspNetCore="Warning"
ENV AllowedHosts="*"
ENV DownstreamApi__BaseUrl="https://localhost:32819"
ENV DownstreamApi__Scopes__0="api://20617e17-61c9-4b69-b842-e30adc5d3570/Forecast.Read"
ENV IsPreviewEnabled=true

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