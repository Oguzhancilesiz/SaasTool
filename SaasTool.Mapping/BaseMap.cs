using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasTool.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Mapping
{
    public class BaseMap<T> : IEntityTypeConfiguration<T> where T : class, IEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired()
                .HasColumnType("datetime");
            builder.Property(x => x.ModifiedDate)
                 .IsRequired()
                .HasColumnType("datetime2"); ;
            builder.Property(x => x.AutoID)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .Metadata
                .SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            builder.HasQueryFilter(x => x.Status != Core.Enums.Status.Deleted);


        }
    }
}
