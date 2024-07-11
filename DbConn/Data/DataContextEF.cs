using DbConn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DbConn.Data
{
	public class DataContextEF : DbContext
	{
		// private string connectionString = "Server=localhost;Database=DotNetCourseDatabase;Trusted_Connection=true;TrustServerCertificate=True;";
		private IConfiguration config;

		public DbSet<Computer>? Computer { get; set; }

		public DataContextEF(IConfiguration config)
		{
			this.config = config;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(config.GetConnectionString("Default"), options => options.EnableRetryOnFailure());
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("TutorialAppSchema");
			modelBuilder.Entity<Computer>();

			// modelBuilder.Entity<Computer>().ToTable("Computer", "TutorialAppSchema");
		}
	}
}