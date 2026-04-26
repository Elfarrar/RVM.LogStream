FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY nuget.config .
COPY ["src/RVM.LogStream.API/RVM.LogStream.API.csproj", "src/RVM.LogStream.API/"]
COPY ["src/RVM.LogStream.Domain/RVM.LogStream.Domain.csproj", "src/RVM.LogStream.Domain/"]
COPY ["src/RVM.LogStream.Infrastructure/RVM.LogStream.Infrastructure.csproj", "src/RVM.LogStream.Infrastructure/"]
RUN dotnet restore "./src/RVM.LogStream.API/RVM.LogStream.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.LogStream.API"
RUN dotnet build "./RVM.LogStream.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RVM.LogStream.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.LogStream.API.dll"]
