ARG DOTNET_VERSION=10.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build

ARG BUILD_DATE
ARG VERSION=1.0.0
ARG REVISION=dev

WORKDIR /src

# Instala bibliotecas necessárias para globalização no Alpine
RUN apk add --no-cache icu-libs

# Copia o arquivo de solução e restaura dependências
COPY *.slnx .
COPY src/AgrosolutionsServiceIngestion.Shared/AgrosolutionsServiceIngestion.Shared.csproj ./src/AgrosolutionsServiceIngestion.Shared/
COPY src/AgrosolutionsServiceIngestion.Domain/AgrosolutionsServiceIngestion.Domain.csproj ./src/AgrosolutionsServiceIngestion.Domain/
COPY src/AgrosolutionsServiceIngestion.Application/AgrosolutionsServiceIngestion.Application.csproj ./src/AgrosolutionsServiceIngestion.Application/
COPY src/AgrosolutionsServiceIngestion.Infrastructure/AgrosolutionsServiceIngestion.Infrastructure.csproj ./src/AgrosolutionsServiceIngestion.Infrastructure/
COPY src/AgrosolutionsServiceIngestion.Api/AgrosolutionsServiceIngestion.Api.csproj ./src/AgrosolutionsServiceIngestion.Api/

RUN dotnet restore

# Copia todo o restante do código
COPY src/ ./src/

# Publica a API
RUN dotnet publish src/AgrosolutionsServiceIngestion.Api/AgrosolutionsServiceIngestion.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final

# Labels para metadata
LABEL maintainer="AgroSolutions Team" \
      org.opencontainers.image.title="AgroSolutions Ingestion API" \
      org.opencontainers.image.description="API de ingestão de dados de sensores agrícolas" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.created="${BUILD_DATE}"

# Instalar dependências
RUN apk add --no-cache \
    icu-libs \
    tzdata \
    ca-certificates

# Criar usuário não-root
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup

WORKDIR /app

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Configurar variáveis de ambiente
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    TZ=America/Sao_Paulo

# Mudar ownership dos arquivos para appuser
RUN chown -R appuser:appgroup /app

# Trocar para usuário não-root
USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AgrosolutionsServiceIngestion.Api.dll"]