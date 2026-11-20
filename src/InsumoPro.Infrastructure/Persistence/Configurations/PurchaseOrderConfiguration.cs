using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsumoPro.Infrastructure.Persistence.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("purchase_orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.OrderDate).IsRequired();
            builder.Property(x => x.ExpectedDate).IsRequired();
            builder.HasMany(x => x.Items)
                   .WithOne(i => i.PurchaseOrder)
                   .HasForeignKey(i => i.PurchaseOrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(x => x.Creator)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
