*[English](README.en.md) | **Portugues***

# RVM.LogStream

Plataforma centralizada de ingestao e busca de logs com retencao configuravel, dashboard SignalR e analytics.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-44%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## Sobre

RVM.LogStream e uma plataforma centralizada de logs que permite ingestao em batch, busca avancada por multiplos filtros (source, level, query, correlationId, date range), politicas de retencao configuraveis por source e analytics de volume por level/source. O sistema utiliza SignalR para push em tempo real de novos logs e oferece um pipeline completo de observabilidade.

---

## Tecnologias

| Camada | Stack |
|---|---|
| Runtime | .NET 10, ASP.NET Core 10 |
| Real-time | SignalR |
| ORM | Entity Framework Core 10 |
| Banco de Dados | PostgreSQL (Npgsql 10.0.1) |
| Logging | Serilog + Compact JSON |
| Autenticacao | API Key (header `X-API-Key`) |
| Testes | xUnit 2.9, Moq 4.20, EF Core InMemory |
| Containerizacao | Docker, Docker Compose |

---

## Arquitetura

```
┌───────────────────────────────────────────────────────┐
│                     API Layer                         │
│  ┌─────────────┐ ┌──────────┐ ┌───────────────────┐  │
│  │  Ingestion  │ │  Search  │ │  Retention/Stats  │  │
│  │  Controller │ │Controller│ │   Controllers     │  │
│  └──────┬──────┘ └────┬─────┘ └────────┬──────────┘  │
│         │             │                │              │
│  ┌──────▼──────┐ ┌────▼─────┐  ┌───────▼──────────┐  │
│  │  Ingestion  │ │  Search  │  │  RetentionWorker  │  │
│  │  Service    │ │  Service │  │ (BackgroundService)│  │
│  └──────┬──────┘ └────┬─────┘  └───────┬──────────┘  │
│         │             │                │              │
│  ┌──────▼─────────────▼────────────────▼──────────┐  │
│  │              SignalR Hub (Push)                 │  │
│  └────────────────────┬───────────────────────────┘  │
├───────────────────────┼───────────────────────────────┤
│                 Domain Layer                          │
│  ┌────────────┐ ┌───────────┐ ┌───────────────────┐  │
│  │  LogEntry  │ │ LogSource │ │ RetentionPolicy   │  │
│  └────────────┘ └───────────┘ └───────────────────┘  │
│  ┌────────────────────────────────────────────────┐  │
│  │           Repository Interfaces                │  │
│  └────────────────────┬───────────────────────────┘  │
├───────────────────────┼───────────────────────────────┤
│             Infrastructure Layer                      │
│  ┌────────────────────▼───────────────────────────┐  │
│  │         LogStreamDbContext (EF Core)            │  │
│  │         PostgreSQL + Npgsql                     │  │
│  └────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────┐  │
│  │         Repository Implementations             │  │
│  └────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────┘
```

---

## Estrutura do Projeto

```
RVM.LogStream/
├── src/
│   ├── RVM.LogStream.API/
│   │   ├── Auth/
│   │   │   ├── ApiKeyAuthHandler.cs
│   │   │   └── ApiKeyAuthOptions.cs
│   │   ├── Controllers/
│   │   │   ├── IngestionController.cs
│   │   │   ├── RetentionController.cs
│   │   │   ├── SearchController.cs
│   │   │   ├── SourcesController.cs
│   │   │   └── StatsController.cs
│   │   ├── Dtos/
│   │   │   ├── IngestionDtos.cs
│   │   │   ├── RetentionDtos.cs
│   │   │   ├── SearchDtos.cs
│   │   │   ├── SourceDtos.cs
│   │   │   └── StatsDtos.cs
│   │   ├── Health/
│   │   │   └── DatabaseHealthCheck.cs
│   │   ├── Hubs/
│   │   │   └── LogStreamHub.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Services/
│   │   │   ├── LogIngestionService.cs
│   │   │   ├── LogSearchService.cs
│   │   │   └── RetentionWorker.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── RVM.LogStream.Domain/
│   │   ├── Entities/
│   │   │   ├── LogEntry.cs
│   │   │   ├── LogSource.cs
│   │   │   └── RetentionPolicy.cs
│   │   ├── Enums/
│   │   │   └── LogLevel.cs
│   │   ├── Interfaces/
│   │   │   ├── ILogEntryRepository.cs
│   │   │   ├── ILogSourceRepository.cs
│   │   │   └── IRetentionPolicyRepository.cs
│   │   └── Models/
│   │       └── AggregationModels.cs
│   └── RVM.LogStream.Infrastructure/
│       ├── Data/
│       │   ├── Configurations/
│       │   │   ├── LogEntryConfiguration.cs
│       │   │   ├── LogSourceConfiguration.cs
│       │   │   └── RetentionPolicyConfiguration.cs
│       │   └── LogStreamDbContext.cs
│       ├── Repositories/
│       │   ├── LogEntryRepository.cs
│       │   ├── LogSourceRepository.cs
│       │   └── RetentionPolicyRepository.cs
│       └── DependencyInjection.cs
├── test/
│   └── RVM.LogStream.Test/
│       ├── Domain/
│       │   └── EntityTests.cs
│       ├── Infrastructure/
│       │   ├── LogEntryRepositoryTests.cs
│       │   ├── LogSourceRepositoryTests.cs
│       │   └── RetentionPolicyRepositoryTests.cs
│       └── Services/
│           └── LogIngestionServiceTests.cs
├── docker-compose.dev.yml
├── docker-compose.prod.yml
└── RVM.LogStream.slnx
```

