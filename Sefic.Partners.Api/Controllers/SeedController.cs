using Microsoft.AspNetCore.Mvc;
using Sefic.Partners.Api.Helper;
using Sefic.Partners.Services.Interface;

namespace Sefic.Partners.Api.Controllers;

[Route("api/[controller]")]
public class SeedController : BaseController
{
    private readonly IPartnerService _partnerService;

    public SeedController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }
    
    [HttpPost("/api/partners/carregamento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CarregarPartners()
    {
        var result = await _partnerService.SeedAsync();
        return ProcessResult(result);
    }
}
