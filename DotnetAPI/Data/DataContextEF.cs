using System.Data;
using Dapper;
using DotnetAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Data
{
	public class DataContextEF : DbContext
	{
		private readonly string? ConnectionString = "";

		public virtual DbSet<User> Users { get; set; }

		public DataContextEF(IConfiguration cfg)
		{
			ConnectionString = cfg.GetConnectionString("Default");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("TutorialAppSchema");

			modelBuilder.Entity<User>().ToTable("Users", "TutorialAppSchema").HasKey(u => u.UserId);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(ConnectionString, optionsBuilder => optionsBuilder.EnableRetryOnFailure());
			}
		}
	}
}