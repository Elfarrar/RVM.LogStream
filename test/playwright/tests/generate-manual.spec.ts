/**
 * RVM.LogStream — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema de logs centralizados,
 * captura screenshots em desktop e mobile, e gera as imagens para o manual.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';

const BASE_URL = process.env.LOGSTREAM_BASE_URL ?? 'https://logstream.lab.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`), fullPage });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`), fullPage });
  await page.setViewportSize({ width: 1280, height: 800 });
}

test.describe('RVM.LogStream — Manual Visual', () => {
  test('01 Dashboard', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-dashboard');
  });

  test('02 Busca de Logs', async ({ page }) => {
    await page.goto(`${BASE_URL}/search`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-search');
  });

  test('03 Fontes', async ({ page }) => {
    await page.goto(`${BASE_URL}/sources`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-sources');
  });

  test('04 Retencao', async ({ page }) => {
    await page.goto(`${BASE_URL}/retention`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-retention');
  });

  test('05 Logs (API)', async ({ page }) => {
    await page.goto(`${BASE_URL}/api/logs`);
    await page.waitForLoadState('networkidle');
    await capture(page, '05-logs-api');
  });

  test('06 Estatisticas (API)', async ({ page }) => {
    await page.goto(`${BASE_URL}/api/stats`);
    await page.waitForLoadState('networkidle');
    await capture(page, '06-stats');
  });
});
