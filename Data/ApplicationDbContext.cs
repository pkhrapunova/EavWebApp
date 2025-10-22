using EavWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EavWebApp.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options) { }

		public DbSet<EVATable> Tables { get; set; } = null!;
		public DbSet<Field> Fields { get; set; } = null!;
		public DbSet<EVAValue> Values { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<EVATable>().ToTable("RecordType");
			modelBuilder.Entity<Field>().ToTable("RecordField");
			modelBuilder.Entity<EVAValue>().ToTable("FieldValue");
		}
	}
}