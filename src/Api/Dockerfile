FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/ProyectoAvengers.Shared.csproj src/Shared/
COPY src/Domain/ProyectoAvengers.Domain.csproj src/Domain/
COPY src/Application/ProyectoAvengers.Application.csproj src/Application/
COPY src/Infrastructure/ProyectoAvengers.Infrastructure.csproj src/Infrastructure/
COPY src/Api/ProyectoAvengers.Api.csproj src/Api/
RUN dotnet restore src/Api/ProyectoAvengers.Api.csproj

COPY . .
RUN dotnet publish src/Api/ProyectoAvengers.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --gid 1000 appuser

COPY --from=build /app .
RUN mkdir -p /app/uploads && chown -R appuser:appgroup /app

USER appuser
ENTRYPOINT ["dotnet", "ProyectoAvengers.Api.dll"]
