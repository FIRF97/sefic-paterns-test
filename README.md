# Sefic Partners API

API RESTful para gestao de parceiros de negocio (clientes, fornecedores ou ambos) em ambiente de empresa unica. Desenvolvida em ASP.NET Core (.NET 9) com Entity Framework Core e Microsoft SQL Server, em conformidade com as especificacoes do documento 02-Fase-Pratica.pdf e com o padrao arquitetural em camadas do projeto de referencia ms-core-fundcube.

---

## 1. Visao Geral da Arquitetura

A solucao esta estruturada em camadas modulares com separacao estrita de responsabilidades:

- **Sefic.Partners.Api**: Controladores HTTP, configuracao do pipeline ASP.NET Core, Swagger OpenAPI, pagina inicial de apresentacao e middleware global de excecoes.
- **Sefic.Partners.Core**: Interfaces e contratos de dominio (IUnitOfWork).
- **Sefic.Partners.Data**: Implementacao do repositorio generico (Repository<T>), Unit of Work (UnitOfWork), construtor de paginacao (PagedResponseBuilder) e extensoes de consulta.
- **Sefic.Partners.Services**: Regras de negocio, validacoes de dominio, mapeamento de DTOs e servico de aplicacao (PartnerService).
- **Sefic.Partners.Shared**: Entidades de dominio (Partner, BaseEntity), enums (PartnerType), modelos de paginacao (PageFilter, PartnerFilter), DbContext (PartnersDbContext) e migracoes do Entity Framework Core.
- **Sefic.Partners.Tests**: Testes automatizados de integracao e unidade cobrindo todos os cenarios do teste pratico.

### Localizacao das Migracoes

As migracoes estao localizadas no assembly Shared (Sefic.Partners.Shared), mantendo o projeto Api desacoplado de dependencias diretas de design de base de dados.

---

## 2. Tecnologias

