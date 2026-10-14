namespace Sefic.Partners.Api.Helper;

public static class WelcomePageHelper
{
    public static string GetHtml()
    {
        return """
<!DOCTYPE html>
<html lang="pt">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sefic Partners API — Painel de Apresentação</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=JetBrains+Mono:wght@400;600&display=swap" rel="stylesheet">
    <style>
        :root {
            --bg-base: #070b14;
            --bg-surface: #0f172a;
            --bg-card: rgba(17, 24, 39, 0.75);
            --border-subtle: rgba(255, 255, 255, 0.08);
            --border-hover: rgba(99, 102, 241, 0.4);
            --primary: #6366f1;
            --primary-light: #818cf8;
            --primary-glow: rgba(99, 102, 241, 0.25);
            --accent-cyan: #06b6d4;
            --accent-emerald: #10b981;
            --accent-amber: #f59e0b;
            --text-main: #f8fafc;
            --text-muted: #94a3b8;
            --text-dim: #64748b;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Inter', system-ui, -apple-system, sans-serif;
            background-color: var(--bg-base);
            color: var(--text-main);
            min-height: 100vh;
            line-height: 1.6;
            background-image: 
                radial-gradient(circle at 15% 15%, rgba(99, 102, 241, 0.12) 0%, transparent 45%),
                radial-gradient(circle at 85% 20%, rgba(6, 182, 212, 0.1) 0%, transparent 40%),
                radial-gradient(circle at 50% 80%, rgba(16, 185, 129, 0.08) 0%, transparent 50%);
            background-attachment: fixed;
            padding: 2.5rem 1.5rem 4rem;
        }

        .container {
            max-width: 1180px;
            margin: 0 auto;
        }

        /* Hero */
        .hero {
            text-align: center;
            padding: 2.5rem 1rem 3.5rem;
            position: relative;
        }

        .badge-status {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 1rem;
            border-radius: 9999px;
            background: rgba(16, 185, 129, 0.12);
            border: 1px solid rgba(16, 185, 129, 0.3);
            color: #34d399;
            font-size: 0.85rem;
            font-weight: 600;
            letter-spacing: 0.02em;
            margin-bottom: 1.5rem;
            box-shadow: 0 0 20px rgba(16, 185, 129, 0.15);
        }

        .pulse-dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #10b981;
            box-shadow: 0 0 10px #10b981;
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0%, 100% { opacity: 1; transform: scale(1); }
            50% { opacity: 0.4; transform: scale(0.85); }
        }

        .hero h1 {
            font-size: clamp(2.3rem, 5vw, 3.8rem);
            font-weight: 800;
            letter-spacing: -0.03em;
            line-height: 1.15;
            background: linear-gradient(135deg, #ffffff 30%, #94a3b8 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 1rem;
        }

        .hero h1 span {
            background: linear-gradient(135deg, #818cf8 0%, #06b6d4 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        .hero p {
            font-size: 1.15rem;
            color: var(--text-muted);
            max-width: 760px;
            margin: 0 auto 2rem;
            font-weight: 400;
        }

        /* Action Buttons */
        .cta-group {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 1rem;
            margin-bottom: 3.5rem;
        }

        .btn {
            display: inline-flex;
            align-items: center;
            gap: 0.6rem;
            padding: 0.85rem 1.6rem;
            border-radius: 12px;
            font-size: 0.95rem;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
            cursor: pointer;
            border: 1px solid transparent;
        }

        .btn-primary {
            background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
            color: #ffffff;
            box-shadow: 0 8px 24px -4px var(--primary-glow);
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 12px 30px -4px rgba(99, 102, 241, 0.45);
            background: linear-gradient(135deg, #6d70f6 0%, #5851eb 100%);
        }

        .btn-seed {
            background: rgba(16, 185, 129, 0.12);
            color: #34d399;
            border-color: rgba(16, 185, 129, 0.35);
        }

        .btn-seed:hover {
            background: rgba(16, 185, 129, 0.22);
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(16, 185, 129, 0.2);
        }

        .btn-secondary {
            background: rgba(255, 255, 255, 0.05);
            color: var(--text-main);
            border-color: var(--border-subtle);
            backdrop-filter: blur(10px);
        }

        .btn-secondary:hover {
            background: rgba(255, 255, 255, 0.09);
            border-color: rgba(255, 255, 255, 0.18);
            transform: translateY(-2px);
        }

        /* Toast feedback */
        #seedToast {
            display: none;
            position: fixed;
            bottom: 2rem;
            right: 2rem;
            background: #0f172a;
            border: 1px solid #10b981;
            color: #f8fafc;
            padding: 1rem 1.4rem;
            border-radius: 12px;
            box-shadow: 0 15px 35px rgba(0,0,0,0.5), 0 0 25px rgba(16, 185, 129, 0.25);
            font-size: 0.95rem;
            z-index: 9999;
            align-items: center;
            gap: 0.75rem;
            animation: slideIn 0.3s ease-out;
        }

        @keyframes slideIn {
            from { transform: translateY(100px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }

        /* Section Title */
        .section-header {
            margin-bottom: 2rem;
            display: flex;
            align-items: flex-end;
            justify-content: space-between;
            border-bottom: 1px solid var(--border-subtle);
            padding-bottom: 0.8rem;
        }

        .section-header h2 {
            font-size: 1.5rem;
            font-weight: 700;
            color: #f1f5f9;
        }

        .section-header span {
            color: var(--text-dim);
            font-size: 0.88rem;
        }

        /* Grid Cards */
        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
            gap: 1.5rem;
            margin-bottom: 3.5rem;
        }

        .card {
            background: var(--bg-card);
            border: 1px solid var(--border-subtle);
            border-radius: 16px;
            padding: 1.8rem;
            backdrop-filter: blur(12px);
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }

        .card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 3px;
            background: linear-gradient(90deg, transparent, var(--accent-color, #6366f1), transparent);
            opacity: 0;
            transition: opacity 0.3s ease;
        }

        .card:hover {
            transform: translateY(-4px);
            border-color: var(--border-hover);
            box-shadow: 0 16px 32px -8px rgba(0, 0, 0, 0.4), 0 0 25px var(--primary-glow);
        }

        .card:hover::before {
            opacity: 1;
        }

        .card-icon {
            width: 44px;
            height: 44px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.3rem;
            margin-bottom: 1.2rem;
            background: rgba(99, 102, 241, 0.12);
            border: 1px solid rgba(99, 102, 241, 0.25);
            color: #a5b4fc;
        }

        .card h3 {
            font-size: 1.15rem;
            font-weight: 600;
            color: #f8fafc;
            margin-bottom: 0.6rem;
        }

        .card p {
            color: var(--text-muted);
            font-size: 0.92rem;
            margin-bottom: 1rem;
        }

        .card ul {
            list-style: none;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .card li {
            font-size: 0.88rem;
            color: #cbd5e1;
            display: flex;
            align-items: flex-start;
            gap: 0.5rem;
        }

        .card li::before {
            content: "✓";
            color: #34d399;
            font-weight: bold;
        }

        /* Endpoints Table Card */
        .endpoints-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
        }

        .endpoints-table th {
            text-align: left;
            padding: 0.9rem 1rem;
            color: var(--text-dim);
            font-weight: 600;
            border-bottom: 1px solid var(--border-subtle);
            text-transform: uppercase;
            font-size: 0.75rem;
            letter-spacing: 0.05em;
        }

        .endpoints-table td {
            padding: 1rem;
            border-bottom: 1px solid rgba(255, 255, 255, 0.04);
            color: #e2e8f0;
        }

        .badge-method {
            font-family: 'JetBrains Mono', monospace;
            font-size: 0.75rem;
            font-weight: 700;
            padding: 0.25rem 0.55rem;
            border-radius: 6px;
            letter-spacing: 0.04em;
        }

        .method-get { background: rgba(6, 182, 212, 0.15); color: #22d3ee; border: 1px solid rgba(6, 182, 212, 0.3); }
        .method-post { background: rgba(16, 185, 129, 0.15); color: #34d399; border: 1px solid rgba(16, 185, 129, 0.3); }
        .method-put { background: rgba(245, 158, 11, 0.15); color: #fbbf24; border: 1px solid rgba(245, 158, 11, 0.3); }

        .route-code {
            font-family: 'JetBrains Mono', monospace;
            color: #f1f5f9;
            font-size: 0.85rem;
        }

        /* Footer */
        footer {
            margin-top: 4rem;
            padding-top: 2rem;
            border-top: 1px solid var(--border-subtle);
            text-align: center;
            color: var(--text-dim);
            font-size: 0.88rem;
        }

        footer strong {
            color: #cbd5e1;
        }
    </style>
</head>
<body>
    <div class="container">
        <!-- Hero Section -->
        <header class="hero">
            <div class="badge-status">
                <div class="pulse-dot"></div>
                API Operacional — .NET 9.0 & SQL Server
            </div>
            <h1>Sefic Partners <span>Web API</span></h1>
            <p>Sistema completo de gestão de clientes e fornecedores empresariais, desenvolvido com Entity Framework Core e Microsoft SQL Server sob a arquitetura de referência <code>ms-core-fundcube</code>.</p>
            
            <div class="cta-group">
                <a href="/swagger" class="btn btn-primary" id="btn-swagger">
                    <span>⚡ Explorar Swagger UI</span>
                </a>
                <button class="btn btn-seed" id="btn-seed" onclick="executarSeed()">
                    <span>🌱 Executar Seed Inicial</span>
                </button>
                <a href="/api/partners" class="btn btn-secondary" id="btn-partners">
                    <span>📊 Listar Parceiros (JSON)</span>
                </a>
            </div>
        </header>

        <!-- Seção: O que foi feito -->
        <div class="section-header">
            <h2>O Que Foi Implementado</h2>
            <span>Conformidade com 02-Fase-Pratica.pdf</span>
        </div>

        <div class="grid">
            <!-- Card 1 -->
            <div class="card" style="--accent-color: #6366f1;">
                <div class="card-icon">🏛️</div>
                <h3>Arquitetura em 5 Camadas</h3>
                <p>Espinha dorsal modular inspirada no padrão corporativo Fundcube:</p>
                <ul>
                    <li><strong>Sefic.Partners.Api:</strong> Controllers enxutos e tratamento global.</li>
                    <li><strong>Sefic.Partners.Core:</strong> Contrato <code>IUnitOfWork</code> de domínio.</li>
                    <li><strong>Sefic.Partners.Data:</strong> Repositórios genéricos e paginação SQL Server.</li>
                    <li><strong>Sefic.Partners.Services:</strong> Validações de domínio e orquestração DTO.</li>
                    <li><strong>Sefic.Partners.Shared:</strong> Entidades, enums e <strong>migrações dedicadas</strong>.</li>
                </ul>
            </div>

            <!-- Card 2 -->
            <div class="card" style="--accent-color: #06b6d4;">
                <div class="card-icon">📑</div>
                <h3>Paginação Padronizada & URLs</h3>
                <p>Estrutura idêntica à especificação solicitada com metadados de navegação:</p>
                <ul>
                    <li><code>pageSize</code>, <code>pageNumber</code>, <code>totalPages</code>, <code>totalRecords</code>.</li>
                    <li>Contagem real no banco via <code>CountAsync()</code> antes do Skip/Take.</li>
                    <li>Links completos gerados: <code>firstPage</code>, <code>lastPage</code>, <code>nextPage</code>, <code>previousPage</code>.</li>
                    <li>Ordenação padrão definida: <strong>mais recentes primeiro</strong> (<code>CreatedAt desc, Id asc</code>).</li>
                </ul>
            </div>

            <!-- Card 3 -->
            <div class="card" style="--accent-color: #10b981;">
                <div class="card-icon">🎯</div>
                <h3>PartnerFilter Limpo (Herança de PageFilter)</h3>
                <p>Eliminação total de redundância e duplicação no Swagger UI:</p>
                <ul>
                    <li><code>PageFilter</code>: Base genérica contendo <code>pageNumber</code>, <code>pageSize</code> e <code>orderBy</code>.</li>
                    <li><code>PartnerFilter</code>: Herda de <code>PageFilter</code> e contém apenas os campos da entidade: <code>search</code>, <code>type</code> e <code>includeInactive</code>.</li>
                    <li>Zero parâmetros duplicados ou internos expostos no Swagger.</li>
                    <li>Suporte transparente aos aliases <code>?page=1</code> e <code>?pageNumber=1</code>.</li>
                </ul>
            </div>

            <!-- Card 4 -->
            <div class="card" style="--accent-color: #f59e0b;">
                <div class="card-icon">🔒</div>
                <h3>Concorrência Otimista Real</h3>
                <p>Proteção contra o problema de perda de atualização (Lost Update):</p>
                <ul>
                    <li>Propriedade <code>byte[] Version</code> baseada em <code>rowversion</code> do SQL Server.</li>
                    <li>Exposta no DTO como <strong>token Base64 opaco</strong>.</li>
                    <li>Ao tentar atualizar com uma versão defasada, a API devolve <code>409 Conflict</code>.</li>
                    <li>Rejeição imediata de tokens com formato Base64 inválido (<code>400 Bad Request</code>).</li>
                </ul>
            </div>

            <!-- Card 5 -->
            <div class="card" style="--accent-color: #ec4899;">
                <div class="card-icon">🛡️</div>
                <h3>Normalização & Regras de Negócio</h3>
                <p>Validações rigorosas garantindo integridade máxima do cadastro:</p>
                <ul>
                    <li><strong>NIF/TaxNumber:</strong> Normalização de pontuação e espaços, exigindo 9 dígitos com zeros à esquerda preservados.</li>
                    <li><strong>NIF Único na BD:</strong> Índice único no SQL Server, reservado mesmo após desativação (soft delete).</li>
                    <li><strong>CreditLimit:</strong> 0 a 1.000.000,00 com bloqueio estrito de precisão excessiva (> 2 casas decimais).</li>
                    <li><strong>Type:</strong> Restrito aos enums <code>CUSTOMER</code>, <code>SUPPLIER</code> e <code>BOTH</code>.</li>
                </ul>
            </div>

            <!-- Card 6 -->
            <div class="card" style="--accent-color: #10b981;">
                <div class="card-icon">🌱</div>
                <h3>SeedController Dedicado</h3>
                <p>Controlador isolado para carga e redefinição de dados de demonstração:</p>
                <ul>
                    <li>Endpoint oficial: <code>POST /api/seed/carregarPartners</code>.</li>
                    <li>Carga de parceiros de exemplo (clientes, fornecedores e mistos).</li>
                    <li>Reativação inteligente: se o registo já existia inativo, é reativado no seed.</li>
                    <li>Controlador de parceiros (<code>PartnersController</code>) mantido 100% focado no CRUD REST.</li>
                </ul>
            </div>
        </div>

        <!-- Seção: Tabela de Endpoints -->
        <div class="section-header">
            <h2>Endpoints Principais da API</h2>
            <span>Rotas RESTful disponíveis</span>
        </div>

        <div class="card" style="padding: 0; overflow-x: auto; margin-bottom: 2rem;">
            <table class="endpoints-table">
                <thead>
                    <tr>
                        <th>Método</th>
                        <th>Endpoint</th>
                        <th>Descrição</th>
                        <th>Sucesso</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><span class="badge-method method-get">GET</span></td>
                        <td><span class="route-code">/api/partners</span></td>
                        <td>Lista parceiros com paginação, ordenação por data e filtros (search, type, includeInactive).</td>
                        <td><code>200 OK</code></td>
                    </tr>
                    <tr>
                        <td><span class="badge-method method-get">GET</span></td>
                        <td><span class="route-code">/api/partners/{id}</span></td>
                        <td>Obtém detalhes do parceiro e token opaco de versão pelo GUID (incluindo inativos).</td>
                        <td><code>200 OK</code></td>
                    </tr>
                    <tr>
                        <td><span class="badge-method method-post">POST</span></td>
                        <td><span class="route-code">/api/partners</span></td>
                        <td>Cria novo parceiro normalizando Nome e NIF com validação de duplicidade.</td>
                        <td><code>201 Created</code></td>
                    </tr>
                    <tr>
                        <td><span class="badge-method method-put">PUT</span></td>
                        <td><span class="route-code">/api/partners/{id}</span></td>
                        <td>Atualiza parceiro ou altera estado ativo/inativo com controle de concorrência.</td>
                        <td><code>200 OK</code></td>
                    </tr>
                    <tr>
                        <td><span class="badge-method method-post">POST</span></td>
                        <td><span class="route-code">/api/seed/carregarPartners</span></td>
                        <td>Carrega os dados iniciais de parceiros de demonstração na base de dados.</td>
                        <td><code>200 OK</code></td>
                    </tr>
                    <tr>
                        <td><span class="badge-method method-get">GET</span></td>
                        <td><span class="route-code">/swagger</span></td>
                        <td>Interface interativa OpenAPI / Swagger para teste de todas as operações.</td>
                        <td><code>200 OK</code></td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Toast de Notificação -->
        <div id="seedToast">
            <span id="toastIcon">🌱</span>
            <span id="toastMsg">Seed executado com sucesso!</span>
        </div>

        <!-- Footer -->
        <footer>
            <p><strong>Sefic Partners API</strong> &bull; Desenvolvido em conformidade com as especificações da Fase 2 (Sefic Web) &bull; .NET 9.0 LTS</p>
        </footer>
    </div>

    <script>
        async function executarSeed() {
            const btn = document.getElementById('btn-seed');
            const toast = document.getElementById('seedToast');
            const toastMsg = document.getElementById('toastMsg');
            const toastIcon = document.getElementById('toastIcon');

            btn.disabled = true;
            btn.style.opacity = '0.6';
            btn.innerHTML = '<span>⏳ A carregar dados...</span>';

            try {
                const response = await fetch('/api/seed/carregarPartners', {
                    method: 'POST',
                    headers: { 'Accept': 'application/json' }
                });

                const data = await response.json();

                toastIcon.textContent = response.ok ? '✅' : '❌';
                toastMsg.textContent = data.retorno?.mensagem || data.message || 'Dados iniciais carregados com sucesso!';
                toast.style.borderColor = response.ok ? '#10b981' : '#ef4444';
                toast.style.display = 'flex';

                setTimeout(() => {
                    toast.style.display = 'none';
                }, 5000);
            } catch (err) {
                toastIcon.textContent = '❌';
                toastMsg.textContent = 'Erro ao conectar à API para executar o seed.';
                toast.style.borderColor = '#ef4444';
                toast.style.display = 'flex';

                setTimeout(() => {
                    toast.style.display = 'none';
                }, 5000);
            } finally {
                btn.disabled = false;
                btn.style.opacity = '1';
                btn.innerHTML = '<span>🌱 Executar Seed Inicial</span>';
            }
        }
    </script>
</body>
</html>
""";
    }
}
