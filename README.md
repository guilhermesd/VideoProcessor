# 🎥 Video Processor - Sistema de Processamento de Vídeos

Este projeto é um sistema completo para upload, processamento e download de vídeos. Ele permite que usuários façam upload de vídeos, os quais são processados por um worker (extração de frames e compactação em ZIP), e o resultado pode ser baixado posteriormente.

## 🔧 Funcionalidades

- Registro e login de usuários com autenticação JWT.
- Upload de arquivos de vídeo.
- Processamento assíncrono usando RabbitMQ e Worker (extração de frames via FFmpeg).
- Download do vídeo processado (ZIP com frames).
- Cache da listagem de vídeos por usuário com Redis.
- Atualização automática da listagem na interface (frontend).
- Integração com Docker Compose e Kubernetes.
- Testes automatizados com xUnit.
- CI/CD com GitHub Actions.

## 🏛️ Arquitetura Proposta

A arquitetura foi pensada em **camadas**, visando **escalabilidade**, **manutenibilidade** e **testabilidade**. Os principais projetos e responsabilidades são:

### 🗂 Estrutura do Projeto

```
VideoProcessorSolution/
├── Api/                        # Projeto ASP.NET Core (API REST)
│   ├── Controllers/            # Endpoints de Upload, Download, Login e Cadastro
│   ├── Middlewares/           # Tratamento global de exceções
│   └── Program.cs             # Configuração de serviços, autenticação, Swagger, etc.
│
├── Application/               # Camada de aplicação
│   └── UseCases/              # Casos de uso e orquestração de lógica
│
├── Domain/                    # Camada de domínio
│   ├── Entities/              # Entidades de negócio (User, VideoJob)
│   ├── Enums/                 # Enumerações (Status do vídeo, etc.)
│   └── Interfaces/            # Contratos de repositórios e serviços externos
│
├── Infrastructure/            # Implementações de infraestrutura
│   ├── Repositories/          # Implementações de repositórios (MongoDB, Redis)
│   ├── MessageBus/            # RabbitMQ: Producer e Consumer
│   └── ServicosExternos/      # Serviço de notificação (ex: e-mail fake)
│
├── Worker/                    # Projeto Worker para processar vídeos
│   └── WorkerService.cs       # Consome fila, processa vídeos e gera arquivos ZIP
│
├── Frontend/                  # Interface web (HTML, JavaScript e Bootstrap)
│   └── index.html             # Tela única com login, cadastro, upload e listagem
│
├── Tests/                     # Testes de unidade com xUnit e Moq
│   ├── AuthControllerTests.cs
│   └── VideoControllerTests.cs
│
├── docker-compose.yml         # Orquestração local com Docker (API, Worker, Redis, Mongo, RabbitMQ, NGINX, Grafana)
├── nginx.conf                 # Configuração do NGINX como proxy reverso
└── .github/
    └── workflows/             # CI/CD com GitHub Actions
        └── build.yml          # Pipeline de build, testes e push para ECR
```

## 🗃️ Persistência de Dados

- **MongoDB**: Armazena as entidades \`User\` e \`VideoJob\`.
- **Redis**: Armazena cache da listagem de vídeos por usuário.
- **Volumes Docker**: Diretório compartilhado entre API e Worker para os arquivos de upload e saída (ZIPs).

## ⚙️ Tecnologias Utilizadas

| Camada/Serviço | Tecnologia |
|----------------|------------|
| Backend        | ASP.NET Core 7.0, C# |
| Frontend       | HTML, JavaScript, Bootstrap |
| Banco de dados | MongoDB |
| Fila de Mensagens | RabbitMQ |
| Cache          | Redis |
| Processamento  | FFmpeg (via \`ProcessStartInfo\`) |
| Containerização | Docker e Docker Compose |
| Orquestração   | Kubernetes (YAMLs já preparados) |
| CI/CD          | GitHub Actions |
| Testes         | xUnit, Moq |

## 📁 Scripts e Recursos

### MongoDB – Criação de Banco e Coleções

O MongoDB é criado automaticamente pelo container no Docker Compose.

Caso queira testar localmente:

\`\`\`bash
docker exec -it mongo-video-service mongosh
use Pagamentos
db.VideoJobs.createIndex({ "UserId": 1 })
db.Users.createIndex({ "Email": 1 }, { unique: true })
\`\`\`

### Docker Compose

\`\`\`bash
docker-compose up --build
\`\`\`

## 🔐 Autenticação

- Login e Registro via \`/auth/login\` e \`/auth/register\`
- Token JWT gerado e salvo no \`localStorage\` do frontend
- Proteção dos endpoints com \`[Authorize]\`

## 📦 CI/CD

### GitHub Actions

- **Build da API e Worker**
- **Execução dos Testes**
- **Push automático da imagem para o ECR (ou DockerHub)**
- **Deploy em Kubernetes (opcional via webhook)**

Arquivo: \`.github/workflows/main.yml\`

## 🧪 Testes Automatizados

Localizados em \`Tests/\` com cobertura de:

- Autenticação
- Upload de Vídeos
- Listagem de vídeos
- Worker com mocks

Rodar com:

\`\`\`bash
dotnet test
\`\`\`

## 🔗 Acessos Locais

| Serviço     | URL                          |
|-------------|------------------------------|
| Frontend    | http://localhost             |
| API         | http://localhost/api         |
| RabbitMQ UI | http://localhost:15672       |
| Grafana     | http://localhost:3000        |
| Mongo Express (opcional) | http://localhost:8081 |

## 🚀 Escalabilidade

- Cada componente roda de forma **isolada em containers**, podendo ser **replicado** no Kubernetes.
- A fila RabbitMQ garante **desacoplamento** entre upload e processamento.
- Redis permite caching eficiente em múltiplas instâncias da API.
- Uploads e Zips são salvos em **volume compartilhado** (ou PVC no Kubernetes).

## 🤝 Contribuindo

1. Fork o repositório
2. Crie sua branch (\`git checkout -b feature/sua-feature\`)
3. Commit suas mudanças (\`git commit -m 'feat: nova feature'\`)
4. Push para a branch (\`git push origin feature/sua-feature\`)
5. Crie um Pull Request
