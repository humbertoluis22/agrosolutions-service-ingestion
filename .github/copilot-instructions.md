# Copilot instructions for agrosolutions-service-ingestion

## Architecture and boundaries
- This repo is a layered .NET 10 ingestion API split into `Api`, `Application`, `Infrastructure`, `Shared` (solution file: `AgrosolutionsServiceIngestion.slnx`).
- Request flow is: HTTP controller -> use case -> publisher interface -> concrete messaging adapter.
  - Controller: `src/AgrosolutionsServiceIngestion.Api/controllers/IngestionController.cs`
  - Use case: `src/AgrosolutionsServiceIngestion.Application/UseCase/PublishSensorRawUseCase.cs`
  - Port/interface: `src/AgrosolutionsServiceIngestion.Application/Interfaces/ISensorRawPublisher.cs`
  - Adapters: `src/AgrosolutionsServiceIngestion.Infrastructure/Messaging/*SensorRawPublisher.cs`
- `Application` should not depend on transport specifics. Keep messaging-provider details in `Infrastructure` and wire them in `Program.cs`.
- Source of truth for request contract is `Shared`, especially `SensorRawRequest` and `Shared.Enums.SensorType`.

## Runtime wiring patterns
- DI registration currently uses **AWS SNS** publisher (`SnsSensorRawPublisher`) as active implementation in `src/AgrosolutionsServiceIngestion.Api/Program.cs`.
- RabbitMQ publisher exists (`RabbitMqSensorRawPublisher`) but is currently commented out in DI.
- If changing provider, update only DI wiring and required environment/config values; keep `ISensorRawPublisher` contract stable.

## Validation and payload conventions
- Incoming payload is polymorphic: `SensorRawRequest.Data` is `JsonNode` (`src/AgrosolutionsServiceIngestion.Shared/DTOs/Request/SensorRawRequest.cs`).
- Validation is centralized in `SensorRawRequestValidator` and deserializes `Data` based on `TypeSensor`.
- Preserve current enum/value semantics from `src/AgrosolutionsServiceIngestion.Shared/Enums/SensorType.cs` (`Solo`, `Silo`, `Meteorologica`).
- Error messages in validator are in Portuguese; keep language/style consistent when editing validation rules.

## API and endpoint specifics
- Ingestion endpoint is `POST /api/ingestion/sensor` (controller route + action route). Keep docs/examples aligned with this path.
- Health endpoint is `/health` (`Program.cs`), and deployment probes depend on it.

## Build and local workflows
- Restore/build solution: `dotnet restore` then `dotnet build AgrosolutionsServiceIngestion.slnx`.
- Run API locally: `dotnet run --project src/AgrosolutionsServiceIngestion.Api/AgrosolutionsServiceIngestion.Api.csproj`.
- Local dev via Docker Compose: `docker-compose up -d` (port `5198` on host, `8080` in container).
- This repo currently has no test projects/scripts; do not invent test commands unless adding a real test project.

## Container and deployment integration

### Port consistency
- Container port: `8080` (standardized across all environments)
- Local dev: Access via `http://localhost:5198` (docker-compose maps 5198→8080)
- Kubernetes: Service exposes port `80` → targets container port `8080`
- Dockerfile sets `ASPNETCORE_URLS=http://+:8080` and `EXPOSE 8080`

### Docker
- Docker image is built from root `Dockerfile` using multi-stage build with .NET 10 Alpine.
- Runs as non-root user (UID 1001) for security.
- Includes healthcheck: `wget http://localhost:8080/health`.
- `docker-compose.yaml` injects AWS credentials and maps nested config keys via `AWS__*` env vars.

### Kubernetes (Production)
- Production manifests are in `k8s/production/` following enterprise-grade patterns.
- **Namespace**: `agrosolutions-ingestion` (isolated from other services).
- **Deployment**: 2 replicas minimum, rolling update strategy, resource limits (256Mi-512Mi RAM, 200m-1000m CPU).
- **Service**: ClusterIP on port 80 → targets container port 8080.
- **HPA**: Autoscales 2-10 pods based on CPU (70%) and memory (80%) with behavior policies.
- **Ingress**: AWS ALB Controller for optional direct access (debug only; production traffic via API Gateway).
- **Observability**: ServiceMonitor for Prometheus scraping `/metrics`, PrometheusRule with alerts (API down, high CPU/memory, high error rate, frequent restarts).
- **Resource Policies**: NetworkPolicy (restricts ingress/egress), PodDisruptionBudget (min 1 available), ResourceQuota, LimitRange.
- ConfigMap injects `AWS__Region`, `AWS__SnsTopicArn`, `ASPNETCORE_URLS=http://*:8080`.
- Secrets inject `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` from K8s secrets.

### CI/CD Pipeline
- GitHub Actions workflow in `.github/workflows/deploy.yml`.
- **Triggers**: Push to `main`/`develop`, PR to `main`, manual dispatch.
- **Jobs**:
  1. **Build and Test**: Restore, build, run tests (if present).
  2. **Deploy to EKS**: Build Docker image → push to ECR (`316295889438.dkr.ecr.sa-east-1.amazonaws.com/agrosolutions-ingestion-api`) → apply K8s manifests → wait for rollout.
- **Required Secrets**: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` (see `.github/SECRETS_SETUP.md`).
- **Documentation**: `k8s/CI_CD_SUMMARY.md` for pipeline details, `k8s/README.md` for K8s operations.

## Change discipline for agents
- Prefer minimal, layer-respecting changes: API orchestration in `Api`, business flow in `Application`, provider integrations in `Infrastructure`, contracts/enums in `Shared`.
- When updating payload fields or enums, update validator + publisher serialization assumptions together to avoid runtime schema drift.
- For K8s changes: Test locally with `kubectl apply --dry-run=client`, follow namespace convention (`agrosolutions-ingestion`), maintain resource limits, preserve observability annotations.
- Port consistency: Always use `8080` for container port; update `Dockerfile`, `docker-compose.yaml`, and K8s manifests together if changing ports.