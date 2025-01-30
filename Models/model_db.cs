using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace api_aguas.Models
{
    public partial class model_db : DbContext
    {
        public model_db()
            : base("name=model_db")
        {
        }

        public virtual DbSet<report_types> report_types { get; set; }
        public virtual DbSet<report> reports { get; set; }
        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<report_types>()
                .Property(e => e.phone)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<report_types>()
                .HasMany(e => e.reports)
                .WithRequired(e => e.report_types)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<report>()
                .Property(e => e.latitute)
                .HasPrecision(9, 6);

            modelBuilder.Entity<report>()
                .Property(e => e.longitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<user>()
                .HasMany(e => e.reports)
                .WithRequired(e => e.user)
                .WillCascadeOnDelete(false);
        }
    }
}
