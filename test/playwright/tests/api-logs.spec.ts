import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.LOGSTREAM_BASE_URL ?? 'https://logstream.lab.rvmtech.com.br';

test.describe('LogStream API', () => {
  test.skip(
    process.env.LOGSTREAM_RUN_SMOKE !== '1',
    'Defina LOGSTREAM_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/sources retorna lista de fontes ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/sources`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/search retorna resultados ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/search`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/stats retorna estatisticas de logs ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/stats`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/retention retorna politicas de retencao ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/retention`);
    expect([200, 401]).toContain(response.status());
  });

  test('POST /api/ingestion sem corpo retorna 400 ou 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/ingestion`);
    expect([400, 401]).toContain(response.status());
  });
});
