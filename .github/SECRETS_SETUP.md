# GitHub Secrets Setup - Ingestion Service

Este documento detalha como configurar os secrets necessários no GitHub para o pipeline de CI/CD.

## 📍 Localização

`Settings > Secrets and variables > Actions > Repository secrets`

---

## 🔒 Secrets Obrigatórios

### 1. AWS_ACCESS_KEY_ID

**Descrição**: Chave de acesso da conta AWS para deploy no EKS e push no ECR.

**Como obter**:
```bash
aws configure get aws_access_key_id
```

**Permissões necessárias**:
- ECR: `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`
- EKS: `eks:DescribeCluster`, `eks:ListClusters`
- IAM: `iam:GetRole` (para verificar roles)

**Adicionar ao GitHub**:
1. Acesse: `https://github.com/YOUR_USERNAME/agrosolutions-service-ingestion/settings/secrets/actions`
2. Clique em "New repository secret"
3. Name: `AWS_ACCESS_KEY_ID`
4. Secret: Cole o valor da access key
5. Clique em "Add secret"

---

### 2. AWS_SECRET_ACCESS_KEY

**Descrição**: Chave secreta da conta AWS correspondente ao Access Key ID.

**Como obter**:
```bash
aws configure get aws_secret_access_key
```

**Adicionar ao GitHub**:
1. Acesse: `https://github.com/YOUR_USERNAME/agrosolutions-service-ingestion/settings/secrets/actions`
2. Clique em "New repository secret"
3. Name: `AWS_SECRET_ACCESS_KEY`
4. Secret: Cole o valor da secret key
5. Clique em "Add secret"

---

## 🔐 IAM Policy Necessária

Criar uma política IAM com as seguintes permissões:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "ECRPermissions",
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload",
        "ecr:DescribeRepositories",
        "ecr:ListImages"
      ],
      "Resource": "*"
    },
    {
      "Sid": "EKSPermissions",
      "Effect": "Allow",
      "Action": [
        "eks:DescribeCluster",
        "eks:ListClusters",
        "eks:DescribeNodegroup",
        "eks:ListNodegroups"
      ],
      "Resource": "arn:aws:eks:sa-east-1:316295889438:cluster/agrosolutions-eks-cluster"
    },
    {
      "Sid": "IAMPermissions",
      "Effect": "Allow",
      "Action": [
        "iam:GetRole"
      ],
      "Resource": "*"
    }
  ]
}
```

### Criar usuário IAM para CI/CD

```bash
# 1. Criar usuário
aws iam create-user --user-name github-actions-ingestion

# 2. Criar policy
aws iam create-policy \
  --policy-name GitHubActionsIngestionPolicy \
  --policy-document file://iam-policy.json

# 3. Anexar policy ao usuário
aws iam attach-user-policy \
  --user-name github-actions-ingestion \
  --policy-arn arn:aws:iam::316295889438:policy/GitHubActionsIngestionPolicy

# 4. Criar access key
aws iam create-access-key --user-name github-actions-ingestion
```

**IMPORTANTE**: Copie o `AccessKeyId` e `SecretAccessKey` retornados. Esta é a única vez que você verá a secret key!

---

## ✅ Verificar Configuração

### 1. Verificar secrets no GitHub

```bash
# Usando GitHub CLI
gh secret list
```

Deve mostrar:
```
AWS_ACCESS_KEY_ID       Updated 2026-02-19
AWS_SECRET_ACCESS_KEY   Updated 2026-02-19
```

### 2. Testar permissões AWS

```bash
# Configurar credenciais localmente (temporário)
export AWS_ACCESS_KEY_ID="valor-do-secret"
export AWS_SECRET_ACCESS_KEY="valor-do-secret"

# Testar ECR
aws ecr describe-repositories --region sa-east-1

# Testar EKS
aws eks describe-cluster --name agrosolutions-eks-cluster --region sa-east-1

# Limpar variáveis
unset AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY
```

### 3. Testar pipeline

Após configurar os secrets, faça um push para `main` e verifique se:
1. Job "Build and Test" passa
2. Job "Deploy to EKS" passa
3. Pods são criados no EKS
4. Deployment fica healthy

---

## 🔄 Rotação de Secrets

### Quando rotacionar:

- ✅ A cada 90 dias (boa prática)
- ✅ Quando houver suspeita de comprometimento
- ✅ Quando um colaborador sair da equipe

### Como rotacionar:

```bash
# 1. Criar nova access key para o usuário
aws iam create-access-key --user-name github-actions-ingestion

# 2. Atualizar secrets no GitHub (via UI ou CLI)
gh secret set AWS_ACCESS_KEY_ID
gh secret set AWS_SECRET_ACCESS_KEY

# 3. Testar que pipeline ainda funciona

# 4. Deletar access key antiga
aws iam delete-access-key \
  --user-name github-actions-ingestion \
  --access-key-id OLD_KEY_ID
```

---

## 🚨 Troubleshooting

### Secret não está sendo lido

**Sintoma**: Pipeline falha com `The secret AWS_ACCESS_KEY_ID was not found`

**Solução**:
1. Verificar que secret está em `Repository secrets` (não `Environment secrets`)
2. Verificar nome exato (case-sensitive)
3. Re-criar o secret

### Permissão negada ao acessar ECR/EKS

**Sintoma**: `AccessDeniedException` ou `User: ... is not authorized to perform: ecr:GetAuthorizationToken`

**Solução**:
1. Verificar IAM policy anexada ao usuário
2. Verificar se as permissões estão corretas
3. Aguardar 1-2 minutos para propagação da policy

### Access key inválida

**Sintoma**: `InvalidClientTokenId` ou `SignatureDoesNotMatch`

**Solução**:
1. Verificar se copiou corretamente o Access Key e Secret Key
2. Verificar se não há espaços ou quebras de linha nos secrets
3. Criar nova access key se necessário

---

## 📚 Referências

- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [AWS IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)
- [AWS ECR Authentication](https://docs.aws.amazon.com/AmazonECR/latest/userguide/registry_auth.html)
