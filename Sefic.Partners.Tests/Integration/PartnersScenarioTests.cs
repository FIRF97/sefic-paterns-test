using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Sefic.Partners.Services.DTO;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Models;
using Xunit;

namespace Sefic.Partners.Tests.Integration;

public class PartnersScenarioTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PartnersScenarioTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Cenário 1:
    /// Criar cliente com Name=" Ana Silva " e NIF "001.234.567": guardar "Ana Silva" e "001234567".
    /// Um novo pedido com "001 234 567" devolve 409. O NIF continua reservado após desativação.
    /// </summary>
    [Fact]
    public async Task Scenario1_CreateClient_TrimAndNifNormalization_AndNifReservedAfterDeactivation()
    {
        var createDto = new PartnerCreateDto
        {
            Name = " Ana Silva ",
            TaxNumber = "001.234.567",
            Type = PartnerType.CUSTOMER,
            Email = "ana.silva@example.com",
            CreditLimit = 5000.00m
        };

        // 1. Criar cliente com espaços e pontuação
        var createResponse = await _client.PostAsJsonAsync("/api/partners", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Ana Silva", created.Name);
        Assert.Equal("001234567", created.TaxNumber);
        Assert.True(created.IsActive);
        Assert.False(string.IsNullOrWhiteSpace(created.Version));

        // 2. Novo pedido com mesmo NIF em formato diferente ("001 234 567") devolve 409
        var duplicateDto = new PartnerCreateDto
        {
            Name = "Outra Ana",
            TaxNumber = "001 234 567",
            Type = PartnerType.CUSTOMER,
            Email = "outra@example.com",
            CreditLimit = 1000m
        };
        var duplicateResponse = await _client.PostAsJsonAsync("/api/partners", duplicateDto);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        // 3. Desativar cliente criado via PUT
        var updateDto = new PartnerUpdateDto
        {
            Name = created.Name,
            TaxNumber = created.TaxNumber,
            Type = created.Type,
            Email = created.Email,
            CreditLimit = created.CreditLimit,
            IsActive = false,
            Version = created.Version
        };
        var deactivateResponse = await _client.PutAsJsonAsync($"/api/partners/{created.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        // 4. O NIF continua reservado após desativação (devolve 409)
        var postAfterDeactivation = await _client.PostAsJsonAsync("/api/partners", duplicateDto);
        Assert.Equal(HttpStatusCode.Conflict, postAfterDeactivation.StatusCode);
    }

    /// <summary>
    /// Cenário 2:
    /// Criar uma entidade BOTH: aparece nos filtros CUSTOMER, SUPPLIER e BOTH. Um CUSTOMER não aparece no filtro SUPPLIER.
    /// </summary>
    [Fact]
    public async Task Scenario2_PartnerTypeFiltering_BothAppearsInAll_CustomerNotInSupplier()
    {
        var bothDto = new PartnerCreateDto
        {
            Name = "Empresa Mista",
            TaxNumber = "002.345.678",
            Type = PartnerType.BOTH,
            Email = "mista@example.com",
            CreditLimit = 10000m
        };
        var customerDto = new PartnerCreateDto
        {
            Name = "Cliente Exclusivo",
            TaxNumber = "003.456.789",
            Type = PartnerType.CUSTOMER,
            Email = "cliente@example.com",
            CreditLimit = 2000m
        };

        var bothResp = await _client.PostAsJsonAsync("/api/partners", bothDto);
        Assert.Equal(HttpStatusCode.Created, bothResp.StatusCode);
        var bothCreated = await bothResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        var custResp = await _client.PostAsJsonAsync("/api/partners", customerDto);
        Assert.Equal(HttpStatusCode.Created, custResp.StatusCode);
        var custCreated = await custResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // Filtro CUSTOMER (deve incluir CUSTOMER e BOTH)
        var getCustomerFilter = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?type=CUSTOMER", JsonOptions);
        Assert.NotNull(getCustomerFilter);
        Assert.Contains(getCustomerFilter.Items, p => p.Id == bothCreated!.Id);
        Assert.Contains(getCustomerFilter.Items, p => p.Id == custCreated!.Id);

        // Filtro SUPPLIER (deve incluir SUPPLIER e BOTH, mas NÃO o CUSTOMER exclusivo)
        var getSupplierFilter = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?type=SUPPLIER", JsonOptions);
        Assert.NotNull(getSupplierFilter);
        Assert.Contains(getSupplierFilter.Items, p => p.Id == bothCreated!.Id);
        Assert.DoesNotContain(getSupplierFilter.Items, p => p.Id == custCreated!.Id);

        // Filtro BOTH (apenas BOTH)
        var getBothFilter = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?type=BOTH", JsonOptions);
        Assert.NotNull(getBothFilter);
        Assert.Contains(getBothFilter.Items, p => p.Id == bothCreated!.Id);
        Assert.DoesNotContain(getBothFilter.Items, p => p.Id == custCreated!.Id);
    }

    /// <summary>
    /// Cenário 3:
    /// Desativar por PUT: sai da listagem por defeito, aparece com IncludeInactive=true e continua acessível pelo Id.
    /// Reativar devolve-a à listagem habitual.
    /// </summary>
    [Fact]
    public async Task Scenario3_DeactivateAndReactivate_ViaPut()
    {
        var partnerDto = new PartnerCreateDto
        {
            Name = "Entidade Desativavel",
            TaxNumber = "004.567.890",
            Type = PartnerType.SUPPLIER,
            CreditLimit = 3000m
        };
        var createResp = await _client.PostAsJsonAsync("/api/partners", partnerDto);
        var created = await createResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.NotNull(created);

        // 1. Desativar por PUT
        var deactivateDto = new PartnerUpdateDto
        {
            Name = created.Name,
            TaxNumber = created.TaxNumber,
            Type = created.Type,
            Email = created.Email,
            CreditLimit = created.CreditLimit,
            IsActive = false,
            Version = created.Version
        };
        var deactivateResp = await _client.PutAsJsonAsync($"/api/partners/{created.Id}", deactivateDto);
        Assert.Equal(HttpStatusCode.OK, deactivateResp.StatusCode);
        var deactivated = await deactivateResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.False(deactivated!.IsActive);

        // 2. Sai da listagem por defeito
        var defaultList = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners", JsonOptions);
        Assert.DoesNotContain(defaultList!.Items, p => p.Id == created.Id);

        // 3. Aparece com IncludeInactive=true
        var inactiveList = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?includeInactive=true", JsonOptions);
        Assert.Contains(inactiveList!.Items, p => p.Id == created.Id);

        // 4. Continua acessível pelo Id
        var getByIdResp = await _client.GetAsync($"/api/partners/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResp.StatusCode);
        var fetchedById = await getByIdResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.False(fetchedById!.IsActive);

        // 5. Reativar devolve-a à listagem habitual
        var reactivateDto = new PartnerUpdateDto
        {
            Name = deactivated.Name,
            TaxNumber = deactivated.TaxNumber,
            Type = deactivated.Type,
            Email = deactivated.Email,
            CreditLimit = deactivated.CreditLimit,
            IsActive = true,
            Version = deactivated.Version
        };
        var reactivateResp = await _client.PutAsJsonAsync($"/api/partners/{created.Id}", reactivateDto);
        Assert.Equal(HttpStatusCode.OK, reactivateResp.StatusCode);

        var defaultListAgain = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners", JsonOptions);
        Assert.Contains(defaultListAgain!.Items, p => p.Id == created.Id);
    }

    /// <summary>
    /// Cenário 4:
    /// Atualizar uma entidade mantendo o próprio NIF é permitido.
    /// Mudar para o NIF de outra entidade devolve 409 e mantém os dados anteriores.
    /// </summary>
    [Fact]
    public async Task Scenario4_UpdateKeepingSameNifIsAllowed_ChangingToAnotherNifReturns409()
    {
        var partnerA = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Parceiro Alpha",
            TaxNumber = "005.111.222",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 1000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        var partnerB = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Parceiro Beta",
            TaxNumber = "005.333.444",
            Type = PartnerType.SUPPLIER,
            CreditLimit = 2000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // 1. Atualizar Partner A mantendo seu próprio NIF (005111222) deve ser permitido
        var updateSelfNifDto = new PartnerUpdateDto
        {
            Name = "Parceiro Alpha Renomeado",
            TaxNumber = "005.111.222",
            Type = partnerA!.Type,
            Email = partnerA.Email,
            CreditLimit = 1500m,
            IsActive = true,
            Version = partnerA.Version
        };
        var updateSelfResp = await _client.PutAsJsonAsync($"/api/partners/{partnerA.Id}", updateSelfNifDto);
        Assert.Equal(HttpStatusCode.OK, updateSelfResp.StatusCode);
        var updatedA = await updateSelfResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.Equal("Parceiro Alpha Renomeado", updatedA!.Name);

        // 2. Mudar para o NIF de Partner B (005333444) devolve 409
        var changeToOtherNifDto = new PartnerUpdateDto
        {
            Name = "Tentativa de Roubo de NIF",
            TaxNumber = "005.333.444",
            Type = updatedA.Type,
            Email = updatedA.Email,
            CreditLimit = 9999m,
            IsActive = true,
            Version = updatedA.Version
        };
        var conflictResp = await _client.PutAsJsonAsync($"/api/partners/{partnerA.Id}", changeToOtherNifDto);
        Assert.Equal(HttpStatusCode.Conflict, conflictResp.StatusCode);

        // 3. Verifica que os dados anteriores foram mantidos
        var verifyResp = await _client.GetFromJsonAsync<PartnerResponseDto>($"/api/partners/{partnerA.Id}", JsonOptions);
        Assert.Equal("Parceiro Alpha Renomeado", verifyResp!.Name);
        Assert.Equal("005111222", verifyResp.TaxNumber);
        Assert.Equal(1500m, verifyResp.CreditLimit);
    }

    /// <summary>
    /// Cenário 5:
    /// Dois pedidos simultâneos criam o mesmo NIF: um devolve 201, outro 409 e existe um único registo.
    /// Duas atualizações com a mesma Version: uma devolve 200 e outra 409. Verifique com conexões/contextos independentes sobre SQL Server.
    /// </summary>
    [Fact]
    public async Task Scenario5_ConcurrentCreationAndOptimisticConcurrency_OnSqlServer()
    {
        var nif = "006.123.456";
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var createDto1 = new PartnerCreateDto
        {
            Name = "Concorrente 1",
            TaxNumber = nif,
            Type = PartnerType.CUSTOMER,
            CreditLimit = 1000m
        };
        var createDto2 = new PartnerCreateDto
        {
            Name = "Concorrente 2",
            TaxNumber = nif,
            Type = PartnerType.SUPPLIER,
            CreditLimit = 2000m
        };

        // Dois pedidos simultâneos criando o mesmo NIF
        var task1 = client1.PostAsJsonAsync("/api/partners", createDto1);
        var task2 = client2.PostAsJsonAsync("/api/partners", createDto2);
        var responses = await Task.WhenAll(task1, task2);

        var statuses = responses.Select(r => r.StatusCode).ToList();
        Assert.Contains(HttpStatusCode.Created, statuses);
        Assert.Contains(HttpStatusCode.Conflict, statuses);

        // Buscar a entidade criada
        var successResponse = responses.First(r => r.StatusCode == HttpStatusCode.Created);
        var created = await successResponse.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.NotNull(created);

        // Testar concorrência otimista com a mesma Version
        var versionToken = created.Version;

        var update1 = new PartnerUpdateDto
        {
            Name = "Atualização 1",
            TaxNumber = created.TaxNumber,
            Type = created.Type,
            Email = created.Email,
            CreditLimit = 3000m,
            IsActive = true,
            Version = versionToken
        };
        var update2 = new PartnerUpdateDto
        {
            Name = "Atualização 2",
            TaxNumber = created.TaxNumber,
            Type = created.Type,
            Email = created.Email,
            CreditLimit = 4000m,
            IsActive = true,
            Version = versionToken
        };

        // Disparar ambas com a mesma Version
        var upTask1 = client1.PutAsJsonAsync($"/api/partners/{created.Id}", update1);
        var upTask2 = client2.PutAsJsonAsync($"/api/partners/{created.Id}", update2);
        var updateResponses = await Task.WhenAll(upTask1, upTask2);

        var updateStatuses = updateResponses.Select(r => r.StatusCode).ToList();
        Assert.Contains(HttpStatusCode.OK, updateStatuses);
        Assert.Contains(HttpStatusCode.Conflict, updateStatuses);
    }

    /// <summary>
    /// Cenário 6:
    /// CreditLimit=10,999 e NIF com letras devolvem 400.
    /// Teste também Page=0, Type inválido e Version malformada, sem alterações na base de dados.
    /// </summary>
    [Fact]
    public async Task Scenario6_InvalidRequestsReturn400_WithoutChangesToDatabase()
    {
        // 1. CreditLimit com 3 casas decimais (10.999) devolve 400
        var excessPrecisionDto = new PartnerCreateDto
        {
            Name = "Teste Precisao",
            TaxNumber = "007.111.222",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 10.999m
        };
        var resp1 = await _client.PostAsJsonAsync("/api/partners", excessPrecisionDto);
        Assert.Equal(HttpStatusCode.BadRequest, resp1.StatusCode);

        // 2. NIF com letras devolve 400
        var nifWithLettersDto = new PartnerCreateDto
        {
            Name = "Teste Letras",
            TaxNumber = "007.ABC.222",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 100m
        };
        var resp2 = await _client.PostAsJsonAsync("/api/partners", nifWithLettersDto);
        Assert.Equal(HttpStatusCode.BadRequest, resp2.StatusCode);

        // 3. Page = 0 devolve 400
        var resp3 = await _client.GetAsync("/api/partners?page=0");
        Assert.Equal(HttpStatusCode.BadRequest, resp3.StatusCode);

        // 4. Type inválido devolve 400
        var resp4 = await _client.GetAsync("/api/partners?type=INVALID_TYPE");
        Assert.Equal(HttpStatusCode.BadRequest, resp4.StatusCode);

        // 5. Version malformada no PUT devolve 400
        var partner = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Teste Version",
            TaxNumber = "007.333.444",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 500m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        var invalidVersionDto = new PartnerUpdateDto
        {
            Name = partner!.Name,
            TaxNumber = partner.TaxNumber,
            Type = partner.Type,
            Email = partner.Email,
            CreditLimit = partner.CreditLimit,
            IsActive = true,
            Version = "malformed-version-string!!!"
        };
        var resp5 = await _client.PutAsJsonAsync($"/api/partners/{partner.Id}", invalidVersionDto);
        Assert.Equal(HttpStatusCode.BadRequest, resp5.StatusCode);

        // Verificar que o parceiro permanece inalterado na base de dados
        var verify = await _client.GetFromJsonAsync<PartnerResponseDto>($"/api/partners/{partner.Id}", JsonOptions);
        Assert.Equal(500m, verify!.CreditLimit);
        Assert.Equal("Teste Version", verify.Name);
    }

    /// <summary>
    /// Cenário 7:
    /// Demonstre filtros combinados, TotalCount antes da paginação e desempate por Id para nomes iguais.
    /// Search="Ana" não pode fazer corresponder todas as entidades pelo ramo do NIF.
    /// </summary>
    [Fact]
    public async Task Scenario7_CombinedFilters_TotalCountBeforePagination_TieBreakById_AndSearchAccuracy()
    {
        // Criar duas entidades com o mesmo nome exato "Ana Pereira", mas NIFs diferentes
        var p1 = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Ana Pereira",
            TaxNumber = "008.111.111",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 1000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        var p2 = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Ana Pereira",
            TaxNumber = "008.222.222",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 2000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // Criar outra entidade com nome que NÃO tem "Ana"
        var p3 = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Carlos Mendes",
            TaxNumber = "008.333.333",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 3000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // 1. Search="Ana" deve retornar p1 e p2, mas NÃO p3 (Search="Ana" não pode corresponder todas as entidades pelo NIF)
        var searchAna = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ana", JsonOptions);
        Assert.NotNull(searchAna);
        Assert.Contains(searchAna.Items, p => p.Id == p1!.Id);
        Assert.Contains(searchAna.Items, p => p.Id == p2!.Id);
        Assert.DoesNotContain(searchAna.Items, p => p.Id == p3!.Id);

        // 2. Desempate por Id para nomes iguais
        // Quando os nomes são iguais ("Ana Pereira"), a ordenação executada no SQL Server usa a ordem de uniqueidentifier (SqlGuid)
        var anaItems = searchAna.Items.Where(p => p.Name == "Ana Pereira").ToList();
        var expectedOrder = anaItems
            .OrderBy(p => p.Name)
            .ThenBy(p => new System.Data.SqlTypes.SqlGuid(p.Id))
            .Select(p => p.Id)
            .ToList();
        var actualOrder = anaItems.Select(p => p.Id).ToList();
        Assert.Equal(expectedOrder, actualOrder);

        // 3. TotalCount antes da paginação:
        // Ao pedir Page=1 e PageSize=1, TotalCount deve ser >= 2, e Items deve conter apenas 1 item
        var pagedResp = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ana&page=1&pageSize=1", JsonOptions);
        Assert.NotNull(pagedResp);
        Assert.Single(pagedResp.Items);
        Assert.True(pagedResp.TotalCount >= 2);

        // 4. Uma página além dos resultados devolve Items vazio e TotalCount correto
        var pageBeyond = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ana&page=999&pageSize=10", JsonOptions);
        Assert.NotNull(pageBeyond);
        Assert.Empty(pageBeyond.Items);
        Assert.True(pageBeyond.TotalCount >= 2);
    }
}
