# Etapa base: imagem leve do ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# ✅ Instala o ffmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Etapa build: compila o projeto Worker
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos da solução
COPY . .

# Define o diretório do projeto Worker
WORKDIR /src/Worker

# Publica o projeto
RUN dotnet publish -c Release -o /app/publish

# Etapa final: roda o Worker
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# 🔥 Entrypoint do Worker
ENTRYPOINT ["dotnet", "Worker.dll"]
