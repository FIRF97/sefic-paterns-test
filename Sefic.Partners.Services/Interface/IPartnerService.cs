using Sefic.Partners.Services.DTO;
using Sefic.Partners.Services.Helper;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Services.Interface;

public interface IPartnerService
{
    Task<ServiceResult<PartnerResponseDto>> CreateAsync(PartnerCreateDto dto);
    Task<ServiceResult<PartnerResponseDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PageFilter filter, PartnerFiltro? filtro = null);
    Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PartnerFiltro filtro);
    Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PageFilter filter);
    Task<ServiceResult<PartnerResponseDto>> UpdateAsync(Guid id, PartnerUpdateDto dto);
}
