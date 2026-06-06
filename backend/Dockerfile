# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/MediConnect.Domain/MediConnect.Domain.csproj backend/MediConnect.Domain/
COPY backend/MediConnect.Application/MediConnect.Application.csproj backend/MediConnect.Application/
COPY backend/MediConnect.Infrastructure/MediConnect.Infrastructure.csproj backend/MediConnect.Infrastructure/
COPY backend/MediConnect.WebApi/MediConnect.WebApi.csproj backend/MediConnect.WebApi/
RUN dotnet restore backend/MediConnect.WebApi/MediConnect.WebApi.csproj

COPY backend/ backend/
RUN dotnet publish backend/MediConnect.WebApi/MediConnect.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MediConnect.WebApi.dll"]
