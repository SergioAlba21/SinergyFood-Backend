namespace InsumoPro.Application.Features.Diners.DTOs;

// Departments
public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
}

// MealTypes
public class MealTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int MaxPerDay { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMealTypeRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? StartTime { get; set; } // "07:00"
    public string? EndTime { get; set; }   // "10:00"
    public int MaxPerDay { get; set; } = 1;
}

// Diners
public class DinerDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AccessCode { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDinerRequest
{
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AccessCode { get; set; }
}

public class UpdateDinerRequest
{
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AccessCode { get; set; }
    public bool IsActive { get; set; } = true;
}

// Validación / consumo
public class ValidateAccessRequest
{
    public string EmployeeNumber { get; set; } = null!;
    public Guid MealTypeId { get; set; }
    public Guid? ProductId { get; set; } // platillo opcional
}

public class ValidateAccessResponse
{
    public bool Allowed { get; set; }
    public string Message { get; set; } = null!;
    public Guid? DinerId { get; set; }
    public string? DinerName { get; set; }
    public int ConsumptionsToday { get; set; }
    public int MaxPerDay { get; set; }
}

public class DinerConsumptionDto
{
    public Guid Id { get; set; }
    public Guid? DinerId { get; set; }
    public string DinerName { get; set; } = null!;
    public string EmployeeNumber { get; set; } = null!;
    public Guid MealTypeId { get; set; }
    public string MealTypeName { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ConsumedAt { get; set; }
    public string? Folio { get; set; }
    public bool Printed { get; set; }
    public DateTime? PrintedAt { get; set; }
    public bool IsProvisional { get; set; }
    public string? GuestName { get; set; }
    public string? GuestNote { get; set; }
}