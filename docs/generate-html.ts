/**
 * RVM.LogStream — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone.
 *
 * Uso:
 *   cd docs && npx tsx generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'dashboard',
    title: '1. Dashboard de Logs',
    description:
      'Painel central com visao em tempo real do fluxo de logs recebidos. ' +
      'Exibe volume por nivel (DEBUG, INFO, WARN, ERROR, FATAL), ' +
      'taxa de ingestao e alertas de anomalias.',
    screenshot: '01-dashboard',
    features: [
      'Volume de logs por nivel nas ultimas 24h (grafico de barras)',
      'Taxa de ingestao atual (logs/segundo)',
      'Top 5 fontes por volume',
      'Alertas de pico de erros',
      'Atualizacao em tempo real via SignalR',
      'Filtro rapido por nivel e periodo',
    ],
    tips: [
      'Picos no grafico de WARN/ERROR indicam problemas em servicos — clique na barra para ir direto a busca filtrada.',
    ],
  },
  {
    id: 'search',
    title: '2. Busca de Logs',
    description:
      'Interface de busca full-text e filtrada sobre os logs armazenados. ' +
      'Suporta filtros por nivel, fonte, periodo e texto livre. ' +
      'Resultados sao paginados e exportaveis.',
    screenshot: '02-search',
    features: [
      'Busca full-text na mensagem do log',
      'Filtros: nivel (DEBUG/INFO/WARN/ERROR/FATAL), fonte, periodo',
      'Visualizacao do stack trace completo ao expandir',
      'Exportacao dos resultados em JSON ou CSV',
      'Paginacao com ate 1000 resultados por pagina',
      'Destaque de palavras encontradas no texto',
    ],
    tips: [
      'Use aspas para busca exata: "NullReferenceException" encontra somente essa frase.',
      'Combine filtros: nivel=ERROR + fonte=api + "timeout" para investigar falhas especificas.',
    ],
  },
  {
    id: 'sources',
    title: '3. Fontes de Logs',
    description:
      'Gerenciamento das fontes de logs registradas. ' +
      'Cada fonte representa um servico ou aplicacao que envia logs ' +
      'para o LogStream via API.',
    screenshot: '03-sources',
    features: [
      'Listagem de fontes ativas e inativas',
      'Volume de logs por fonte (ultimo 7 dias)',
      'API Key exclusiva por fonte',
      'Configuracao de nivel minimo aceito por fonte',
      'Adicionar, editar e remover fontes',
      'Status de conectividade (ultimo log recebido)',
    ],
    tips: [
      'Defina nivel minimo "INFO" em producao para evitar sobrecarga com logs DEBUG.',
      'Cada fonte tem sua propria API Key — revogue separadamente se comprometida.',
    ],
  },
  {
    id: 'retention',
    title: '4. Politica de Retencao',
    description:
      'Configuracao de quanto tempo cada tipo de log e mantido no banco de dados. ' +
      'Logs expirados sao removidos automaticamente pelo worker de limpeza.',
    screenshot: '04-retention',
    features: [
      'Retencao configuravel por nivel de log (dias)',
      'Retencao diferenciada por fonte',
      'Estimativa de espaco ocupado atual',
      'Historico de execucoes do cleanup',
      'Execucao manual do cleanup (forcada)',
      'Alertas quando o volume supera o threshold configurado',
    ],
    tips: [
      'Mantenha logs ERROR e FATAL por pelo menos 90 dias para pos-mortem.',
      'Logs DEBUG podem ter retencao curta (1-7 dias) para economizar espaco.',
    ],
  },
  {
    id: 'logs-api',
    title: '5. API de Ingestao de Logs',
    description:
      'Endpoint REST para envio de logs pelos servicos clientes. ' +
      'Suporta envio individual e em lote (batch). ' +
      'Autenticacao via API Key no header.',
    screenshot: '05-logs-api',
    features: [
      'POST /api/logs — ingestao individual ou batch (ate 1000 logs por chamada)',
      'GET /api/logs — consulta com filtros (nivel, fonte, periodo, texto)',
      'Campos: nivel, mensagem, fonte, timestamp, propriedades extras (JSON)',
      'Autenticacao via header X-Api-Key',
      'Rate limiting: 500 req/min por chave',
      'Resposta com ID de cada log ingerido',
    ],
    tips: [
      'Use batch para alta vazao — uma chamada com 100 logs e muito mais eficiente que 100 chamadas.',
    ],
  },
  {
    id: 'stats',
    title: '6. Estatisticas',
    description:
      'Relatorios e estatisticas de uso do LogStream: volume historico, ' +
      'distribuicao por nivel e por fonte, e tendencias de crescimento.',
    screenshot: '06-stats',
    features: [
      'Volume total de logs por periodo (7, 30, 90 dias)',
      'Distribuicao percentual por nivel',
      'Top 10 fontes por volume',
      'Taxa de crescimento semanal',
      'Estimativa de crescimento futuro',
      'Exportacao de relatorio em PDF',
    ],
  },
];

// ---------------------------------------------------------------------------
// Utilitarios
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.LogStream - Manual do Usuario</title>
  <style>
    :root { --primary: #0891b2; --surface: #ffffff; --bg: #f4f6fa; --text: #1e293b; --text-muted: #64748b; --border: #e2e8f0; --sidebar-bg: #0f172a; --accent: #06b6d4; }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: var(--bg); color: var(--text); line-height: 1.6; }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header { background: var(--sidebar-bg); color: white; padding: 3rem 1.5rem; text-align: center; }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #94a3b8; font-size: 1rem; }
    header .version { color: #64748b; font-size: 0.85rem; margin-top: 0.5rem; }
    nav { background: var(--surface); border-bottom: 1px solid var(--border); padding: 1rem 1.5rem; position: sticky; top: 0; z-index: 100; }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a { display: inline-block; padding: 0.35rem 0.75rem; border-radius: 0.5rem; font-size: 0.85rem; color: var(--text); text-decoration: none; background: var(--bg); transition: background 0.2s; }
    nav a:hover { background: var(--primary); color: white; }
    section { background: var(--surface); border: 1px solid var(--border); border-radius: 1rem; padding: 2rem; margin-bottom: 2rem; }
    section h2 { font-size: 1.5rem; color: var(--primary); margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 2px solid var(--border); }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips { background: var(--bg); border-radius: 0.75rem; padding: 1rem 1.25rem; margin-bottom: 1.25rem; }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #ecfeff; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--primary); }
    .screenshots { display: flex; gap: 1.5rem; margin-top: 1rem; align-items: flex-start; }
    .screenshot-group { position: relative; flex: 1; border: 1px solid var(--border); border-radius: 0.75rem; overflow: hidden; }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge { position: absolute; top: 0.5rem; right: 0.5rem; background: var(--sidebar-bg); color: white; font-size: 0.7rem; padding: 0.2rem 0.5rem; border-radius: 0.35rem; font-weight: 600; text-transform: uppercase; }
    .no-screenshot { background: var(--bg); padding: 2rem; border-radius: 0.75rem; text-align: center; color: var(--text-muted); }
    footer { text-align: center; padding: 2rem 1rem; color: var(--text-muted); font-size: 0.85rem; }
    @media (max-width: 768px) { .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 100%; flex: 1; } section { padding: 1.25rem; } }
    @media print { nav { display: none; } section { break-inside: avoid; page-break-inside: avoid; } .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 250px; } }
  </style>
</head>
<body>
  <header>
    <h1>RVM.LogStream - Manual do Usuario</h1>
    <p>Logs Centralizados com Busca e Retencao — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.LogStream</strong> e um sistema de logs centralizados para multiplos servicos.
        Recebe logs via API REST, armazena com politicas de retencao configuradas,
        e oferece busca full-text, dashboard em tempo real e relatorios de uso.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Ingestao via API</strong> — individual ou batch, autenticado por API Key</li>
          <li><strong>Busca full-text</strong> — filtros por nivel, fonte, periodo e texto</li>
          <li><strong>Dashboard real-time</strong> — volume e anomalias via SignalR</li>
          <li><strong>Retencao configuravel</strong> — por nivel de log e por fonte</li>
          <li><strong>Multiplas fontes</strong> — cada servico tem sua propria API Key</li>
          <li><strong>Exportacao</strong> — resultados em JSON ou CSV</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Logs Centralizados</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.LogStream - Manual do Usuario

> Logs Centralizados com Busca e Retencao — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.LogStream** centraliza logs de multiplos servicos com busca full-text e retencao configuravel.

**Recursos principais:**
- **Ingestao via API** — individual ou batch, autenticado por API Key
- **Busca full-text** — filtros por nivel, fonte, periodo e texto
- **Dashboard real-time** — volume e anomalias via SignalR
- **Retencao configuravel** — por nivel de log e por fonte
- **Exportacao** — resultados em JSON ou CSV

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`));

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) md += `- ${f}\n`;
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) md += `> - ${t}\n`;
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Tecnologia** | ASP.NET Core + Blazor Server |
| **Tempo real** | SignalR (WebSocket) |
| **Banco de dados** | PostgreSQL 16 |
| **Busca** | Full-text nativo PostgreSQL (tsvector) |
| **Autenticacao** | API Key por fonte |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
