using Microsoft.AspNetCore.Mvc;
using Sefic.Partners.Api.Helper;
using Sefic.Partners.Services.DTO;
using Sefic.Partners.Services.Interface;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Api.Controllers;

[Route("api/[controller]")]
public class PartnersController : BaseController
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    /// <summary>
    /// POST /api/partners — Criar entidade (cliente, fornecedor ou ambos)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PartnerResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PartnerCreateDto dto)
    {
        var result = await _partnerService.CreateAsync(dto);
        if (!result.IsSuccess)
            return ProcessResult(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// GET /api/partners/{id} — Devolver os dados e Version (mesmo se inativa)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PartnerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _partnerService.GetByIdAsync(id);
        return ProcessResult(result);
    }

    /// <summary>
    /// GET /api/partners — Listar com filtros, contagem total e paginação no SQL Server
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PartnerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new PageFilter
        {
            Search = search,
            Type = type,
            IncludeInactive = includeInactive,
            Page = page,
            PageSize = pageSize
        };

        var result = await _partnerService.GetAllAsync(filter);
        return ProcessResult(result);
    }

    /// <summary>
    /// PUT /api/partners/{id} — Atualizar entidade com controle de concorrência otimista via Version
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PartnerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] PartnerUpdateDto dto)
    {
        var result = await _partnerService.UpdateAsync(id, dto);
        return ProcessResult(result);
    }
}
