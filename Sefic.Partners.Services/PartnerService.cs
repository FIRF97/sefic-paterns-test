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
        if (filter is PartnerFilter pf)
            return GetAllAsync(pf);

        var partnerFilter = new PartnerFilter(filter.PageNumber, filter.PageSize)
        {
            OrderBy = filter.OrderBy
        };

        return GetAllAsync(partnerFilter);
    }

    public async Task<ServiceResult<PagedResult<PartnerResponseDto>>> GetAllAsync(PartnerFilter filter, string route = "/api/partners")
    {
        if (!PartnerValidator.ValidatePagination(filter.PageNumber, filter.PageSize, out var paginationError))
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest(paginationError!);

        if (!string.IsNullOrWhiteSpace(filter.Type) && !Enum.TryParse<PartnerType>(filter.Type.Trim(), true, out _))
        {
            return ServiceResult<PagedResult<PartnerResponseDto>>.BadRequest("Type inválido no filtro. Valores aceites: CUSTOMER, SUPPLIER, BOTH.");
        }
        
        var orderByClause = !string.IsNullOrWhiteSpace(filter.OrderBy)
            ? filter.OrderBy
            : "CreatedAt desc, Id asc";

        var combinedFilter = filter.BuildFilter();

        // Consulta executada diretamente no repositório aplicando o filtro via QueryBuilder e paginação no SQL Server
        var pagedEntities = await _uow.PartnerRepository.GetAllAsync(
            filter,
            orderBy: orderByClause,
            filtrosPropriedades: combinedFilter,
            route: route,
            mensagem: "Partner"
        );

        var dtos = pagedEntities.Data.Select(PartnerResponseDto.FromEntity).ToList();
        var pagedResult = new PagedResult<PartnerResponseDto>(
            dtos,
            pagedEntities.TotalRecords,
            pagedEntities.PageNumber,
            pagedEntities.PageSize,
            pagedEntities.TotalPages,
            pagedEntities.NextPage,
            pagedEntities.PreviousPage,
            pagedEntities.FirstPage,
            pagedEntities.LastPage,
            pagedEntities.Retorno.Mensagem);

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

    public async Task<ServiceResult<int>> SeedAsync()
    {
        var initialPartners = new List<Partner>
        {
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Ana Ferreira",
                TaxNumber = "001234567",
                Type = PartnerType.CUSTOMER,
                Email = "ana.ferreira@example.com",
                CreditLimit = 5000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Tech Supplies Lda",
                TaxNumber = "502123456",
                Type = PartnerType.SUPPLIER,
                Email = "contacto@techsupplies.com",
                CreditLimit = 50000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Global Logística & Distribuição",
                TaxNumber = "509988776",
                Type = PartnerType.BOTH,
                Email = "geral@globallog.com",
                CreditLimit = 120000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Papelaria Central",
                TaxNumber = "504112233",
                Type = PartnerType.SUPPLIER,
                Email = "vendas@papelariacentral.pt",
                CreditLimit = 7500.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Partner
            {
                Id = Guid.NewGuid(),
                Name = "Consultoria & Serviços Alpha",
                TaxNumber = "508765432",
                Type = PartnerType.CUSTOMER,
                Email = "info@consultoriaalpha.ao",
                CreditLimit = 25000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var insertedCount = 0;
        foreach (var partner in initialPartners)
        {
            var existing = await _context.Partners.FirstOrDefaultAsync(p => p.TaxNumber == partner.TaxNumber);
            if (existing == null)
            {
                await _uow.PartnerRepository.AddAsync(partner);
                insertedCount++;
            }
            else if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                insertedCount++;
            }
        }

        if (insertedCount > 0)
        {
            await _uow.CommitAsync();
        }

        return ServiceResult<int>.Success(
            insertedCount,
            $"Seed executado com sucesso. {insertedCount} novo(s) parceiro(s) adicionado(s). Total no cadastro: {await _uow.PartnerRepository.CountAsync()}.");
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
