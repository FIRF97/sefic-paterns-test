using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Sefic.Partners.Services.DTO;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Models;
using Xunit;

namespace Sefic.Partners.Tests.Integration;

public class PartnerFiltroTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PartnerFiltroTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PartnerFiltro_FilterById_ShouldReturnOnlyMatchingPartner()
    {
        // 1. Criar parceiro
        var createResp = await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Empresa Filtro Id",
            TaxNumber = "010.111.222",
            Type = PartnerType.CUSTOMER,
            Email = "filtro.id@example.com",
            CreditLimit = 3500m
        });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);
        Assert.NotNull(created);

        // 2. Filtrar por Id
        var filterResp = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>($"/api/partners?id={created.Id}", JsonOptions);
        Assert.NotNull(filterResp);
        Assert.Equal(1, filterResp.TotalCount);
        Assert.Single(filterResp.Items);
        Assert.Equal(created.Id, filterResp.Items.First().Id);
    }

    [Fact]
    public async Task PartnerFiltro_FilterByAllEntityFields_ShouldFilterAccurately()
    {
        // Criar parceiros de teste com características distintas
        var p1 = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Tech Solutions Ltd",
            TaxNumber = "011.222.333",
            Type = PartnerType.SUPPLIER,
            Email = "contact@techsolutions.com",
            CreditLimit = 8000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        var p2 = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Tech Retail Store",
            TaxNumber = "011.444.555",
            Type = PartnerType.CUSTOMER,
            Email = "support@techretail.com",
            CreditLimit = 12000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // A. Filtro por Name parcial
        var byName = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?name=Retail", JsonOptions);
        Assert.NotNull(byName);
        Assert.Contains(byName.Items, x => x.Id == p2!.Id);
        Assert.DoesNotContain(byName.Items, x => x.Id == p1!.Id);

        // B. Filtro por TaxNumber
        var byNif = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?taxNumber=011222333", JsonOptions);
        Assert.NotNull(byNif);
        Assert.Contains(byNif.Items, x => x.Id == p1!.Id);

        // C. Filtro por Email
        var byEmail = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?email=techsolutions", JsonOptions);
        Assert.NotNull(byEmail);
        Assert.Contains(byEmail.Items, x => x.Id == p1!.Id);
        Assert.DoesNotContain(byEmail.Items, x => x.Id == p2!.Id);

        // D. Filtro por CreditLimit exato e faixa (Min/Max)
        var byExactLimit = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?creditLimit=8000", JsonOptions);
        Assert.NotNull(byExactLimit);
        Assert.Contains(byExactLimit.Items, x => x.Id == p1!.Id);

        var byLimitRange = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?minCreditLimit=10000&maxCreditLimit=15000", JsonOptions);
        Assert.NotNull(byLimitRange);
        Assert.Contains(byLimitRange.Items, x => x.Id == p2!.Id);
        Assert.DoesNotContain(byLimitRange.Items, x => x.Id == p1!.Id);

        // E. Filtro por IsActive
        var byActive = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?isActive=true", JsonOptions);
        Assert.NotNull(byActive);
        Assert.All(byActive.Items, x => Assert.True(x.IsActive));
    }

    [Fact]
    public async Task PartnerFiltro_OrderByCreatedDate_AscendingAndDescending_ShouldSortCorrectly()
    {
        // Criar 3 parceiros sequencialmente para garantir tempos distintos de registo
        var pA = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Ordem Primeiro",
            TaxNumber = "012.111.111",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 1000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        await Task.Delay(50); // Garantir diferença na data/hora de registo

        var pB = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Ordem Segundo",
            TaxNumber = "012.222.222",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 2000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        await Task.Delay(50);

        var pC = await (await _client.PostAsJsonAsync("/api/partners", new PartnerCreateDto
        {
            Name = "Ordem Terceiro",
            TaxNumber = "012.333.333",
            Type = PartnerType.CUSTOMER,
            CreditLimit = 3000m
        })).Content.ReadFromJsonAsync<PartnerResponseDto>(JsonOptions);

        // 1. Ordenação Decrescente por Data de Registo (mais recente primeiro: pC, depois pB, depois pA)
        var descResp = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ordem&sortDirection=desc", JsonOptions);
        Assert.NotNull(descResp);
        var descItems = descResp.Items.Where(x => x.Name.StartsWith("Ordem")).ToList();
        Assert.True(descItems.Count >= 3);
        var idxC = descItems.FindIndex(x => x.Id == pC!.Id);
        var idxB = descItems.FindIndex(x => x.Id == pB!.Id);
        var idxA = descItems.FindIndex(x => x.Id == pA!.Id);
        Assert.True(idxC < idxB, "pC (mais recente) deve aparecer antes de pB na ordenação decrescente");
        Assert.True(idxB < idxA, "pB deve aparecer antes de pA na ordenação decrescente");

        // 2. Ordenação Crescente por Data de Registo (mais antigo primeiro: pA, depois pB, depois pC)
        var ascResp = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ordem&sortDirection=asc", JsonOptions);
        Assert.NotNull(ascResp);
        var ascItems = ascResp.Items.Where(x => x.Name.StartsWith("Ordem")).ToList();
        Assert.True(ascItems.Count >= 3);
        var idxAscA = ascItems.FindIndex(x => x.Id == pA!.Id);
        var idxAscB = ascItems.FindIndex(x => x.Id == pB!.Id);
        var idxAscC = ascItems.FindIndex(x => x.Id == pC!.Id);
        Assert.True(idxAscA < idxAscB, "pA (mais antigo) deve aparecer antes de pB na ordenação crescente");
        Assert.True(idxAscB < idxAscC, "pB deve aparecer antes de pC na ordenação crescente");

        // 3. Testar também via parâmetro booleano OrderByDescending=true
        var boolDescResp = await _client.GetFromJsonAsync<PagedResult<PartnerResponseDto>>("/api/partners?search=Ordem&orderByDescending=true", JsonOptions);
        Assert.NotNull(boolDescResp);
        var boolDescItems = boolDescResp.Items.Where(x => x.Name.StartsWith("Ordem")).ToList();
        Assert.True(boolDescItems.FindIndex(x => x.Id == pC!.Id) < boolDescItems.FindIndex(x => x.Id == pA!.Id));
    }
}
