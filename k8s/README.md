# Kubernetes Manifests - Ingestion Service

Arquivos de configuração Kubernetes para deploy do AgroSolutions Ingestion Service no AWS EKS.

## 📁 Estrutura

```
k8s/
├── CI_CD_SUMMARY.md          # Documentação completa do pipeline
├── README.md                 # Este arquivo
└── production/               # Manifestos de produção
    ├── namespace.yaml        # Namespace agrosolutions-ingestion
    ├── configmaps.yaml       # ConfigMaps (AWS, app settings)
    ├── deployment.yaml       # Deployment com 2 replicas, health checks
    ├── services.yaml         # Service ClusterIP
    ├── hpa.yaml              # HorizontalPodAutoscaler (2-10 pods)
    ├── ingress-aws.yaml      # AWS Load Balancer Ingress
    ├── observability.yaml    # ServiceMonitor e PrometheusRules
    └── resource-configs.yaml # NetworkPolicy, PDB, Quotas
```

## 🚀 Deploy Automático (CI/CD)

O deploy é automático via GitHub Actions quando há push na branch `main`.

Ver: [CI_CD_SUMMARY.md](./CI_CD_SUMMARY.md)

## 🔧 Deploy Manual

### Pré-requisitos

1. **AWS CLI** configurado com credenciais
2. **kubectl** instalado
3. **Cluster EKS** provisionado
4. **ECR Repository** criado

### Configurar kubeconfig

```bash
aws eks update-kubeconfig --name agrosolutions-eks-cluster --region sa-east-1
```

### Criar secrets

```bash
# AWS credentials
kubectl create secret generic ingestion-aws-secrets \
  --from-literal=AWS_ACCESS_KEY_ID="YOUR_KEY" \
  --from-literal=AWS_SECRET_ACCESS_KEY="YOUR_SECRET" \
  -n agrosolutions-ingestion
```

### Deploy na ordem correta

```bash
# 1. Namespace
kubectl apply -f production/namespace.yaml

# 2. Secrets (manual - ver acima)

# 3. ConfigMaps
kubectl apply -f production/configmaps.yaml

# 4. Deployment e Service
kubectl apply -f production/deployment.yaml
kubectl apply -f production/services.yaml

# 5. HPA e Resource Configs
kubectl apply -f production/hpa.yaml
kubectl apply -f production/resource-configs.yaml

# 6. Observability (requer Prometheus Operator)
kubectl apply -f production/observability.yaml

# 7. Ingress (opcional - requer AWS Load Balancer Controller)
kubectl apply -f production/ingress-aws.yaml
```

### Ou aplicar tudo de uma vez

```bash
kubectl apply -f production/
```

## 🔍 Verificação

### Status do deployment

```bash
kubectl get deployments -n agrosolutions-ingestion
kubectl get pods -n agrosolutions-ingestion
kubectl get svc -n agrosolutions-ingestion
kubectl get hpa -n agrosolutions-ingestion
```

### Logs

```bash
# Logs do pod mais recente
kubectl logs -n agrosolutions-ingestion -l app=ingestion-api --tail=100 -f

# Logs de um pod específico
kubectl logs -n agrosolutions-ingestion ingestion-api-xxxxx-xxxxx -f
```

### Health check

```bash
# Port-forward para acessar localmente
kubectl port-forward -n agrosolutions-ingestion svc/ingestion-api-service 8080:80

# Em outro terminal
curl http://localhost:8080/health
```

## 📊 Monitoramento

### Métricas Prometheus

As métricas são exportadas em `/metrics` e coletadas automaticamente pelo Prometheus via ServiceMonitor.

### Alertas

Alertas configurados em `observability.yaml`:
- **IngestionApiDown**: API inacessível por 2+ minutos
- **IngestionApiHighCPU**: CPU > 80% por 5 minutos
- **IngestionApiHighMemory**: Memória > 90% por 5 minutos
- **IngestionApiHighErrorRate**: Taxa de erro 5xx > 5%
- **IngestionApiHighLatency**: P95 > 1s
- **IngestionApiFrequentRestarts**: Restarts frequentes

## 🔐 Secrets Management

**NUNCA** commitar secrets no Git. Usar um dos métodos:

### 1. GitHub Secrets (CI/CD)

Configurar em: `Settings > Secrets and variables > Actions`

Ver: [../.github/SECRETS_SETUP.md](../.github/SECRETS_SETUP.md)

### 2. AWS Secrets Manager (runtime)

```bash
# Criar secret no AWS Secrets Manager
aws secretsmanager create-secret \
  --name agrosolutions/ingestion/aws-credentials \
  --secret-string '{"AWS_ACCESS_KEY_ID":"xxx","AWS_SECRET_ACCESS_KEY":"yyy"}' \
  --region sa-east-1
```

Usar External Secrets Operator para sincronizar com K8s.

### 3. Criação manual (dev/staging)

```bash
kubectl create secret generic ingestion-aws-secrets \
  --from-literal=AWS_ACCESS_KEY_ID="$(aws configure get aws_access_key_id)" \
  --from-literal=AWS_SECRET_ACCESS_KEY="$(aws configure get aws_secret_access_key)" \
  -n agrosolutions-ingestion
```

## 🔄 Rollback

```bash
# Ver histórico de deployments
kubectl rollout history deployment/ingestion-api -n agrosolutions-ingestion

# Rollback para versão anterior
kubectl rollout undo deployment/ingestion-api -n agrosolutions-ingestion

# Rollback para revisão específica
kubectl rollout undo deployment/ingestion-api --to-revision=2 -n agrosolutions-ingestion
```

## 🗑️ Cleanup

```bash
# Deletar tudo
kubectl delete namespace agrosolutions-ingestion
```

## 📝 Notas

- **Namespace**: `agrosolutions-ingestion` (diferente do anterior `agrosolutions` para isolamento)
- **Replicas**: 2 mínimo, 10 máximo (autoscaling)
- **Port**: 8080 interno (container), 80 externo (service)
- **Health**: `/health` endpoint
- **Resources**: 256Mi-512Mi RAM, 200m-1000m CPU
