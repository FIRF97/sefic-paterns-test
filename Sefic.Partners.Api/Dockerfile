FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Sefic.Partners.Api/Sefic.Partners.Api.csproj", "Sefic.Partners.Api/"]
COPY ["Sefic.Partners.Core/Sefic.Partners.Core.csproj", "Sefic.Partners.Core/"]
COPY ["Sefic.Partners.Data/Sefic.Partners.Data.csproj", "Sefic.Partners.Data/"]
COPY ["Sefic.Partners.Services/Sefic.Partners.Services.csproj", "Sefic.Partners.Services/"]
COPY ["Sefic.Partners.Shared/Sefic.Partners.Shared.csproj", "Sefic.Partners.Shared/"]

RUN dotnet restore "Sefic.Partners.Api/Sefic.Partners.Api.csproj"

COPY . .
WORKDIR "/src/Sefic.Partners.Api"
RUN dotnet build "Sefic.Partners.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sefic.Partners.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sefic.Partners.Api.dll"]
