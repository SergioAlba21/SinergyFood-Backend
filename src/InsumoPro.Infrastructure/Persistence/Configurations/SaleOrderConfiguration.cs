using InsumoPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsumoPro.Infrastructure.Persistence.Configurations
{
    public class SaleOrderConfiguration : IEntityTypeConfiguration<SaleOrder>
    {
        public void Configure(EntityTypeBuilder<SaleOrder> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Status).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Notes).HasMaxLength(256);
            builder.HasOne(x => x.Creator)
                        .WithMany()
                        .HasForeignKey(x => x.CreatedBy)
                        .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
