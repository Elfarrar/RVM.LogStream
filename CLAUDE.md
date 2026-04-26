# RVM.LogStream

## Visao Geral
Plataforma de logs centralizados com ingestao via API, busca full-text, politica de retencao automatica e streaming ao vivo via SignalR. Aplicacoes enviam logs via HTTP; operadores visualizam e filtram no dashboard Blazor Server em tempo real.

Projeto portfolio demonstrando ingestao de telemetria, busca estruturada, retencao com TTL e streaming de eventos com SignalR.

## Stack
- .NET 10, ASP.NET Core, Blazor Server
- SignalR (`LogStreamHub` em `/hubs/log-stream`)
- Entity Framework Core + PostgreSQL (logs, indices de busca)
- Autenticacao via API Key
- Rate limiting: 60 req/min global
- Serilog + Seq, RVM.Common.Security
- xUnit 83 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.LogStream.API/
    Auth/                     # ApiKeyAuthHandler
    Components/               # Blazor pages (logs, busca, filtros, retencao)
    Controllers/              # REST: ingestao de logs, busca, configuracao retencao
    Health/                   # DatabaseHealthCheck
    Hubs/                     # LogStreamHub (SignalR)
    Middleware/               # CorrelationIdMiddleware
    Services/
      LogIngestionService     # Valida e persiste entradas de log
      LogSearchService        # Busca full-text com filtros
      RetentionWorker         # BackgroundService: apaga logs expirados
  RVM.LogStream.Domain/       # Entidades (LogEntry, RetentionPolicy, SearchFilter)
  RVM.LogStream.Infrastructure/
    Data/                     # LogStreamDbContext
    Repositories/             # ILogRepository
test/
  RVM.LogStream.Test/         # xUnit (83 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- `RetentionWorker` e BackgroundService — roda periodicamente, independente de requests
- `LogStreamHub` anonimo (`AllowAnonymous`) — stream publico por design
- `LogIngestionService` e `LogSearchService` sao scoped — um contexto de DB por request
- Log levels: Trace, Debug, Information, Warning, Error, Critical (enum no Domain)
- Retencao configuravel por nivel de log (ex: Debug 7 dias, Error 90 dias)
- `EnsureCreated` em dev, migration EF Core em producao

## Como Rodar
### Dev
```bash
docker compose -f docker-compose.dev.yml up -d
cd src/RVM.LogStream.API
dotnet run
```

### Testes
```bash
dotnet test test/RVM.LogStream.Test/
```

## Decisoes Arquiteturais
- **LogSearchService separado de LogIngestionService**: ingestao e write-heavy; busca e read-heavy com filtros complexos — separar permite otimizar cada um independentemente
- **RetentionWorker como BackgroundService**: retencao nao precisa ser síncrona com requests — worker roda de madrugada sem impactar performance de ingestao
- **SignalR para stream ao vivo**: polling de logs em UI e caro; push via hub e eficiente e escalavel
- **PostgreSQL full-text search**: evita dependencia de Elasticsearch no portfolio; demonstra capacidade nativa do Postgres
