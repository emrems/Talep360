FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Adım: Proje dosyasını ana dizinden ilgili klasöre kopyala
COPY ["Services/TalepService/TalepService.csproj", "Services/TalepService/"]

# 2. Adım: Bağımlılıkları çöz (Restore)
RUN dotnet restore "Services/TalepService/TalepService.csproj"

# 3. Adım: Tüm kaynak kodunu kopyala
COPY . .

# 4. Adım: Çalışma dizinini projenin içine çek ve derle
WORKDIR "/src/Services/TalepService"
RUN dotnet build "TalepService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TalepService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalepService.dll"]