using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsumoPro.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<Plan> Plans { get; }
    DbSet<BusinessProfile> BusinessProfiles { get; }
    DbSet<Product> Products { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Unit> Units { get; }
    DbSet<Department> Departments { get; }
    DbSet<MealType> MealTypes { get; }
    DbSet<Diner> Diners { get; }
    DbSet<DinerAccessRule> DinerAccessRules { get; }
    DbSet<DinerConsumption> DinerConsumptions { get; }
    DbSet<OnboardingAnswer> OnboardingAnswers { get; }
    DbSet<OnboardingRecommendation> OnboardingRecommendations { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<ProductWarehouse> ProductWarehouses { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeItem> RecipeItems { get; }
    DbSet<ProductionOrder> ProductionOrders { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<MrpSettings> MrpSettings { get; }
    DbSet<MrpSuggestion> MrpSuggestions { get; }
    DbSet<SaleOrder> SaleOrders { get; }
    DbSet<SaleOrderItem> SaleOrderItems { get; }
    DbSet<Customer> Customers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
