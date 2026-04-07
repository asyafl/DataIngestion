FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DataIngestion.Api/DataIngestion.Api.csproj", "DataIngestion.Api/"]
COPY ["DataIngestion.Application/DataIngestion.Application.csproj", "DataIngestion.Application/"]
COPY ["DataIngestion.Domain/DataIngestion.Domain.csproj", "DataIngestion.Domain/"]
COPY ["DataIngestion.Infrastructure/DataIngestion.Infrastructure.csproj", "DataIngestion.Infrastructure/"]

RUN dotnet restore "DataIngestion.Api/DataIngestion.Api.csproj"

COPY . .

WORKDIR "/src/DataIngestion.Api"
RUN dotnet publish "DataIngestion.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DataIngestion.Api.dll"]