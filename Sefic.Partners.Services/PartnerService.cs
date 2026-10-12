using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Services.DTO;
using Sefic.Partners.Services.Helper;
using Sefic.Partners.Services.Interface;
using Sefic.Partners.Shared.Context;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Model;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Services;

public class PartnerService : IPartnerService
{
    private readonly PartnersDbContext _context;

    public PartnerService(PartnersDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<PartnerResponseDto>> CreateAsync(PartnerCreateDto dto)
    {
        if (!PartnerValidator.ValidateName(dto.Name, out var trimmedName, out var nameError))
            return ServiceResult<PartnerResponseDto>.BadRequest(nameError!);

        if (!TaxNumberHelper.Validate(dto.TaxNumber, out var normalizedNif, out var nifError))
            return ServiceResult<PartnerResponseDto>.BadRequest(nifError!);

        if (!PartnerValidator.ValidateEmail(dto.Email, out var normalizedEmail, out var emailError))
            return ServiceResult<PartnerResponseDto>.BadRequest(emailError!);

        if (!PartnerValidator.ValidateCreditLimit(dto.CreditLimit, out var creditError))
            return ServiceResult<PartnerResponseDto>.BadRequest(creditError!);

        if (!Enum.IsDefined(typeof(PartnerType), dto.Type))
            return ServiceResult<PartnerResponseDto>.BadRequest("Type inválido. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");

        // TaxNumber é único em todo o cadastro, incluindo inativos
        var exists = await _context.Partners.AsNoTracking().AnyAsync(p => p.TaxNumber == normalizedNif);
        if (exists)
            return ServiceResult<PartnerResponseDto>.Conflict("Já existe um registo com o NIF fornecido.");

        var entity = new Partner
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            TaxNumber = normalizedNif,
            Type = dto.Type,
            Email = normalizedEmail,
            CreditLimit = dto.CreditLimit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Partners.Add(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return ServiceResult<PartnerResponseDto>.Conflict("Já existe um registo com o NIF fornecido.");
        }

        return ServiceResult<PartnerResponseDto>.Created(PartnerResponseDto.FromEntity(entity));
    }

    public async Task<ServiceResult<PartnerResponseDto>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return ServiceResult<PartnerResponseDto>.BadRequest("Id inválido.");

        var entity = await _context.Partners.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null)
            return ServiceResult<PartnerResponseDto>.NotFound("Entidade com o Id fornecido não existe.");

        return ServiceResult<PartnerResponseDto>.Success(PartnerResponseDto.FromEntity(entity));
    }

    public async Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PageFilter filter)
    {
        if (!PartnerValidator.ValidatePagination(filter.Page, filter.PageSize, out var paginationError))
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest(paginationError!);

        PartnerType? filterType = null;
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            if (Enum.TryParse<PartnerType>(filter.Type.Trim(), true, out var parsedType))
            {
                filterType = parsedType;
            }
            else
            {
                return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest("Type inválido no filtro. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");
            }
        }

        IQueryable<Partner> query = _context.Partners.AsNoTracking();

        // IncludeInactive (false por defeito)
        if (!filter.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        // Filtro por Type:
        // CUSTOMER inclui CUSTOMER e BOTH
        // SUPPLIER inclui SUPPLIER e BOTH
        // BOTH inclui apenas BOTH
        if (filterType.HasValue)
        {
            query = filterType.Value switch
            {
                PartnerType.CUSTOMER => query.Where(p => p.Type == PartnerType.CUSTOMER || p.Type == PartnerType.BOTH),
                PartnerType.SUPPLIER => query.Where(p => p.Type == PartnerType.SUPPLIER || p.Type == PartnerType.BOTH),
                PartnerType.BOTH => query.Where(p => p.Type == PartnerType.BOTH),
                _ => query
            };
        }

        // Filtro Search:
        // Ocorrência parcial no nome (sem case sensitivity) ou no NIF normalizado.
        // Não use uma procura normalizada vazia para fazer todos os NIF corresponderem.
        // Search="Ana" não pode corresponder todas as entidades pelo ramo do NIF.
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTrimmed = filter.Search.Trim();
            var searchNifNormalized = TaxNumberHelper.Normalize(searchTrimmed);

            if (searchNifNormalized.Length > 0 && searchNifNormalized.Any(char.IsDigit))
            {
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%") || p.TaxNumber.Contains(searchNifNormalized));
            }
            else
            {
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTrimmed}%"));
            }
        }

        // Ordene por Name e depois Id, conte antes de paginar e execute filtros/paginação no SQL Server
        query = query.OrderBy(p => p.Name).ThenBy(p => p.Id);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(PartnerResponseDto.FromEntity).ToList();

        var pagedResult = new PagedResult<PartnerResponseDto>(dtos, totalCount, filter.Page, filter.PageSize);
        return ServiceResult<PagedResult<PartnerResponseDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<PartnerResponseDto>> UpdateAsync(Guid id, PartnerUpdateDto dto)
    {
        if (id == Guid.Empty)
            return ServiceResult<PartnerResponseDto>.BadRequest("Id inválido.");

        if (!PartnerValidator.ValidateName(dto.Name, out var trimmedName, out var nameError))
            return ServiceResult<PartnerResponseDto>.BadRequest(nameError!);

        if (!TaxNumberHelper.Validate(dto.TaxNumber, out var normalizedNif, out var nifError))
            return ServiceResult<PartnerResponseDto>.BadRequest(nifError!);

        if (!PartnerValidator.ValidateEmail(dto.Email, out var normalizedEmail, out var emailError))
            return ServiceResult<PartnerResponseDto>.BadRequest(emailError!);

        if (!PartnerValidator.ValidateCreditLimit(dto.CreditLimit, out var creditError))
            return ServiceResult<PartnerResponseDto>.BadRequest(creditError!);

        if (!Enum.IsDefined(typeof(PartnerType), dto.Type))
            return ServiceResult<PartnerResponseDto>.BadRequest("Type inválido. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");

        if (!PartnerValidator.ValidateVersion(dto.Version, out var clientVersionBytes, out var versionError))
            return ServiceResult<PartnerResponseDto>.BadRequest(versionError!);

        var entity = await _context.Partners.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null)
            return ServiceResult<PartnerResponseDto>.NotFound("Entidade com o Id fornecido não existe.");

        // Atualizar mantendo o próprio NIF é permitido. Mudar para o NIF de outra entidade devolve 409 e mantém os dados anteriores.
        var otherWithNif = await _context.Partners.AsNoTracking().AnyAsync(p => p.TaxNumber == normalizedNif && p.Id != id);
        if (otherWithNif)
            return ServiceResult<PartnerResponseDto>.Conflict("Já existe outra entidade com este NIF.");

        // Definir a OriginalValue para controle de concorrência otimista via rowversion
        _context.Entry(entity).Property(p => p.Version).OriginalValue = clientVersionBytes;

        entity.Name = trimmedName;
        entity.TaxNumber = normalizedNif;
        entity.Type = dto.Type;
        entity.Email = normalizedEmail;
        entity.CreditLimit = dto.CreditLimit;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        // CreatedAt e Id não são editáveis pelo cliente

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<PartnerResponseDto>.Conflict("Versão desatualizada. A entidade foi alterada por outro pedido concorrente.");
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return ServiceResult<PartnerResponseDto>.Conflict("Já existe uma entidade com este NIF.");
        }

        return ServiceResult<PartnerResponseDto>.Success(PartnerResponseDto.FromEntity(entity));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            return sqlEx.Number is 2601 or 2627;
        }

        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("Unique", StringComparison.OrdinalIgnoreCase);
    }
}
