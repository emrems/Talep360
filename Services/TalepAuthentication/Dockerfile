# Temel çalışma ortamı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Derleme aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Proje dosyasını kopyala ve bağımlılıkları yükle
COPY ["Services/TalepAuthentication/TalepAuthentication.csproj", "Services/TalepAuthentication/"]
RUN dotnet restore "Services/TalepAuthentication/TalepAuthentication.csproj"

# Tüm kaynak kodunu kopyala
COPY . .

# Proje dizinine git ve build et
WORKDIR "/src/Services/TalepAuthentication"
RUN dotnet build "TalepAuthentication.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Yayınlama aşaması
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TalepAuthentication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final imajı
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalepAuthentication.dll"]