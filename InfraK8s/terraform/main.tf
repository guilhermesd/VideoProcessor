provider "aws" {
  region = "us-east-1"
}

# Busca a VPC default da região us-east-1
data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "da_vpc" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }

  filter {
    name   = "availability-zone"
    values = [
      "us-east-1a",
      "us-east-1b",
      "us-east-1c",
      "us-east-1d",
      "us-east-1f"      
    ]
  }  
}

# Cria o cluster EKS
resource "aws_eks_cluster" "pedidos" {
  name     = "pedidos-cluster"
  role_arn = var.node_role_arn

  vpc_config {
    subnet_ids = data.aws_subnets.da_vpc.ids
  }

  lifecycle {
    prevent_destroy = true # Evita que o Terraform destrua um cluster em produção por acidente
  }

  tags = {
    Name = "pedidos-cluster"
  }    
}

# Cria o node group
resource "aws_eks_node_group" "pedidos_nodes" {
  cluster_name    = aws_eks_cluster.pedidos.name
  node_group_name = "pedidos-nodes"
  node_role_arn   = var.node_role_arn
  subnet_ids      = data.aws_subnets.da_vpc.ids

  scaling_config {
    desired_size = 3
    max_size     = 5
    min_size     = 3
  }

  instance_types = ["t3.small"] # Pode testar "t3.small" se quiser forçar menos RAM
  disk_size      = 20
}

data "aws_eks_cluster" "cluster" {
  name = aws_eks_cluster.pedidos.name
}

data "aws_eks_cluster_auth" "cluster" {
  name = aws_eks_cluster.pedidos.name
}

provider "kubernetes" {
  host                   = data.aws_eks_cluster.cluster.endpoint
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.cluster.certificate_authority[0].data)
  token                  = data.aws_eks_cluster_auth.cluster.token
}

resource "kubernetes_secret" "app_secrets" {

  metadata {
    name = "app-secrets"
  }

  data = {
    DefaultConnection = "Server=mysql-pedidos.cwwqhgcfbsrd.us-east-1.rds.amazonaws.com;Port=3306;Database=pedidos_db;User=admin;Password=Teste#Teste;Connection Timeout=30;"
  }

  type = "Opaque"
}

resource "null_resource" "apply_k8s_yamls" {
  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {

    interpreter = ["bash", "-c"]
    command = <<EOT
      echo "📡 Atualizando kubeconfig..."
      aws eks update-kubeconfig --region us-east-1 --name pedidos-cluster

      echo "🔁 Garantindo que o contexto correto está em uso..."
      kubectl config use-context arn:aws:eks:us-east-1:100527548163
      :cluster/pedidos-cluster

      echo "🔁 Deletando Job antigo (se existir)..."
      kubectl delete job db-migration-job --ignore-not-found

      echo "🚀 Aplicando arquivos YAML..."
      kubectl apply -f ../K8s/
    EOT
  }

  depends_on = [
    aws_eks_cluster.pedidos,
    aws_eks_node_group.pedidos_nodes,
    kubernetes_secret.app_secrets
  ]
}