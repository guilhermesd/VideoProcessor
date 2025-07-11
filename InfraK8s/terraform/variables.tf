variable "node_role_arn" {
  description = "ARN da IAM Role do EKS Node"
  type        = string
  default = "arn:aws:iam::100527548163:role/LabRole"
}