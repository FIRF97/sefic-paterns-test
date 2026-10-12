using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sefic.Partners.Core;
using Sefic.Partners.Data;
using Sefic.Partners.Services.DTO;
using Sefic.Partners.Services.Helper;
using Sefic.Partners.Services.Interface;
using Sefic.Partners.Shared.Context;
using Sefic.Partners.Shared.Enums;
using Sefic.Partners.Shared.Helper;
using Sefic.Partners.Shared.Model;
using Sefic.Partners.Shared.Models;

namespace Sefic.Partners.Services;

public class PartnerService : IPartnerService
{
    private readonly PartnersDbContext _context;
    private readonly IUnitOfWork _uow;

    public PartnerService(PartnersDbContext context, IUnitOfWork? uow = null)
    {
        _context = context;
        _uow = uow ?? new UnitOfWork(context);
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
        return GetAllAsync(filter, null);
    }

    public Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PartnerFiltro filtro)
    {
        var pageFilter = new PageFilter
        {
            Page = filtro.Page,
            PageSize = filtro.PageSize,
            Search = filtro.Search,
            Type = filtro.Type,
            IncludeInactive = filtro.IncludeInactive
        };

        return GetAllAsync(pageFilter, filtro);
    }

    public async Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PageFilter filter, PartnerFiltro? filtro = null)
    {
        if (!PartnerValidator.ValidatePagination(filter.Page, filter.PageSize, out var paginationError))
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest(paginationError!);

        var typeToValidate = filtro?.Type ?? filter.Type;
        if (!string.IsNullOrWhiteSpace(typeToValidate) && !Enum.TryParse<PartnerType>(typeToValidate.Trim(), true, out _))
        {
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest("Type inválido no filtro. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");
        }

        // Determinar ordenação dinâmica aplicada diretamente no banco
        string? orderByClause = null;
        bool isDescending = (filtro?.OrderByDescending ?? false) ||
            string.Equals(filtro?.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(filtro?.SortDirection, "decrescente", StringComparison.OrdinalIgnoreCase);

        bool isAscending = (filtro?.OrderByDescending == false) ||
            string.Equals(filtro?.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(filtro?.SortDirection, "crescente", StringComparison.OrdinalIgnoreCase);

        bool sortByCreatedAt = isDescending || isAscending ||
            string.Equals(filtro?.OrderBy, "CreatedAt", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(filtro?.OrderBy, "DataRegisto", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(filter.OrderBy, "CreatedAt", StringComparison.OrdinalIgnoreCase);

        if (sortByCreatedAt)
        {
            orderByClause = isDescending ? "CreatedAt desc, Id asc" : "CreatedAt asc, Id asc";
        }
        else if (!string.IsNullOrWhiteSpace(filter.OrderBy))
        {
            orderByClause = filter.OrderBy;
        }
        else
        {
            // Padrão Fundcube / Especificação: ordenação determinística por Name e depois Id
            orderByClause = "Name asc, Id asc";
        }

        filtro ??= new PartnerFiltro();

        if (string.IsNullOrWhiteSpace(filtro.Search) && !string.IsNullOrWhiteSpace(filter.Search))
            filtro.Search = filter.Search;

        if (string.IsNullOrWhiteSpace(filtro.Type) && !string.IsNullOrWhiteSpace(filter.Type))
            filtro.Type = filter.Type;

        if (!filtro.IncludeInactive && filter.IncludeInactive)
            filtro.IncludeInactive = filter.IncludeInactive;

        // 2. Consulta executada diretamente no repositório aplicando o filtro via QueryBuilder e paginação no SQL Server (padrão ms-core-fundcube)
        var pagedEntities = await _uow.PartnerRepository.GetAllAsync(
            filter,
            orderBy: orderByClause,
            filtrosPropriedades: filtro.BuildFilter()
        );

        var dtos = pagedEntities.Items.Select(PartnerResponseDto.FromEntity).ToList();
        var pagedResult = new PagedResult<PartnerResponseDto>(dtos, pagedEntities.TotalCount, pagedEntities.Page, pagedEntities.PageSize);
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
