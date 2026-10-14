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
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PartnerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _partnerService.GetByIdAsync(id);
        return ProcessResult(result);
    }

    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PartnerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] PartnerFilter filter)
    {
        if (Request.Query.TryGetValue("page", out var pageVal) && int.TryParse(pageVal, out var p))
        {
            filter.PageNumber = p;
        }

        var route = Request.Path.Value ?? "/api/partners";
        var result = await _partnerService.GetAllAsync(filter, route);
        return ProcessResult(result);
    }
    
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
