using Microsoft.EntityFrameworkCore;

namespace Antiban
{
	public class ApplicationDbContext : DbContext
	{
		protected override void OnConfiguring
		(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseInMemoryDatabase(databaseName: "AntibanDb");
		}

		public void ClearDb()
		{
			this.Database.EnsureDeleted();
			this.Database.EnsureCreated();
		}

		public DbSet<EventMessage> EventMessages { get; set; }

	}
}
