using InsumoPro.Application.Common.Interfaces;
using InsumoPro.Domain.Common;
using InsumoPro.Domain.Common.Interfaces;
using InsumoPro.Domain.Entities;
using InsumoPro.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenant;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenant) : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<MealType> MealTypes => Set<MealType>();
    public DbSet<Diner> Diners => Set<Diner>();
    public DbSet<DinerAccessRule> DinerAccessRules => Set<DinerAccessRule>();
    public DbSet<DinerConsumption> DinerConsumptions => Set<DinerConsumption>();
    public DbSet<OnboardingAnswer> OnboardingAnswers => Set<OnboardingAnswer>();
    public DbSet<OnboardingRecommendation> OnboardingRecommendations => Set<OnboardingRecommendation>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductWarehouse> ProductWarehouses => Set<ProductWarehouse>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<MrpSettings> MrpSettings => Set<MrpSettings>();
    public DbSet<MrpSuggestion> MrpSuggestions => Set<MrpSuggestion>();
    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
    public DbSet<SaleOrderItem> SaleOrderItems => Set<SaleOrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Filtro global de multi-tenancy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }

        foreach (var fk in modelBuilder.Model
    .FindEntityType(typeof(DinerConsumption))!
    .GetForeignKeys())
        {
            System.Diagnostics.Debug.WriteLine(
                $"FK: {fk.PrincipalEntityType.ClrType.Name} <- [{string.Join(",", fk.Properties.Select(p => p.Name))}]");
        }


    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder builder) where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e =>
            _currentTenant.CompanyId == null || e.CompanyId == _currentTenant.CompanyId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.Now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


}