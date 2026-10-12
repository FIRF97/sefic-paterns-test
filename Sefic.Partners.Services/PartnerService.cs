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

    public Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PageFilter filter)
    {
        if (filter is PartnerFiltro pf)
            return GetAllAsync(pf);

        return GetAllAsync(new PartnerFiltro
        {
            Page = filter.Page,
            PageSize = filter.PageSize,
            Search = filter.Search,
            Type = filter.Type,
            IncludeInactive = filter.IncludeInactive
        });
    }

    public async Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PartnerFiltro filtro)
    {
        if (!PartnerValidator.ValidatePagination(filtro.Page, filtro.PageSize, out var paginationError))
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest(paginationError!);

        PartnerType? filterType = null;
        if (!string.IsNullOrWhiteSpace(filtro.Type))
        {
            if (Enum.TryParse<PartnerType>(filtro.Type.Trim(), true, out var parsedType))
            {
                filterType = parsedType;
            }
            else
            {
                return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest("Type inválido no filtro. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");
            }
        }

        IQueryable<Partner> query = _context.Partners.AsNoTracking();

        // 1. Filtro por Id
        if (filtro.Id.HasValue && filtro.Id.Value != Guid.Empty)
        {
            query = query.Where(p => p.Id == filtro.Id.Value);
        }

        // 2. Filtro IsActive / IncludeInactive
        if (filtro.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filtro.IsActive.Value);
        }
        else if (!filtro.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        // 3. Filtro por Type:
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

        // 4. Filtro específico por Nome
        if (!string.IsNullOrWhiteSpace(filtro.Name))
        {
            var nameTrimmed = filtro.Name.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{nameTrimmed}%"));
        }

        // 5. Filtro específico por NIF (TaxNumber)
        if (!string.IsNullOrWhiteSpace(filtro.TaxNumber))
        {
            var nifNormalized = TaxNumberHelper.Normalize(filtro.TaxNumber);
            if (!string.IsNullOrWhiteSpace(nifNormalized))
            {
                query = query.Where(p => p.TaxNumber.Contains(nifNormalized));
            }
        }

        // 6. Filtro específico por Email
        if (!string.IsNullOrWhiteSpace(filtro.Email))
        {
            var emailTrimmed = filtro.Email.Trim();
            query = query.Where(p => p.Email != null && EF.Functions.Like(p.Email, $"%{emailTrimmed}%"));
        }

        // 7. Filtro por CreditLimit (exato ou intervalo)
        if (filtro.CreditLimit.HasValue)
        {
            query = query.Where(p => p.CreditLimit == filtro.CreditLimit.Value);
        }
        if (filtro.MinCreditLimit.HasValue)
        {
            query = query.Where(p => p.CreditLimit >= filtro.MinCreditLimit.Value);
        }
        if (filtro.MaxCreditLimit.HasValue)
        {
            query = query.Where(p => p.CreditLimit <= filtro.MaxCreditLimit.Value);
        }

        // 8. Filtro por Data de Registo (CreatedAt)
        if (filtro.CreatedAt.HasValue)
        {
            var targetDate = filtro.CreatedAt.Value.Date;
            var nextDay = targetDate.AddDays(1);
            query = query.Where(p => p.CreatedAt >= targetDate && p.CreatedAt < nextDay);
        }
        if (filtro.CreatedAtFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filtro.CreatedAtFrom.Value);
        }
        if (filtro.CreatedAtTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filtro.CreatedAtTo.Value);
        }

        // 9. Filtro por Data de Atualização (UpdatedAt)
        if (filtro.UpdatedAt.HasValue)
        {
            var targetDate = filtro.UpdatedAt.Value.Date;
            var nextDay = targetDate.AddDays(1);
            query = query.Where(p => p.UpdatedAt >= targetDate && p.UpdatedAt < nextDay);
        }
        if (filtro.UpdatedAtFrom.HasValue)
        {
            query = query.Where(p => p.UpdatedAt >= filtro.UpdatedAtFrom.Value);
        }
        if (filtro.UpdatedAtTo.HasValue)
        {
            query = query.Where(p => p.UpdatedAt <= filtro.UpdatedAtTo.Value);
        }

        // 10. Filtro Search (busca global por nome ou NIF):
        if (!string.IsNullOrWhiteSpace(filtro.Search))
        {
            var searchTrimmed = filtro.Search.Trim();
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

        // 11. Ordenação mediante a data de registo (CreatedAt) ou padrão por Name
        bool isDescending = filtro.OrderByDescending ??
            (string.Equals(filtro.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(filtro.SortDirection, "decrescente", StringComparison.OrdinalIgnoreCase));

        bool isAscending = (filtro.OrderByDescending == false) ||
            (string.Equals(filtro.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(filtro.SortDirection, "crescente", StringComparison.OrdinalIgnoreCase));

        bool sortByCreatedAt = isDescending || isAscending ||
            string.Equals(filtro.OrderBy, "CreatedAt", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(filtro.OrderBy, "DataRegisto", StringComparison.OrdinalIgnoreCase);

        if (sortByCreatedAt)
        {
            query = isDescending
                ? query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
                : query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id);
        }
        else
        {
            // Ordenação padrão da especificação original: Name e depois Id
            query = query.OrderBy(p => p.Name).ThenBy(p => p.Id);
        }

        var totalCount = await query.CountAsync();

        // Projeção direta no banco de dados trazendo estritamente as colunas necessárias
        var items = await query
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.TaxNumber,
                p.Type,
                p.Email,
                p.CreditLimit,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt,
                p.Version
            })
            .ToListAsync();

        var dtos = items.Select(p => new PartnerResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            TaxNumber = p.TaxNumber,
            Type = p.Type,
            Email = p.Email,
            CreditLimit = p.CreditLimit,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Version = p.Version != null && p.Version.Length > 0 ? Convert.ToBase64String(p.Version) : string.Empty
        }).ToList();

        var pagedResult = new PagedResult<PartnerResponseDto>(dtos, totalCount, filtro.Page, filtro.PageSize);
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
