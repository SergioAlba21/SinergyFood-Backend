using ClosedXML.Excel;
using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Application.Features.Diners.DTOs;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InsumoPro.Infrastructure.Services;

public class DinerService : IDinerService
{
    private readonly ApplicationDbContext _context;

    private readonly IConfiguration _config;

    public DinerService(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // -------- Departments --------
    public async Task<List<DepartmentDto>> GetDepartmentsAsync(Guid companyId)
    {
        return await _context.Departments
            .Where(d => d.CompanyId == companyId)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                IsActive = d.IsActive
            }).ToListAsync();
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(Guid companyId, CreateDepartmentRequest request)
    {
        var entity = new Department
        {
            CompanyId = companyId,
            Name = request.Name,
            Code = request.Code,
            IsActive = true
        };
        _context.Departments.Add(entity);
        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            IsActive = entity.IsActive
        };
    }

    // -------- Meal Types --------
    public async Task<List<MealTypeDto>> GetMealTypesAsync(Guid companyId)
    {
        return await _context.MealTypes
            .Where(m => m.CompanyId == companyId)
            .Select(m => new MealTypeDto
            {
                Id = m.Id,
                Name = m.Name,
                Code = m.Code,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                MaxPerDay = m.MaxPerDay,
                IsActive = m.IsActive
            }).ToListAsync();
    }

    public async Task<MealTypeDto> CreateMealTypeAsync(Guid companyId, CreateMealTypeRequest request)
    {
        TimeSpan? start = null;
        TimeSpan? end = null;

        if (!string.IsNullOrWhiteSpace(request.StartTime) && TimeSpan.TryParse(request.StartTime, out var s))
            start = s;
        if (!string.IsNullOrWhiteSpace(request.EndTime) && TimeSpan.TryParse(request.EndTime, out var e))
            end = e;

        var entity = new MealType
        {
            CompanyId = companyId,
            Name = request.Name,
            Code = request.Code,
            StartTime = start,
            EndTime = end,
            MaxPerDay = request.MaxPerDay,
            IsActive = true
        };

        _context.MealTypes.Add(entity);
        await _context.SaveChangesAsync();

        return new MealTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            MaxPerDay = entity.MaxPerDay,
            IsActive = entity.IsActive
        };
    }

    // -------- Diners --------
    public async Task<List<DinerDto>> GetDinersAsync(Guid companyId)
    {
        return await _context.Diners
            .Where(d => d.CompanyId == companyId)
            .Include(d => d.Department)
            .Select(d => new DinerDto
            {
                Id = d.Id,
                EmployeeNumber = d.EmployeeNumber,
                FirstName = d.FirstName,
                LastName = d.LastName,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department != null ? d.Department.Name : null,
                Email = d.Email,
                Phone = d.Phone,
                AccessCode = d.AccessCode,
                IsActive = d.IsActive
            }).ToListAsync();
    }

    public async Task<DinerDto?> GetDinerByIdAsync(Guid companyId, Guid id)
    {
        var d = await _context.Diners
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (d == null) return null;

        return new DinerDto
        {
            Id = d.Id,
            EmployeeNumber = d.EmployeeNumber,
            FirstName = d.FirstName,
            LastName = d.LastName,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department?.Name,
            Email = d.Email,
            Phone = d.Phone,
            AccessCode = d.AccessCode,
            IsActive = d.IsActive
        };
    }

    public async Task<DinerDto> CreateDinerAsync(Guid companyId, CreateDinerRequest request)
    {
        var exists = await _context.Diners.AnyAsync(d =>
            d.CompanyId == companyId && d.EmployeeNumber == request.EmployeeNumber);

        if (exists)
            throw new Exception("Ya existe un comensal con ese número de empleado");

        var entity = new Diner
        {
            CompanyId = companyId,
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DepartmentId = request.DepartmentId,
            Email = request.Email,
            Phone = request.Phone,
            AccessCode = request.AccessCode,
            IsActive = true
        };

        _context.Diners.Add(entity);
        await _context.SaveChangesAsync();

        return (await GetDinerByIdAsync(companyId, entity.Id))!;
    }

    public async Task<DinerDto?> UpdateDinerAsync(Guid companyId, Guid id, UpdateDinerRequest request)
    {
        var entity = await _context.Diners
            .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

        if (entity == null) return null;

        entity.EmployeeNumber = request.EmployeeNumber;
        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.DepartmentId = request.DepartmentId;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.AccessCode = request.AccessCode;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return await GetDinerByIdAsync(companyId, entity.Id);
    }

    public async Task<bool> DeleteDinerAsync(Guid companyId, Guid id)
    {
        var entity = await _context.Diners
            .FirstOrDefaultAsync(d => d.Id == id && d.CompanyId == companyId);

        if (entity == null) return false;

        entity.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // -------- Validación y consumo --------
    public async Task<ValidateAccessResponse> ValidateAndRegisterAsync(
        Guid companyId, Guid userId, ValidateAccessRequest request)
    {
        var diner = await _context.Diners
            .FirstOrDefaultAsync(d =>
                d.CompanyId == companyId &&
                d.EmployeeNumber == request.EmployeeNumber &&
                d.IsActive);

        if (diner == null)
        {
            return new ValidateAccessResponse
            {
                Allowed = false,
                Message = "Comensal no encontrado o inactivo"
            };
        }

        var mealType = await _context.MealTypes
            .FirstOrDefaultAsync(m => m.Id == request.MealTypeId && m.CompanyId == companyId && m.IsActive);

        if (mealType == null)
        {
            return new ValidateAccessResponse
            {
                Allowed = false,
                Message = "Tipo de comida no válido"
            };
        }

        // Validar horario si existe
        var now = DateTime.Now.TimeOfDay;
        if (mealType.StartTime.HasValue && mealType.EndTime.HasValue)
        {
            if (now < mealType.StartTime.Value || now > mealType.EndTime.Value)
            {
                return new ValidateAccessResponse
                {
                    Allowed = false,
                    Message = $"Fuera de horario de {mealType.Name}"
                };
            }
        }

        // Consumos de hoy
        var today = DateTime.Now.Date;
        var consumptionsToday = await _context.DinerConsumptions
            .CountAsync(c =>
                c.CompanyId == companyId &&
                c.DinerId == diner.Id &&
                c.MealTypeId == mealType.Id &&
                c.ConsumedAt >= today &&
                c.ConsumedAt < today.AddDays(1));

        if (consumptionsToday >= mealType.MaxPerDay)
        {
            return new ValidateAccessResponse
            {
                Allowed = false,
                Message = $"Ya alcanzó el máximo de {mealType.MaxPerDay} para {mealType.Name}",
                DinerId = diner.Id,
                DinerName = $"{diner.FirstName} {diner.LastName}".Trim(),
                ConsumptionsToday = consumptionsToday,
                MaxPerDay = mealType.MaxPerDay
            };
        }

        // Registrar consumo
        var consumption = new DinerConsumption
        {
            CompanyId = companyId,
            DinerId = diner.Id,
            MealTypeId = mealType.Id,
            ProductId = request.ProductId,
            Quantity = 1,
            ConsumedAt = DateTime.Now,
            ValidatedBy = userId
        };

        _context.DinerConsumptions.Add(consumption);
        await _context.SaveChangesAsync();

        return new ValidateAccessResponse
        {
            Allowed = true,
            Message = "Acceso permitido",
            DinerId = diner.Id,
            DinerName = $"{diner.FirstName} {diner.LastName}".Trim(),
            ConsumptionsToday = consumptionsToday + 1,
            MaxPerDay = mealType.MaxPerDay
        };
    }


    public async Task<CheckInResponse> CheckInFromDeviceAsync(
    Guid companyId,
    Guid? userId,
    DeviceCheckInRequest request,
    CancellationToken cancellationToken = default)
    {
        var employeeNumber = (request.EmployeeNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(employeeNumber))
            return Deny("Número de empleado requerido");

        var punchedAt = request.PunchedAt.HasValue
            ? DateTime.SpecifyKind(request.PunchedAt.Value, DateTimeKind.Utc)
            : DateTime.Now;

        var diner = await _context.Diners
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.CompanyId == companyId &&
                d.IsActive &&
                (d.EmployeeNumber == employeeNumber || d.AccessCode == employeeNumber),
                cancellationToken);

        if (diner is null)
            return Deny("Comensal no registrado o inactivo", employeeNumber);

        var dinerName = $"{diner.FirstName} {diner.LastName}".Trim();

        // Meal type
        Domain.Entities.MealType? mealType = null;

        if (request.MealTypeId.HasValue)
        {
            mealType = await _context.MealTypes.AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.CompanyId == companyId &&
                    m.Id == request.MealTypeId.Value &&
                    m.IsActive,
                    cancellationToken);
        }
        else
        {
            var time = punchedAt.TimeOfDay;
            var meals = await _context.MealTypes.AsNoTracking()
                .Where(m => m.CompanyId == companyId && m.IsActive)
                .ToListAsync(cancellationToken);

            // Ajusta si StartTime/EndTime son TimeSpan? en tu entidad
            mealType = meals.FirstOrDefault(m =>
                IsInMealWindow(m, time));
        }

        if (mealType is null)
            return Deny("Fuera de horario de comida o sin tipo configurado", employeeNumber, dinerName);

        var dayStart = new DateTime(punchedAt.Year, punchedAt.Month, punchedAt.Day, 0, 0, 0, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var consumptionsToday = await _context.DinerConsumptions.CountAsync(c =>
            c.CompanyId == companyId &&
            c.DinerId == diner.Id &&
            c.MealTypeId == mealType.Id &&
            c.ConsumedAt >= dayStart &&
            c.ConsumedAt < dayEnd,
            cancellationToken);

        var maxPerDay = mealType.MaxPerDay <= 0 ? 1 : mealType.MaxPerDay;

        if (consumptionsToday >= maxPerDay)
        {
            return new CheckInResponse
            {
                Allowed = false,
                Message = "Ya registró su consumo permitido para este horario",
                EmployeeNumber = diner.EmployeeNumber,
                DinerName = dinerName,
                MealTypeName = mealType.Name,
                ValidAt = punchedAt,
                ConsumptionsToday = consumptionsToday,
                MaxPerDay = maxPerDay
            };
        }

        var consumption = new Domain.Entities.DinerConsumption
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            DinerId = diner.Id,
            MealTypeId = mealType.Id,
            Quantity = 1,
            ConsumedAt = punchedAt,
            CreatedAt = DateTime.Now
        };



        var folio = $"C-{punchedAt:yyyyMMdd}-{consumption.Id.ToString()[..8].ToUpperInvariant()}";

        consumption.Folio = folio;
        consumption.Printed = false;


        _context.DinerConsumptions.Add(consumption);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckInResponse
        {
            Allowed = true,
            Message = "Consumo autorizado",
            EmployeeNumber = diner.EmployeeNumber,
            DinerName = dinerName,
            MealTypeName = mealType.Name,
            ValidAt = punchedAt,
            ConsumptionsToday = consumptionsToday + 1,
            MaxPerDay = maxPerDay,
            Ticket = new TicketDto
            {
                Title = "COMEDOR",
                EmployeeNumber = diner.EmployeeNumber,
                DinerName = dinerName,
                MealTypeName = mealType.Name,
                IssuedAt = punchedAt,
                ValidFor = "1 consumo",
                Folio = folio
            }
        };
    }

    private static bool IsInMealWindow(Domain.Entities.MealType meal, TimeSpan time)
    {
        if (!meal.StartTime.HasValue || !meal.EndTime.HasValue)
            return false;

        return time >= meal.StartTime.Value && time <= meal.EndTime.Value;
    }

    private static CheckInResponse Deny(string message, string? employeeNumber = null, string? dinerName = null)
        => new()
        {
            Allowed = false,
            Message = message,
            EmployeeNumber = employeeNumber,
            DinerName = dinerName,
            ConsumptionsToday = 0,
            MaxPerDay = 0
        };

    public async Task<List<PendingTicketDto>> GetPendingTicketsAsync(
    Guid companyId,
    CancellationToken ct = default)
    {
        return await (
            from c in _context.DinerConsumptions.AsNoTracking()
            join m in _context.MealTypes.AsNoTracking() on c.MealTypeId equals m.Id
            join d in _context.Diners.AsNoTracking() on c.DinerId equals d.Id into dj
            from d in dj.DefaultIfEmpty()
            where c.CompanyId == companyId && !c.Printed && c.Folio != null
            orderby c.ConsumedAt
            select new PendingTicketDto
            {
                ConsumptionId = c.Id,
                Folio = c.Folio!,
                EmployeeNumber = d != null ? d.EmployeeNumber : "PROVISIONAL",
                DinerName = d != null
                    ? (d.FirstName + " " + (d.LastName ?? "")).Trim()
                    : (c.GuestName ?? "Invitado"),
                MealTypeName = m.Name,
                IssuedAt = c.ConsumedAt,
                ValidFor = "1 consumo",
                Title = c.IsProvisional ? "CUPÓN PROVISIONAL" : "COMEDOR"
            }
        ).ToListAsync(ct);
    }

    public async Task<bool> MarkTicketPrintedAsync(Guid companyId, Guid consumptionId, CancellationToken ct = default)
    {
        var c = await _context.DinerConsumptions
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == consumptionId, ct);
        if (c == null) return false;

        c.Printed = true;
        c.PrintedAt = DateTime.Now;
        await _context.SaveChangesAsync(ct);
        return true;
    }
    public async Task<PendingTicketDto?> GetTicketByConsumptionIdAsync(
    Guid companyId,
    Guid consumptionId,
    CancellationToken ct = default)
    {
        return await (
            from c in _context.DinerConsumptions.AsNoTracking()
            join m in _context.MealTypes.AsNoTracking() on c.MealTypeId equals m.Id
            join d in _context.Diners.AsNoTracking() on c.DinerId equals d.Id into dj
            from d in dj.DefaultIfEmpty()
            where c.CompanyId == companyId && c.Id == consumptionId && c.Folio != null
            select new PendingTicketDto
            {
                ConsumptionId = c.Id,
                Folio = c.Folio!,
                EmployeeNumber = d != null ? d.EmployeeNumber : "PROVISIONAL",
                DinerName = d != null
                    ? (d.FirstName + " " + (d.LastName ?? "")).Trim()
                    : (c.GuestName ?? "Invitado"),
                MealTypeName = m.Name,
                IssuedAt = c.ConsumedAt,
                ValidFor = "1 consumo",
                Title = c.IsProvisional ? "CUPÓN PROVISIONAL" : "COMEDOR"
            }
        ).FirstOrDefaultAsync(ct);
    }

    public async Task<CheckInResponse> CreateProvisionalCouponAsync(
    Guid companyId,
    Guid? userId,
    CreateProvisionalCouponRequest request,
    CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        Domain.Entities.MealType? mealType = null;

        if (request.MealTypeId.HasValue)
        {
            mealType = await _context.MealTypes.AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.CompanyId == companyId &&
                    m.Id == request.MealTypeId.Value &&
                    m.IsActive,
                    cancellationToken);
        }
        else
        {
            var meals = await _context.MealTypes.AsNoTracking()
                .Where(m => m.CompanyId == companyId && m.IsActive)
                .ToListAsync(cancellationToken);

            mealType = meals.FirstOrDefault(m => IsInMealWindow(m, now.TimeOfDay));
        }

        if (mealType is null)
            return Deny("Sin tipo de comida válido o fuera de horario");

        var id = Guid.NewGuid();
        var folio = $"P-{now:yyyyMMdd}-{id.ToString()[..8].ToUpperInvariant()}";
        var guestName = string.IsNullOrWhiteSpace(request.GuestName)
            ? "Invitado"
            : request.GuestName.Trim();

        var consumption = new DinerConsumption
        {
            Id = id,
            CompanyId = companyId,
            DinerId = null,
            MealTypeId = mealType.Id,
            Quantity = 1,
            ConsumedAt = now,
            CreatedAt = now,
            ValidatedBy = userId,
            Folio = folio,
            Printed = false,
            IsProvisional = true,
            GuestName = guestName,
            GuestNote = string.IsNullOrWhiteSpace(request.GuestNote)
                ? null
                : request.GuestNote.Trim(),
            Notes = "Cupón provisional"
        };

        _context.DinerConsumptions.Add(consumption);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckInResponse
        {
            Allowed = true,
            Message = "Cupón provisional autorizado",
            EmployeeNumber = "PROVISIONAL",
            DinerName = guestName,
            MealTypeName = mealType.Name,
            ValidAt = now,
            ConsumptionsToday = 1,
            MaxPerDay = 1,
            Ticket = new TicketDto
            {
                Title = "CUPÓN PROVISIONAL",
                EmployeeNumber = "PROVISIONAL",
                DinerName = guestName,
                MealTypeName = mealType.Name,
                IssuedAt = now,
                ValidFor = "1 consumo",
                Folio = folio
            }
        };
    }

    public async Task<List<DinerConsumptionDto>> GetConsumptionsAsync(
    Guid companyId,
    DateTime? from = null,
    DateTime? to = null,
    CancellationToken ct = default)
    {
        DateTime? fromDay = from?.Date;
        DateTime? toExclusive = to?.Date.AddDays(1);

        var q = _context.DinerConsumptions
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId);

        if (fromDay.HasValue)
            q = q.Where(c => c.ConsumedAt >= fromDay.Value);
        if (toExclusive.HasValue)
            q = q.Where(c => c.ConsumedAt < toExclusive.Value);

        return await q
            .OrderByDescending(c => c.ConsumedAt)
            .Select(c => new DinerConsumptionDto
            {
                Id = c.Id,
                EmployeeNumber = c.IsProvisional
                    ? (c.GuestName != null ? "PROV" : "")
                    : c.Diner!.EmployeeNumber,
                DinerName = c.IsProvisional
                    ? (c.GuestName ?? "Invitado")
                    : ((c.Diner!.FirstName + " " + (c.Diner.LastName ?? "")).Trim()),
                MealTypeName = c.MealType.Name,
                ConsumedAt = c.ConsumedAt,
                Folio = c.Folio,
                IsProvisional = c.IsProvisional,
                Printed = c.Printed,
                // DepartmentName = c.Diner != null ? c.Diner.Department!.Name : null
            })
            .ToListAsync(ct);
    }

    public async Task<byte[]> ExportConsumptionsExcelAsync(
    Guid companyId,
    DateTime from,
    DateTime to,
    decimal? pricePerDay = null,
    CancellationToken ct = default)
    {
        var price = pricePerDay
        ?? _config.GetValue<decimal?>("Reporting:PricePerMealDay")
        ?? 0m;

        if (from.Date > to.Date)
            throw new ArgumentException("La fecha inicial no puede ser mayor que la fecha final.");

        if (pricePerDay < 0)
            throw new ArgumentOutOfRangeException(
                nameof(pricePerDay),
                "El precio por día no puede ser negativo.");

        var rows = await GetConsumptionsAsync(companyId, from, to, ct);

        string? ResolveLogo(string? configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
                return null;

            if (Path.IsPathRooted(configuredPath) && File.Exists(configuredPath))
                return configuredPath;

            var absolutePath = Path.Combine(AppContext.BaseDirectory, configuredPath);
            return File.Exists(absolutePath) ? absolutePath : null;
        }

        var raudalLogoPath = ResolveLogo(_config["Reporting:LogoRaudalPath"]);
        var sinergyLogoPath = ResolveLogo(_config["Reporting:LogoSinergyPath"]);

        // Paleta de marca.
        var white = XLColor.FromHtml("#FFFFFF");
        var gray01 = XLColor.FromHtml("#EFEFEF");
        var gray02 = XLColor.FromHtml("#D9D9D8");
        var gray03 = XLColor.FromHtml("#ABABAB");
        var gray04 = XLColor.FromHtml("#666667");
        var gray05 = XLColor.FromHtml("#4C4C4D");
        var black = XLColor.FromHtml("#151111");
        var red = XLColor.FromHtml("#F33A20");
        var redSoft = XLColor.FromHtml("#FFF0ED");
        var green = XLColor.FromHtml("#23835D");
        var greenSoft = XLColor.FromHtml("#EAF6F1");
        var amber = XLColor.FromHtml("#9A6700");
        var amberSoft = XLColor.FromHtml("#FFF7D6");

        // Un empleado de nómina debe tener número y no ser provisional.
        static bool IsPayrollEmployee(string? employeeNumber, bool isProvisional) =>
            !isProvisional
            && !string.IsNullOrWhiteSpace(employeeNumber)
            && !string.Equals(employeeNumber, "PROV", StringComparison.OrdinalIgnoreCase);

        var summary = rows
            .GroupBy(row => new
            {
                EmployeeNumber = row.EmployeeNumber?.Trim() ?? string.Empty,
                DinerName = row.DinerName?.Trim() ?? string.Empty,
                row.IsProvisional
            })
            .Select(group =>
            {
                var daysUsed = group
                    .Select(item => item.ConsumedAt.Date)
                    .Distinct()
                    .Count();

                var isPayrollEmployee = IsPayrollEmployee(
                    group.Key.EmployeeNumber,
                    group.Key.IsProvisional);

                return new
                {
                    group.Key.EmployeeNumber,
                    group.Key.DinerName,
                    group.Key.IsProvisional,
                    IsPayrollEmployee = isPayrollEmployee,
                    ConsumptionTypes = string.Join(
                        ", ",
                        group
                            .Select(item => item.MealTypeName?.Trim())
                            .Where(value => !string.IsNullOrWhiteSpace(value))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(value => value)),
                    DaysUsed = daysUsed,
                    TotalConsumptions = group.Count(),
                    PayrollDiscount = isPayrollEmployee
                        ? daysUsed * pricePerDay
                        : 0m
                };
            })
            .OrderByDescending(item => item.PayrollDiscount)
            .ThenByDescending(item => item.DaysUsed)
            .ThenBy(item => item.DinerName)
            .ToList();

        var totalConsumptions = rows.Count;
        var payrollEmployees = summary.Count(item => item.IsPayrollEmployee);
        var provisionalPeople = summary.Count(item => item.IsProvisional);
        var totalPayrollDays = summary
            .Where(item => item.IsPayrollEmployee)
            .Sum(item => item.DaysUsed);
        var totalPayrollDiscount = summary.Sum(item => item.PayrollDiscount);

        using var workbook = new XLWorkbook();
        workbook.Properties.Title = "Reporte de consumos del comedor";
        workbook.Properties.Subject = "Cálculo de descuentos de nómina";
        workbook.Properties.Company = "Raudal · SinergyFood";
        workbook.Properties.Author = "InsumoPro";

        void ConfigureWorksheet(IXLWorksheet worksheet, XLPageOrientation orientation)
        {
            worksheet.Style.Font.FontName = "Aptos";
            worksheet.Style.Font.FontSize = 10;
            worksheet.Style.Font.FontColor = black;
            worksheet.ShowGridLines = false;
            worksheet.PageSetup.PageOrientation = orientation;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.FitToPages(1, 0);
            worksheet.PageSetup.Margins.Top = 0.4;
            worksheet.PageSetup.Margins.Bottom = 0.4;
            worksheet.PageSetup.Margins.Left = 0.35;
            worksheet.PageSetup.Margins.Right = 0.35;
            worksheet.PageSetup.Footer.Center.AddText(
                "Documento generado automáticamente por InsumoPro · Uso interno");
            worksheet.PageSetup.Footer.Right.AddText("Página &P de &N");
        }

        void AddBrandHeader(
            IXLWorksheet worksheet,
            int lastColumn,
            string title,
            string subtitle)
        {
            worksheet.Range(1, 1, 1, lastColumn).Merge();
            worksheet.Range(1, 1, 1, lastColumn).Style.Fill.BackgroundColor = red;
            worksheet.Row(1).Height = 7;

            worksheet.Row(2).Height = 46;

            if (raudalLogoPath is not null)
            {
                worksheet
                    .AddPicture(raudalLogoPath)
                    .MoveTo(worksheet.Cell(2, 1), 4, 4)
                    .WithSize(116, 37);
            }

            if (sinergyLogoPath is not null)
            {
                worksheet
                    .AddPicture(sinergyLogoPath)
                    .MoveTo(worksheet.Cell(2, lastColumn), 6, 2)
                    .WithSize(48, 42);
            }

            worksheet.Range(3, 1, 3, lastColumn).Merge();
            worksheet.Cell(3, 1).Value = title;
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Font.FontSize = 20;
            worksheet.Cell(3, 1).Style.Font.FontColor = black;
            worksheet.Row(3).Height = 28;

            worksheet.Range(4, 1, 4, lastColumn).Merge();
            worksheet.Cell(4, 1).Value = subtitle;
            worksheet.Cell(4, 1).Style.Font.FontSize = 11;
            worksheet.Cell(4, 1).Style.Font.FontColor = gray04;

            worksheet.Range(5, 1, 5, lastColumn).Merge();
            worksheet.Cell(5, 1).Value =
                $"Periodo: {from:dd/MM/yyyy} — {to:dd/MM/yyyy}"
                + $"   ·   Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Cell(5, 1).Style.Font.FontSize = 9;
            worksheet.Cell(5, 1).Style.Font.FontColor = gray04;

            worksheet.Range(6, 1, 6, lastColumn).Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
            worksheet.Range(6, 1, 6, lastColumn).Style.Border.BottomBorderColor = gray02;
            worksheet.Row(6).Height = 7;
        }

        void StyleTableHeader(IXLRange range)
        {
            range.Style.Fill.BackgroundColor = black;
            range.Style.Font.FontColor = white;
            range.Style.Font.Bold = true;
            range.Style.Font.FontSize = 9;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.WrapText = true;
        }

        void ApplyBodyBorders(IXLRange range)
        {
            range.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
            range.Style.Border.BottomBorderColor = gray02;
        }

        // -------------------------------------------------------------------------
        // HOJA 1: RESUMEN DE NÓMINA
        // -------------------------------------------------------------------------
        var summarySheet = workbook.Worksheets.Add("Resumen nómina");
        ConfigureWorksheet(summarySheet, XLPageOrientation.Landscape);
        summarySheet.TabColor = red;
        AddBrandHeader(
            summarySheet,
            8,
            "Consumos de nómina",
            "Resumen de días utilizados y descuento estimado del servicio de comedor");

        void AddKpi(
            int firstColumn,
            int lastColumn,
            string label,
            XLCellValue value,
            XLColor accent,
            string? numberFormat = null)
        {
            const int firstRow = 8;
            const int lastRow = 10;

            var card = summarySheet.Range(firstRow, firstColumn, lastRow, lastColumn);
            card.Style.Fill.BackgroundColor = white;
            card.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            card.Style.Border.OutsideBorderColor = gray02;

            summarySheet
                .Range(firstRow, firstColumn, firstRow, lastColumn)
                .Merge();
            summarySheet.Cell(firstRow, firstColumn).Value = label;
            summarySheet.Cell(firstRow, firstColumn).Style.Font.Bold = true;
            summarySheet.Cell(firstRow, firstColumn).Style.Font.FontSize = 8;
            summarySheet.Cell(firstRow, firstColumn).Style.Font.FontColor = gray04;
            summarySheet.Cell(firstRow, firstColumn).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            summarySheet
                .Range(firstRow + 1, firstColumn, lastRow, lastColumn)
                .Merge();
            summarySheet.Cell(firstRow + 1, firstColumn).Value = value;
            summarySheet.Cell(firstRow + 1, firstColumn).Style.Font.Bold = true;
            summarySheet.Cell(firstRow + 1, firstColumn).Style.Font.FontSize = 20;
            summarySheet.Cell(firstRow + 1, firstColumn).Style.Font.FontColor = accent;
            summarySheet.Cell(firstRow + 1, firstColumn).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            summarySheet.Cell(firstRow + 1, firstColumn).Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            if (numberFormat is not null)
                summarySheet.Cell(firstRow + 1, firstColumn).Style.NumberFormat.Format =
                    numberFormat;
        }

        AddKpi(1, 2, "CONSUMOS REGISTRADOS", totalConsumptions, black);
        AddKpi(3, 4, "EMPLEADOS EN NÓMINA", payrollEmployees, black);
        AddKpi(5, 6, "DÍAS A DESCONTAR", totalPayrollDays, green);
        AddKpi(
            7,
            8,
            "DESCUENTO ESTIMADO",
            totalPayrollDiscount,
            red,
            "$#,##0.00");

        summarySheet.Row(8).Height = 18;
        summarySheet.Row(9).Height = 18;
        summarySheet.Row(10).Height = 18;

        summarySheet.Range(12, 1, 12, 8).Merge();
        summarySheet.Cell(12, 1).Value = "CONCENTRADO PARA NÓMINA";
        summarySheet.Cell(12, 1).Style.Font.Bold = true;
        summarySheet.Cell(12, 1).Style.Font.FontSize = 10;
        summarySheet.Cell(12, 1).Style.Font.FontColor = red;
        summarySheet.Cell(12, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Left;

        const int summaryHeaderRow = 13;
        var summaryHeaders = new[]
        {
        "#",
        "No. empleado",
        "Nombre",
        "Tipo de consumo",
        "Días usados",
        "Consumos",
        "Precio por día",
        "Descuento nómina"
    };

        for (var index = 0; index < summaryHeaders.Length; index++)
            summarySheet.Cell(summaryHeaderRow, index + 1).Value = summaryHeaders[index];

        StyleTableHeader(summarySheet.Range(summaryHeaderRow, 1, summaryHeaderRow, 8));
        summarySheet.Row(summaryHeaderRow).Height = 28;

        var summaryRow = summaryHeaderRow + 1;

        foreach (var (item, index) in summary.Select((value, index) => (value, index)))
        {
            summarySheet.Cell(summaryRow, 1).Value = index + 1;
            summarySheet.Cell(summaryRow, 2).Value =
                string.IsNullOrWhiteSpace(item.EmployeeNumber)
                    ? "SIN NÚMERO"
                    : item.EmployeeNumber;
            summarySheet.Cell(summaryRow, 3).Value =
                string.IsNullOrWhiteSpace(item.DinerName)
                    ? "Sin nombre"
                    : item.DinerName;
            summarySheet.Cell(summaryRow, 4).Value =
                string.IsNullOrWhiteSpace(item.ConsumptionTypes)
                    ? "Sin clasificar"
                    : item.ConsumptionTypes;
            summarySheet.Cell(summaryRow, 5).Value = item.DaysUsed;
            summarySheet.Cell(summaryRow, 6).Value = item.TotalConsumptions;
            summarySheet.Cell(summaryRow, 7).Value =
                item.IsPayrollEmployee ? pricePerDay : 0m;
            summarySheet.Cell(summaryRow, 8).Value = item.PayrollDiscount;

            summarySheet.Cell(summaryRow, 7).Style.NumberFormat.Format = "$#,##0.00";
            summarySheet.Cell(summaryRow, 8).Style.NumberFormat.Format = "$#,##0.00";
            summarySheet.Cell(summaryRow, 8).Style.Font.Bold = true;

            summarySheet.Range(summaryRow, 1, summaryRow, 2).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            summarySheet.Range(summaryRow, 5, summaryRow, 8).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            if (item.IsProvisional)
            {
                summarySheet.Range(summaryRow, 1, summaryRow, 8).Style.Fill.BackgroundColor =
                    amberSoft;
                summarySheet.Cell(summaryRow, 2).Style.Font.FontColor = amber;
                summarySheet.Cell(summaryRow, 2).Style.Font.Bold = true;
            }
            else if (summaryRow % 2 == 0)
            {
                summarySheet.Range(summaryRow, 1, summaryRow, 8).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#FAFAFA");
            }

            ApplyBodyBorders(summarySheet.Range(summaryRow, 1, summaryRow, 8));
            summaryRow++;
        }

        var summaryLastDataRow = Math.Max(summaryRow - 1, summaryHeaderRow);
        var summaryTotalRow = summaryLastDataRow + 1;

        summarySheet.Range(summaryTotalRow, 1, summaryTotalRow, 4).Merge();
        summarySheet.Cell(summaryTotalRow, 1).Value = "TOTAL GENERAL";
        summarySheet.Cell(summaryTotalRow, 5).Value = totalPayrollDays;
        summarySheet.Cell(summaryTotalRow, 6).Value = totalConsumptions;
        summarySheet.Cell(summaryTotalRow, 7).Value = pricePerDay;
        summarySheet.Cell(summaryTotalRow, 8).Value = totalPayrollDiscount;
        summarySheet.Range(summaryTotalRow, 1, summaryTotalRow, 8).Style.Fill.BackgroundColor =
            black;
        summarySheet.Range(summaryTotalRow, 1, summaryTotalRow, 8).Style.Font.FontColor =
            white;
        summarySheet.Range(summaryTotalRow, 1, summaryTotalRow, 8).Style.Font.Bold = true;
        summarySheet.Range(summaryTotalRow, 5, summaryTotalRow, 8).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        summarySheet.Cell(summaryTotalRow, 7).Style.NumberFormat.Format = "$#,##0.00";
        summarySheet.Cell(summaryTotalRow, 8).Style.NumberFormat.Format = "$#,##0.00";
        summarySheet.Row(summaryTotalRow).Height = 24;

        if (summaryLastDataRow >= summaryHeaderRow + 1)
        {
            summarySheet
                .Range(summaryHeaderRow, 1, summaryLastDataRow, 8)
                .SetAutoFilter();
        }

        var noteRow = summaryTotalRow + 2;
        summarySheet.Range(noteRow, 1, noteRow, 8).Merge();
        summarySheet.Cell(noteRow, 1).Value =
            $"Nota: el descuento se calcula por día único de uso × {pricePerDay:C2}. "
            + $"Las {provisionalPeople} persona(s) provisional(es) se muestran para control, "
            + "pero no se incluyen en el descuento de nómina.";
        summarySheet.Cell(noteRow, 1).Style.Fill.BackgroundColor = redSoft;
        summarySheet.Cell(noteRow, 1).Style.Font.FontColor = gray05;
        summarySheet.Cell(noteRow, 1).Style.Font.FontSize = 9;
        summarySheet.Cell(noteRow, 1).Style.Alignment.WrapText = true;
        summarySheet.Row(noteRow).Height = 30;

        summarySheet.Column(1).Width = 6;
        summarySheet.Column(2).Width = 16;
        summarySheet.Column(3).Width = 30;
        summarySheet.Column(4).Width = 22;
        summarySheet.Column(5).Width = 13;
        summarySheet.Column(6).Width = 12;
        summarySheet.Column(7).Width = 16;
        summarySheet.Column(8).Width = 19;
        summarySheet.SheetView.FreezeRows(summaryHeaderRow);
        summarySheet.SheetView.FreezeColumns(3);
        summarySheet.PageSetup.PrintAreas.Clear();
        summarySheet.PageSetup.PrintAreas.Add($"A1:H{noteRow}");

        // -------------------------------------------------------------------------
        // HOJA 2: DETALLE
        // -------------------------------------------------------------------------
        var detailSheet = workbook.Worksheets.Add("Detalle");
        ConfigureWorksheet(detailSheet, XLPageOrientation.Landscape);
        detailSheet.TabColor = black;
        AddBrandHeader(
            detailSheet,
            8,
            "Detalle de consumos",
            "Registro individual de accesos al servicio de comedor");

        detailSheet.Range(8, 1, 8, 8).Merge();
        detailSheet.Cell(8, 1).Value =
            $"{totalConsumptions} registros encontrados en el periodo seleccionado";
        detailSheet.Cell(8, 1).Style.Fill.BackgroundColor = gray01;
        detailSheet.Cell(8, 1).Style.Font.FontColor = gray04;
        detailSheet.Cell(8, 1).Style.Font.Bold = true;
        detailSheet.Cell(8, 1).Style.Font.FontSize = 9;
        detailSheet.Cell(8, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Left;
        detailSheet.Row(8).Height = 22;

        const int detailHeaderRow = 10;
        var detailHeaders = new[]
        {
        "#",
        "Fecha",
        "Hora",
        "No. empleado",
        "Nombre",
        "Comida",
        "Folio",
        "Tipo"
    };

        for (var index = 0; index < detailHeaders.Length; index++)
            detailSheet.Cell(detailHeaderRow, index + 1).Value = detailHeaders[index];

        StyleTableHeader(detailSheet.Range(detailHeaderRow, 1, detailHeaderRow, 8));
        detailSheet.Row(detailHeaderRow).Height = 28;

        var detailRow = detailHeaderRow + 1;

        foreach (var (item, index) in rows
                     .OrderBy(value => value.ConsumedAt)
                     .Select((value, index) => (value, index)))
        {
            detailSheet.Cell(detailRow, 1).Value = index + 1;
            detailSheet.Cell(detailRow, 2).Value = item.ConsumedAt.Date;
            detailSheet.Cell(detailRow, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            detailSheet.Cell(detailRow, 3).Value = item.ConsumedAt;
            detailSheet.Cell(detailRow, 3).Style.DateFormat.Format = "HH:mm";
            detailSheet.Cell(detailRow, 4).Value =
                string.IsNullOrWhiteSpace(item.EmployeeNumber)
                    ? "SIN NÚMERO"
                    : item.EmployeeNumber;
            detailSheet.Cell(detailRow, 5).Value =
                string.IsNullOrWhiteSpace(item.DinerName)
                    ? "Sin nombre"
                    : item.DinerName;
            detailSheet.Cell(detailRow, 6).Value =
                string.IsNullOrWhiteSpace(item.MealTypeName)
                    ? "Sin clasificar"
                    : item.MealTypeName;
            detailSheet.Cell(detailRow, 7).Value =
                string.IsNullOrWhiteSpace(item.Folio)
                    ? "—"
                    : item.Folio;
            detailSheet.Cell(detailRow, 8).Value =
                item.IsProvisional ? "Provisional" : "Normal";

            detailSheet.Range(detailRow, 1, detailRow, 4).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            detailSheet.Cell(detailRow, 8).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            detailSheet.Cell(detailRow, 8).Style.Font.Bold = true;

            if (item.IsProvisional)
            {
                detailSheet.Cell(detailRow, 8).Style.Fill.BackgroundColor = amberSoft;
                detailSheet.Cell(detailRow, 8).Style.Font.FontColor = amber;
            }
            else
            {
                detailSheet.Cell(detailRow, 8).Style.Fill.BackgroundColor = greenSoft;
                detailSheet.Cell(detailRow, 8).Style.Font.FontColor = green;
            }

            if (detailRow % 2 == 0)
            {
                detailSheet.Range(detailRow, 1, detailRow, 7).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#FAFAFA");
            }

            ApplyBodyBorders(detailSheet.Range(detailRow, 1, detailRow, 8));
            detailRow++;
        }

        var detailLastDataRow = Math.Max(detailRow - 1, detailHeaderRow);

        if (detailLastDataRow >= detailHeaderRow + 1)
        {
            detailSheet
                .Range(detailHeaderRow, 1, detailLastDataRow, 8)
                .SetAutoFilter();
        }

        detailSheet.Column(1).Width = 6;
        detailSheet.Column(2).Width = 13;
        detailSheet.Column(3).Width = 9;
        detailSheet.Column(4).Width = 16;
        detailSheet.Column(5).Width = 29;
        detailSheet.Column(6).Width = 17;
        detailSheet.Column(7).Width = 27;
        detailSheet.Column(8).Width = 15;
        detailSheet.SheetView.FreezeRows(detailHeaderRow);
        detailSheet.SheetView.FreezeColumns(5);
        detailSheet.PageSetup.PrintAreas.Clear();
        detailSheet.PageSetup.PrintAreas.Add($"A1:H{detailLastDataRow}");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }


    static void BuildHeader(
        IXLWorksheet ws,
        string? logoRaudal,
        string? logoSinergy,
        DateTime from,
        DateTime to,
        string subtitle)
    {
        ws.Row(1).Height = 48;
        if (logoRaudal != null && File.Exists(logoRaudal))
            ws.AddPicture(logoRaudal).MoveTo(ws.Cell(1, 1)).WithSize(90, 40);
        if (logoSinergy != null && File.Exists(logoSinergy))
            ws.AddPicture(logoSinergy).MoveTo(ws.Cell(1, 3)).WithSize(90, 40);

        ws.Cell(3, 1).Value = "RAUDAL  ·  SINERGYFOOD";
        ws.Cell(3, 1).Style.Font.Bold = true;
        ws.Cell(3, 1).Style.Font.FontSize = 14;

        ws.Cell(4, 1).Value = "Control de comedores industriales";
        ws.Cell(5, 1).Value = subtitle;
        ws.Cell(5, 1).Style.Font.Bold = true;

        ws.Cell(6, 1).Value =
            $"Periodo: {from:dd/MM/yyyy} — {to:dd/MM/yyyy}";
        ws.Cell(6, 1).Style.Font.FontColor = XLColor.FromHtml("#666666");
    }

    static void StyleTableHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#241f1e");
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
}