---

## Como Executar

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) (ou Docker)

### Configuracao

1. Clone o repositorio:
```bash
git clone https://github.com/rvenegas5/RVM.LogStream.git
cd RVM.LogStream
```

2. Configure a connection string e API keys via variaveis de ambiente ou `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rvmlogstream;Username=postgres;Password=sua_senha"
  },
  "ApiKeys": {
    "Keys": [
      { "Key": "sua-api-key", "AppId": "app-1", "Name": "MeuApp" }
    ]
  },
  "Retention": {
    "CheckIntervalMinutes": 60
  }
}
```

3. Execute a aplicacao:
```bash
cd src/RVM.LogStream.API
dotnet run
```

### Docker Compose

```bash
docker compose -f docker-compose.dev.yml up -d
```

A API ficara disponivel em `http://localhost:8080`.

---

## Endpoints da API

Todos os endpoints requerem autenticacao via header `X-API-Key`.

### Ingestao

| Metodo | Rota | Descricao |
|---|---|---|
| `POST` | `/api/ingestion` | Ingestao de logs em batch |

**Request body:**
```json
{
  "events": [
    {
      "timestamp": "2026-04-09T12:00:00Z",
      "level": "Error",
      "message": "Falha na conexao",
      "messageTemplate": null,
      "source": "payments-api",
      "correlationId": "abc-123",
      "properties": "{\"key\":\"value\"}",
      "exception": "NullReferenceException"
    }
  ]
}
```

**Response:** `{ "accepted": 1, "rejected": 0 }`

### Busca

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/api/search` | Busca com filtros avancados |

**Query parameters:** `query`, `source`, `level`, `correlationId`, `from`, `to`, `offset`, `limit`

### Sources

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/api/sources` | Listar todas as sources |
| `GET` | `/api/sources/{name}` | Buscar source por nome |

### Retencao

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/api/retention` | Listar politicas de retencao |
| `GET` | `/api/retention/{id}` | Buscar politica por ID |
| `POST` | `/api/retention` | Criar politica de retencao |
| `PUT` | `/api/retention/{id}` | Atualizar politica |
| `DELETE` | `/api/retention/{id}` | Remover politica |

### Estatisticas

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/api/stats` | Volume por level e por source |

**Query parameters:** `source`, `from`, `to`

### Real-time

| Protocolo | Rota | Descricao |
|---|---|---|
| SignalR | `/hubs/log-stream` | Push em tempo real de novos logs |

**Eventos:** `LogReceived` | **Grupos:** `JoinSourceGroup(source)`, `LeaveSourceGroup(source)`

### Health Check

| Metodo | Rota | Descricao |
|---|---|---|
| `GET` | `/health` | Verificacao de saude (publico) |

---

## Testes

O projeto possui **44 testes** cobrindo domain, infrastructure e services.

```bash
dotnet test
```

| Suite | Arquivo | Testes |
|---|---|---|
| Domain | `EntityTests.cs` | 12 (6 Fact + 1 Theory x6 InlineData) |
| Infrastructure | `LogEntryRepositoryTests.cs` | 12 |
| Infrastructure | `LogSourceRepositoryTests.cs` | 5 |
| Infrastructure | `RetentionPolicyRepositoryTests.cs` | 8 |
| Services | `LogIngestionServiceTests.cs` | 7 |
| **Total** | | **44** |

### Stack de testes
- **xUnit 2.9** - Framework de testes
- **Moq 4.20** - Mocking para services
- **EF Core InMemory** - Banco em memoria para testes de repositorio

---

## Funcionalidades

- **Ingestao em batch** - Envio de multiplos log entries em uma unica requisicao
- **Busca avancada** - Filtros por source, level, query, correlationId e date range
- **Politicas de retencao** - Configuracao de retencao por source com pattern matching
- **RetentionWorker** - BackgroundService que aplica politicas de retencao automaticamente
- **Analytics de volume** - Agregacao de logs por level e por source
- **SignalR push** - Notificacao em tempo real de novos logs via WebSocket
- **6 niveis de log** - Trace, Debug, Information, Warning, Error, Fatal
- **Paginacao** - Offset/limit com clamp (max 200 por pagina)
- **Correlation ID** - Middleware que propaga ou gera X-Correlation-ID
- **Rate limiting** - 60 requisicoes/minuto por IP
- **API Key auth** - Autenticacao por chave com identificacao de app
- **Health check** - Endpoint `/health` com verificacao de conectividade do banco
- **Docker ready** - Docker Compose para dev e prod com rede `rvmtech`

---

<p align="center">
  Desenvolvido por <strong>RVM Tech</strong>
</p>
