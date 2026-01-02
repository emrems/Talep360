FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Adım: Proje dosyasını kopyala
COPY ["Services/OcelotGateway/OcelotGateway.csproj", "Services/OcelotGateway/"]

# 2. Adım: Restore et
RUN dotnet restore "Services/OcelotGateway/OcelotGateway.csproj"

# 3. Adım: Tüm kodu kopyala
COPY . .

# 4. Adım: İçeri gir ve derle
WORKDIR "/src/Services/OcelotGateway"
RUN dotnet build "OcelotGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OcelotGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotGateway.dll"]