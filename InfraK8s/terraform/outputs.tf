output "cluster_name" {
  value = aws_eks_cluster.pedidos.name
}

output "cluster_endpoint" {
  value = aws_eks_cluster.pedidos.endpoint
}

output "cluster_certificate" {
  value = aws_eks_cluster.pedidos.certificate_authority[0].data
}