# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY eCommerceSolution.slnx ./

# Copy project files first for better Docker layer caching
COPY src/eCommerce.Api/eCommerce.Api.csproj src/eCommerce.Api/
COPY src/eCommerce.Application/eCommerce.Application.csproj src/eCommerce.Application/
COPY src/eCommerce.Domain/eCommerce.Domain.csproj src/eCommerce.Domain/
COPY src/eCommerce.Infrastructure/eCommerce.Infrastructure.csproj src/eCommerce.Infrastructure/
COPY src/eCommerce.Persistence/eCommerce.Persistence.csproj src/eCommerce.Persistence/

# Restore dependencies
RUN dotnet restore eCommerceSolution.slnx

# Copy full source
COPY src/ src/

# Publish API project only
RUN dotnet publish src/eCommerce.Api/eCommerce.Api.csproj \
    -c Release \
    -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "eCommerce.Api.dll"]