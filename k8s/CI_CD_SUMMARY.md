# 🚀 CI/CD Pipeline - AgroSolutions Ingestion Service

## 📋 Visão Geral

O pipeline de CI/CD é executado automaticamente via **GitHub Actions** definido em `.github/workflows/deploy.yml`.

---

## 🔄 Trigger Automático

O workflow é disparado quando:
- ✅ Push na branch `main` ou `develop`
- ✅ Pull Request para `main`
- ✅ Execução manual via `workflow_dispatch`

---

## 📦 Jobs do Pipeline

### 1️⃣ Build and Test

**Execução**: Sempre (em todos os eventos)

**Passos**:
1. Checkout do código
2. Setup .NET SDK 10.0
3. Restore de dependências
4. Build da solution
5. Execução de testes (quando disponíveis)

**Resultado**: Valida que o código compila.

---

### 2️⃣ Deploy to EKS

**Execução**: Apenas quando push em `main`

**Dependência**: `build-and-test` deve passar

**Passos**:
1. **Build Docker Image**
   - Build multi-stage com .NET 10 Alpine
   - Tag com `$(github.sha)` e `latest`
   
2. **Push para ECR**
   - Login no ECR: `316295889438.dkr.ecr.sa-east-1.amazonaws.com`
   - Push da imagem: `agrosolutions-ingestion-api:latest`
   
3. **Deploy no EKS**
   - Configura `kubectl` com credenciais AWS
   - Aplica manifests de `k8s/production/`
   - Aguarda rollout completar
   - Verifica health do deployment

**Variáveis de ambiente**:
```yaml
AWS_REGION: sa-east-1
ECR_REPOSITORY: agrosolutions-ingestion-api
EKS_CLUSTER_NAME: agrosolutions-eks-cluster
```

---

## 🔐 Secrets Necessários

Configure em: `Settings > Secrets and variables > Actions > Repository secrets`

### Obrigatórios:

| Secret | Descrição | Como obter |
|--------|-----------|------------|
| `AWS_ACCESS_KEY_ID` | AWS Access Key para ECR/EKS | `aws configure get aws_access_key_id` |
| `AWS_SECRET_ACCESS_KEY` | AWS Secret Key | `aws configure get aws_secret_access_key` |

### Permissões IAM necessárias:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "eks:DescribeCluster",
        "eks:ListClusters"
      ],
      "Resource": "arn:aws:eks:sa-east-1:316295889438:cluster/agrosolutions-eks-cluster"
    }
  ]
}
```

Ver documentação completa: [../.github/SECRETS_SETUP.md](../.github/SECRETS_SETUP.md)

---

## 📊 Monitoramento do Pipeline

### GitHub Actions UI

1. Acesse: `https://github.com/YOUR_ORG/agrosolutions-service-ingestion/actions`
2. Visualize runs do workflow
3. Verifique logs de cada step
4. Analise artefatos gerados (se houver)

### Notificações

- ✅ **Sucesso**: Badge verde no README
- ❌ **Falha**: Email para autor do commit
- 🟡 **Em progresso**: Status no PR

---

## 🔧 Executar Manualmente

### Via GitHub UI

1. Acesse: `Actions > Deploy to EKS`
2. Clique em `Run workflow`
3. Selecione branch (ex: `main`)
4. Clique em `Run workflow`

### Via GitHub CLI

```bash
gh workflow run deploy.yml --ref main
```

---

## 🐛 Troubleshooting

### Build falha

**Sintoma**: Step "Build" retorna erro de compilação

**Solução**:
```bash
# Local: testar build antes de commit
dotnet build AgrosolutionsServiceIngestion.slnx
```

### Push para ECR falha

**Sintoma**: `AccessDeniedException` ao fazer push

**Causa**: Credenciais AWS inválidas ou sem permissão

**Solução**:
1. Verificar secrets `AWS_ACCESS_KEY_ID` e `AWS_SECRET_ACCESS_KEY`
2. Validar permissões IAM (ver seção acima)
3. Verificar se repositório ECR existe:
   ```bash
   aws ecr describe-repositories --repository-names agrosolutions-ingestion-api --region sa-east-1
   ```

### Deploy falha

**Sintoma**: `kubectl apply` retorna erro

**Possíveis causas**:
1. **Cluster inacessível**: Validar credenciais EKS
2. **Namespace não existe**: `kubectl create namespace agrosolutions-ingestion`
3. **Secrets ausentes**: Criar secrets manualmente (ver README.md)
4. **Manifests inválidos**: Validar YAML localmente

**Solução**:
```bash
# Validar manifests localmente
kubectl apply -f k8s/production/ --dry-run=client

# Verificar connectivity com cluster
aws eks update-kubeconfig --name agrosolutions-eks-cluster --region sa-east-1
kubectl cluster-info
```

### Rollout timeout

**Sintoma**: Deployment não fica "healthy" após 5 minutos

**Causa**: Pods não passam readiness probe

**Debug**:
```bash
# Ver status dos pods
kubectl get pods -n agrosolutions-ingestion

# Ver logs do pod
kubectl logs -n agrosolutions-ingestion ingestion-api-xxxxx-xxxxx

# Descrever pod para ver eventos
kubectl describe pod -n agrosolutions-ingestion ingestion-api-xxxxx-xxxxx
```

**Soluções comuns**:
- Verificar se ConfigMap tem configurações corretas (SNS ARN, região)
- Verificar se Secrets existem
- Verificar se imagem foi pushed corretamente para ECR
- Verificar resource limits (memória insuficiente?)

---

## 📚 Referências

- [Kubernetes Manifests](./README.md)
- [GitHub Secrets Setup](../.github/SECRETS_SETUP.md)
- [AWS ECR Documentation](https://docs.aws.amazon.com/ecr/)
- [AWS EKS Documentation](https://docs.aws.amazon.com/eks/)
