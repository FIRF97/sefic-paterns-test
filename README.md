# Sefic Partners API — Gestão de Clientes e Fornecedores

API RESTful desenvolvida em **ASP.NET Core (C#)** com **Entity Framework Core** e **SQL Server**, projetada para a gestão de parceiros de negócio (clientes, fornecedores ou ambos) de uma única empresa, em total conformidade com os requisitos do documento **`02-Fase-Pratica.pdf`** e seguindo a arquitetura em camadas do projeto de referência **`ms-core-fundcube`**.

---

## 🛠️ Tecnologias e Versões

- **.NET SDK / Runtime:** `.NET 9.0` (C# 13) / Compatível com `.NET 8.0 LTS`
- **Entity Framework Core:** `9.0.8` (SQL Server Provider, Tools, Design)
- **Base de Dados:** Microsoft SQL Server 2022 (compatível com Developer Edition, Express e LocalDB)
- **Containerização:** Docker e Docker Compose
- **Testes Automatizados:** xUnit `2.9.2`, Microsoft.AspNetCore.Mvc.Testing `9.0.0`
- **Documentação de API:** Swagger / OpenAPI via Swashbuckle `9.0.3`

---

## 🏛️ Arquitetura do Projeto (Baseada no Padrão Fundcube)

A solução está estruturada em 6 projetos modulares, mantendo a separação clara de responsabilidades:

```
Sefic.Partners/
├── Sefic.Partners.Api/          # Camada de Apresentação (Controllers, Middleware Global, DI, Dockerfile)
├── Sefic.Partners.Core/         # Contratos e Interfaces de Domínio (IUnitOfWork)
├── Sefic.Partners.Data/         # Persistência EF Core (PartnersDbContext, Repository<T>, UnitOfWork, PagedResponseBuilder)
├── Sefic.Partners.Services/     # Regras de Negócio, Validações, DTOs e IPartnerService
├── Sefic.Partners.Shared/       # Entidades (Partner, BaseEntity), Enums, Modelos Comuns e MIGRATIONS
├── Sefic.Partners.Tests/        # Testes Automatizados (Cobrindo todos os 7 cenários mínimos)
├── data/                        # Scripts SQL de exemplo (seed.sql)
├── docker-compose.yml           # Orquestração do SQL Server e da API
├── requests.http                # Exemplos de requisições HTTP para todos os endpoints
└── README.md                    # Guia completo de preparação, execução e testes
```

### Destaque da Regra das Migrações:
Conforme solicitado expressamente, **as migrações NÃO ficam no projeto Api, mas sim em `Shared` (`Sefic.Partners.Shared`)**:
- O `PartnersDbContext` está configurado com `sql.MigrationsAssembly("Sefic.Partners.Shared")`.
- A pasta `Sefic.Partners.Shared/Migrations/` está preparada para receber as migrações geradas pelo utilizador.
- **Nenhuma migração foi pré-gerada no projeto** para que o utilizador a gere conforme as suas instruções.

---

## ⚙️ Regras de Domínio e Validações Implementadas

1. **Entidade Partner:**
   - `Id`: GUID gerado pelo servidor.
   - `Name`: Obrigatório, entre 2 e 100 caracteres após `Trim()`.
   - `TaxNumber` (NIF): Obrigatório, normaliza removendo espaços, pontos e hífenes (`.`, `-`, ` `) e exige exatamente 9 dígitos ASCII. Preserva zeros iniciais.
   - `TaxNumber Único na BD`: Garantido por índice único (`IX_Partners_TaxNumber_Unique`) em todo o cadastro (incluindo inativos) e resiliente a pedidos concorrentes sem lock em memória.
   - `Type`: Aceita `CUSTOMER`, `SUPPLIER` ou `BOTH`.
   - `Email`: Opcional. String vazia ou apenas espaços é guardada como `null`. Quando preenchido, aplica `Trim()` e valida o formato de e-mail.
   - `CreditLimit`: Decimal entre 0 e 1.000.000, com no máximo duas casas decimais, persistido como `decimal(18,2)`. **Rejeita precisão excessiva (ex: `10.999`) com 400 Bad Request**.
   - `IsActive`: Inicia a `true` na criação. Desativação/reativação é feita via `PUT /api/partners/{id}`.
   - `Version`: Token opaco em Base64 baseado em `rowversion` do SQL Server para **concorrência otimista real**.
   - `CreatedAt` e `UpdatedAt`: Definidos pelo servidor em UTC (`CreatedAt` nunca é alterado pelo cliente).

2. **Endpoints:**
   - `POST /api/partners`: Cria entidade. Retorna `201 Created` com header `Location`, `Id` e todos os dados incluindo `Version`. Se NIF duplicado, retorna `409 Conflict`.
   - `GET /api/partners/{id}`: Retorna dados e `Version` (inclusive de inativas). Se não existir, retorna `404 Not Found`.
   - `GET /api/partners`: Retorna `Items`, `TotalCount`, `Page` e `PageSize`. Suporta `search`, `type`, `includeInactive` (false por defeito), `page` (>= 1) e `pageSize` (1 a 100). Ordena por `Name ASC, Id ASC`.
   - `PUT /api/partners/{id}`: Atualiza campos. Exige todos os campos e `Version`. Retorna `200 OK` com nova `Version`. Se concorrência ou NIF de outro registo, retorna `409 Conflict`.

---

## 🚀 Como Gerar as Migrações (Passo Obrigatório Inicial)

Como as migrações não foram pré-geradas, gere a migração inicial apontando para o projeto `Shared` com o comando:

```bash
# 1. Navegue até a pasta da solução
cd Sefic.Partners

# 2. Gerar a migração em Sefic.Partners.Shared
dotnet ef migrations add InitialCreate --project Sefic.Partners.Shared --startup-project Sefic.Partners.Api

# 3. Aplicar a migração à Base de Dados
dotnet ef database update --project Sefic.Partners.Shared --startup-project Sefic.Partners.Api
```

> **Nota:** Caso não tenha a ferramenta CLI do EF Core instalada globalmente, instale-a via:
> `dotnet tool install --global dotnet-ef`

---

## 🐳 Executando com Docker

O projeto já está 100% configurado para rodar com Docker:

```bash
cd Sefic.Partners

# Iniciar o SQL Server e a API
docker compose up --build
```

- **Swagger da API:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Porta da API:** `8080`
- **SQL Server:** `localhost:1433` (Utilizador: `sa`, Palavra-passe: `YourStrong@Passw0rd`)

---

## 💻 Executando Localmente (Sem Docker)

1. Certifique-se de que tem o SQL Server ou SQL Server LocalDB em execução.
2. Atualize a Connection String em `appsettings.Development.json` caso necessário.
3. Execute:

```bash
cd Sefic.Partners
dotnet restore
dotnet build
dotnet run --project Sefic.Partners.Api
```

Aceda ao Swagger em: `http://localhost:5000/swagger` ou na porta configurada pelo Kestrel.

---

## 🧪 Testes Automatizados

Foram implementados testes cobrindo rigorosamente todos os **7 cenários mínimos exigidos no enunciado**:

| Cenário | Descrição |
|---|---|
| **Cenário 1** | Criar com `" Ana Silva "` e `"001.234.567"`; normalização de nome e NIF; tentativa duplicada com `"001 234 567"` retorna 409; NIF continua reservado após desativação. |
| **Cenário 2** | Entidade `BOTH` aparece nos filtros `CUSTOMER`, `SUPPLIER` e `BOTH`. Um `CUSTOMER` exclusivo não aparece no filtro `SUPPLIER`. |
| **Cenário 3** | Desativação via `PUT` remove da listagem normal, mantém em `includeInactive=true` e acessível por ID; reativação restaura listagem padrão. |
| **Cenário 4** | Atualização mantendo o próprio NIF é permitida. Mudar para NIF de outro parceiro devolve 409 e preserva dados originais. |
| **Cenário 5** | Criações simultâneas com mesmo NIF: uma devolve 201 e outra 409 (apenas 1 registo na BD). Atualizações simultâneas com a mesma `Version`: uma devolve 200 e outra 409 (concorrência otimista via SQL Server). |
| **Cenário 6** | `CreditLimit=10,999` e NIF com letras devolvem 400. `Page=0`, `Type` inválido e `Version` malformada devolvem 400 sem alterar a base de dados. |
| **Cenário 7** | Filtros combinados, `TotalCount` calculado antes da paginação, desempate por `Id` para nomes iguais e `Search="Ana"` sem corresponder a todas as entidades pelo NIF. |

Para executar a suíte de testes:

```bash
cd Sefic.Partners
dotnet test
```

---

## 📦 Dados de Exemplo (Seed Data)

Para carregar dados de exemplo com diferentes tipos de clientes e fornecedores, utilize o script SQL disponível em:

```
Sefic.Partners/data/seed.sql
```

Pode executá-lo diretamente via `sqlcmd`, Azure Data Studio ou SQL Server Management Studio (SSMS).

---

## 📄 Exemplos de Chamadas HTTP

O ficheiro **`requests.http`** na raiz do projeto contém exemplos prontos para execução com extensões como REST Client (VS Code) ou HttpClient (Visual Studio / JetBrains Rider).
