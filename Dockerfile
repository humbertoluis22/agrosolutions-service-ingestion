ARG DOTNET_VERSION=10.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS build
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

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

COPY --from=build /app/publish .

# Define permissões de usuário não-root (boa prática de segurança)
RUN chown -R 0:0 /app && chmod -R g+w /app

EXPOSE 5198
ENTRYPOINT ["dotnet", "AgrosolutionsServiceIngestion.Api.dll"]