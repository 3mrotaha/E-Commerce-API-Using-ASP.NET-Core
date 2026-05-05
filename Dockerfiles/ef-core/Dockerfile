FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

COPY eCommerceSolution.slnx ./

COPY src/eCommerce.Api/eCommerce.Api.csproj src/eCommerce.Api/
COPY src/eCommerce.Application/eCommerce.Application.csproj src/eCommerce.Application/
COPY src/eCommerce.Domain/eCommerce.Domain.csproj src/eCommerce.Domain/
COPY src/eCommerce.Infrastructure/eCommerce.Infrastructure.csproj src/eCommerce.Infrastructure/
COPY src/eCommerce.Persistence/eCommerce.Persistence.csproj src/eCommerce.Persistence/

RUN dotnet restore eCommerceSolution.slnx

COPY src/ src/

RUN dotnet tool install --global dotnet-ef

ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "ef"]