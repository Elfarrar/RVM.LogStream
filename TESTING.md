# Testes — RVM.LogStream

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.LogStream.Test/`
- **Total:** 83 testes
- **Foco:** LogIngestionService (validacao), LogSearchService (filtros, paginacao), RetentionWorker (expiracao por politica), logica de busca full-text

```bash
dotnet test test/RVM.LogStream.Test/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** dashboard de logs, busca com filtros, stream ao vivo (SignalR), configuracao de retencao

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
LOGSTREAM_BASE_URL=http://localhost:5000
LOGSTREAM_API_KEY=<api-key-dev>
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- `RetentionWorker` desativado em testes via configuracao de ambiente
