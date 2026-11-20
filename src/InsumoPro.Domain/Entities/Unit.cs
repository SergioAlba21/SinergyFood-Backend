namespace InsumoPro.Domain.Entities;

public class Unit
{
    public int Id { get; set; }
    public Guid? CompanyId { get; set; }                 // null = global
    public string Name { get; set; } = null!;
    public string Abbreviation { get; set; } = null!;
    public bool IsBase { get; set; }
}