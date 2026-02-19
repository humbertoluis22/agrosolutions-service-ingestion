# 🚀 Agrosolutions Service Ingestion

[![Deploy to EKS](https://github.com/YOUR_ORG/agrosolutions-service-ingestion/actions/workflows/deploy.yml/badge.svg)](https://github.com/YOUR_ORG/agrosolutions-service-ingestion/actions/workflows/deploy.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![AWS](https://img.shields.io/badge/AWS-SNS-orange.svg)](https://aws.amazon.com/sns/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-EKS-blue.svg)](https://aws.amazon.com/eks/)

`agrosolutions-service-ingestion` é uma API Raw de alta performance responsável pela ingestão, validação e enfileiramento de dados de sensores agrícolas para processamento posterior.

# 🎯 Objetivos

 - Receber cargas de dados (payloads) de sensores via HTTP.
 - Garantir desacoplamento e alta disponibilidade utilizando RabbitMQ.
 - Fornecer endpoints de health check para monitoramento de infraestrutura.

# 📃 Funcionalidades principais:

 - **Ingestão de Dados**: Endpoint único (`POST /api/ingestion`) capaz de processar dados polimórficos.
 - **Validação Polimórfica**: Utiliza FluentValidation para aplicar regras específicas dependendo do `type_sensor` (ex: valida pH para sensores de solo, CO₂ para silos).
 - **Publicação Fanout**: Encaminha mensagens válidas para uma Exchange Fanout no RabbitMQ (`sensor_raw_data`), permitindo múltiplos consumidores futuros.
 - **Monitoramento**: Rota de `/health` para verificação de disponibilidade.

# 🔍 Validação e Dados

A API não persiste dados diretamente no banco relacional, focando em throughput. Ela valida a integridade da mensagem antes do enfileiramento.

A API expõe o seguinte endpoint principal:

 - **Ingestão (`POST /api/ingestion`)**:
     - Recebe um JSON contendo `FieldId`, `SensorId`, `TypeSensor`,`TimeStamp` e um objeto `Data` dinâmico.
     - Retorna `202 Accepted` se o JSON for válido e enfileirado com sucesso.
     - Retorna `400 Bad Request` com detalhes do erro se o schema do objeto `data` não corresponder ao tipo de sensor.

# ⚙️ Dependências

O projeto utiliza as seguintes bibliotecas principais:

 - Microsoft.AspNetCore.OpenApi
 - FluentValidation.AspNetCore (Validação robusta)
 - RabbitMQ.Client (Mensageria)
 - Swashbuckle.AspNetCore (Swagger)

# 🔄️ Fluxo de Ingestão

```mermaid
graph TD
    A["Worker Gerador (HTTP)"] -->|POST /api/ingestion| B["API Ingestion"]
    B -->|Valida Schema JSON| C{Dados Válidos?}
    C -- Sim --> D["Serializa Mensagem"]
    D -->|BasicPublish| E["RabbitMQ (Exchange: sensor_raw_data)"]
    C -- Não --> F["Retorna HTTP 400"]
```

# 🏗️ Arquitetura e Deployment

## Estrutura do Projeto

```
agrosolutions-service-ingestion/
├── src/
│   ├── Api/                    # Controllers, Program.cs
│   ├── Application/            # Use Cases, Validators
│   ├── Infrastructure/         # Messaging (SNS, RabbitMQ)
│   └── Shared/                 # DTOs, Enums
├── k8s/production/             # Kubernetes manifests
│   ├── namespace.yaml
│   ├── deployment.yaml         # 2-10 replicas autoscaling
│   ├── services.yaml
│   ├── hpa.yaml                # Horizontal Pod Autoscaler
│   ├── observability.yaml      # Prometheus metrics & alerts
│   └── resource-configs.yaml   # NetworkPolicy, PDB, Quotas
├── .github/workflows/
│   └── deploy.yml              # CI/CD pipeline
└── Dockerfile                  # Multi-stage .NET 10 Alpine
```

## Deployment em Produção (AWS EKS)

### 🚀 CI/CD Automático

Push para `main` dispara automaticamente:
1. ✅ Build e testes
2. ✅ Build da imagem Docker
3. ✅ Push para ECR (`316295889438.dkr.ecr.sa-east-1.amazonaws.com/agrosolutions-ingestion-api`)
4. ✅ Deploy no EKS cluster
5. ✅ Aguarda rollout e verifica saúde

Ver: [k8s/CI_CD_SUMMARY.md](k8s/CI_CD_SUMMARY.md)

### 📊 Recursos Kubernetes

- **Namespace**: `agrosolutions-ingestion`
- **Replicas**: 2 mínimo, 10 máximo (autoscaling por CPU/Memória)
- **Port**: Container `8080`, Service `80`
- **Resources**: 256Mi-512Mi RAM, 200m-1000m CPU
- **Probes**: Liveness e Readiness em `/health`
- **Observability**: Prometheus ServiceMonitor + Alertas

### 🔐 Secrets Necessários

Configurar no GitHub (`Settings > Secrets`):
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`

Ver: [.github/SECRETS_SETUP.md](.github/SECRETS_SETUP.md)

## Deploy Manual

```bash
# 1. Configurar kubectl
aws eks update-kubeconfig --name agrosolutions-eks-cluster --region sa-east-1

# 2. Aplicar manifestos
kubectl apply -f k8s/production/

# 3. Verificar deployment
kubectl get pods -n agrosolutions-ingestion
kubectl logs -n agrosolutions-ingestion -l app=ingestion-api --tail=50 -f
```

Ver documentação completa: [k8s/README.md](k8s/README.md)

# 💻 Desenvolvimento Local

## Requisitos

- .NET SDK 10.0
- Docker e Docker Compose
- AWS CLI (configurado com credenciais)

## Executar Localmente

```bash
# Opção 1: Diretamente com .NET
dotnet run --project src/AgrosolutionsServiceIngestion.Api/AgrosolutionsServiceIngestion.Api.csproj

# Opção 2: Via Docker Compose
docker-compose up -d

# Acessar: http://localhost:5198/swagger
# Health: http://localhost:5198/health
```