- **Runtime / SDK**: .NET 9.0 (C# 13)
- **ORM**: Entity Framework Core 9.0.8 (SQL Server Provider, Tools, Design)
- **Base de Dados**: Microsoft SQL Server 2022 (compativel com Developer Edition, Express e LocalDB)
- **Documentacao de API**: Swagger / OpenAPI (Swashbuckle 9.0.3)
- **Containerizacao**: Docker e Docker Compose
- **Testes**: xUnit 2.9.2 e Microsoft.AspNetCore.Mvc.Testing 9.0.0

---

## 3. Regras de Dominio e Validacoes

### Entidade Partner

- **Id**: Identificador unico global (GUID) gerado pelo servidor.
- **Name**: Obrigatorio, entre 2 e 100 caracteres. Espacos no inicio e fim sao removidos (Trim).
- **TaxNumber (NIF)**: Obrigatorio. Formatacao com pontos, tracos ou espacos e removida automaticamente. Exige exatamente 9 digitos numericos e preserva zeros a esquerda.
- **Unicidade do NIF**: Garantida por indice unico (IX_Partners_TaxNumber_Unique) em todo o cadastro. Permanece reservado mesmo apos desativacao (soft delete). Tentativas de duplicidade retornam status 409 Conflict.
- **Type**: Valores permitidos: CUSTOMER, SUPPLIER e BOTH.
- **Email**: Opcional. String vazia ou apenas com espacos e gravada como null. Quando preenchido, valida formato de email e aplica Trim.
- **CreditLimit**: Valor decimal entre 0 e 1.000.000,00 com no maximo duas casas decimais, persistido como decimal(18,2). Valores com precisao superior (ex.: 10.999) sao rejeitados com 400 Bad Request.
- **IsActive**: Booleano. Novo registo inicia ativo (true). A desativacao e efetuada via PUT (soft delete).
- **Version**: Controle de concorrencia otimista real via rowversion do SQL Server, transmitido nos DTOs como token Base64 opaco. Atualizacoes com versao defasada retornam 409 Conflict.
- **CreatedAt / UpdatedAt**: Carimbos de data/hora UTC gerados no servidor. CreatedAt e imutavel apos a criacao.

---

## 4. Endpoints da API

### Gestao de Parceiros (PartnersController)

| Metodo | Rota | Descricao | Status de Sucesso |
|---|---|---|---|
| POST | /api/partners | Cria um parceiro (cliente, fornecedor ou misto). | 201 Created |
| GET | /api/partners/{id} | Obtem dados completos e token de versao do parceiro por GUID. | 200 OK |
| GET | /api/partners | Lista parceiros paginados, ordenados por data de criacao (CreatedAt desc, Id asc). | 200 OK |
| PUT | /api/partners/{id} | Atualiza dados do parceiro com verificacao de concorrencia otimista. | 200 OK |

#### Parametros de Consulta em GET /api/partners

O filtro de consulta herda de PageFilter (PartnerFilter : PageFilter) para manter a assinatura limpa e sem redundancias:

- `pageNumber`: Numero da pagina (padrao: 1). Suporta tambem ?page=1 por compatibilidade.
- `pageSize`: Quantidade de registos por pagina (padrao: 20, maximo: 100).
- `search`: Pesquisa textual sobre Name ou TaxNumber.
- `type`: Filtro por tipo de parceiro (CUSTOMER, SUPPLIER, BOTH).
- `includeInactive`: Booleano (padrao: false). Se true, inclui parceiros inativos.
- `orderBy`: Clausula de ordenacao opcional. Padrao: CreatedAt desc, Id asc.

### Carga de Dados Iniciais (SeedController)

| Metodo | Rota | Descricao | Status de Sucesso |
|---|---|---|---|
| POST | /api/seed/carregarPartners | Carrega os dados de exemplo iniciais na base de dados. | 200 OK |

### Apresentacao e Documentacao

| Metodo | Rota | Descricao |
|---|---|---|
| GET | / | Pagina HTML tecnica com apresentacao da API, resumo do que foi feito e opcao para carregar dados iniciais. |
| GET | /swagger | Interface interativa Swagger UI / OpenAPI. |

---

## 5. Estrutura da Resposta Paginada

O endpoint GET /api/partners devolve o envelope de paginacao padronizado:

```json
{
  "pageSize": 20,
  "pageNumber": 1,
  "totalPages": 1,
  "totalRecords": 4,
  "records": 4,
  "nextPage": null,
  "previousPage": null,
  "firstPage": "http://localhost:5000/api/partners?pageNumber=1&pageSize=20",
  "lastPage": "http://localhost:5000/api/partners?pageNumber=1&pageSize=20",
  "data": [
    {
      "id": "b9635bd6-0b44-4cdd-a899-9d6babe5f396",
      "name": "Ana Silva",
      "taxNumber": "001234567",
      "type": "CUSTOMER",
      "email": "ana.silva@example.com",
      "creditLimit": 5000.00,
      "isActive": true,
      "version": "AAAAAAAAB9E=",
      "createdAt": "2026-10-14T22:05:22.5266667Z",
      "updatedAt": "2026-10-14T22:05:22.5266667Z"
    }
  ],
  "retorno": {
    "status": 200,
    "mensagem": "Partner"
  }
}
```

---

## 6. Configuracao e Execucao

### Execucao via Docker Compose

```bash
docker compose up --build
```

- Swagger UI: http://localhost:8080/swagger
- Painel de Apresentacao: http://localhost:8080/
- SQL Server: localhost:1433 (Utilizador: sa, Senha: YourStrong@Passw0rd)

### Execucao Local

1. Configurar a string de conexao no ficheiro `Sefic.Partners.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SeficPartnersDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

2. Aplicar as migracoes:

```bash
dotnet ef database update --project Sefic.Partners.Shared --startup-project Sefic.Partners.Api
```

3. Executar a API:

```bash
dotnet run --project Sefic.Partners.Api
```

---

## 7. Testes Automatizados

A suite de testes de integracao e unidade cobre todos os cenarios exigidos:

- Validacao e normalizacao de NIF e Nome com remocao de espacos e pontuacao.
- Reserva de NIF na base de dados mesmo apos desativacao.
- Filtros por tipo de parceiro (CUSTOMER, SUPPLIER, BOTH).
- Desativacao logica e reativacao via PUT.
- Concorrencia otimista real com verificacao de conflitos via token Version.
- Limite de credito com validacao estrita de casas decimais.
- Paginacao com contagem previa (CountAsync) e metadados de navegacao.
- Rota de carga inicial (SeedController).

Para executar os testes:

```bash
dotnet test Sefic.Partners.Tests\Sefic.Partners.Tests.csproj
```
